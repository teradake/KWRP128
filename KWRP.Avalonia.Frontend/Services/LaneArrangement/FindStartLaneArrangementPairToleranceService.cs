using KWRP.Avalonia.Backend;
using KWRP.Avalonia.Backend.Enums;
using KWRP.Avalonia.Backend.Model.Modlules.LaneFilter;
using KWRP.Avalonia.Backend.Model.Modlules.LaneIntegration;
using KWRP.Avalonia.Backend.Model.Modules.LaneArrangement;
using KWRP.Avalonia.Backend.Model.Shapes;
using KWRP.Avalonia.Backend.Models;
using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Frontend.Models.Settings;
using KWRP.Avalonia.Frontend.Models.Stores;
using KWRP.Backend.Services;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Trdk.Geometry;

namespace KWRP.Avalonia.Frontend.Services.LaneArrangement
{
    public class FindStartLaneArrangementPairToleranceService : ILaneArrangementService
    {
        private readonly ParameterStore _parameterStore;
        private readonly CanvasItemStore _canvasItemStore;
        private readonly ILogService _logService;
        private readonly ILaneArrangementEvaluationService _evaluator;
        private readonly DfsLaneIntegrator _laneIntegrator;
        private readonly ApplicationStore _applicationStore;
        private readonly INotificationService _notificationService;
        private readonly ILanguageService _languageService;

        private readonly List<double> _directions = [];

        public FindStartLaneArrangementPairToleranceService(
            ParameterStore parameterStore,
            CanvasItemStore canvasItemStore,
            ILogService logService,
            ILaneArrangementEvaluationService evaluator,
            DfsLaneIntegrator laneIntegrator,
            ApplicationStore applicationStore,
            INotificationService notificationService,
            ILanguageService languageService)
        {
            _parameterStore = parameterStore;
            _canvasItemStore = canvasItemStore;
            _logService = logService;
            _evaluator = evaluator;
            _laneIntegrator = laneIntegrator;
            _applicationStore = applicationStore;
            _notificationService = notificationService;

            _logService.LogDebug("init");
            _languageService = languageService;
        }

        public void AllocateDirections(params double[] rotationDirectionsRadian)
        {
            _directions.Clear();
            _directions.AddRange(rotationDirectionsRadian);
        }

        public async Task<double> ArrangeLanesAsync(PolygonWithHoles target, bool onlyCalculateDirection = false)
        {
            var simulators = _directions
                .Select(d => target.Rotate(-d))
                .Select(p => new FindStartLaneLastLapAdjustmentArranger(p, _logService, _languageService))
                .ToArray();

            var minLength = _parameterStore.LaneChangeLength
                + (_parameterStore.FrontAllowance + _parameterStore.FrontOffset - _parameterStore.PerimeterAllowance)
                + (_parameterStore.RearAllowance + _parameterStore.RearOffset - _parameterStore.PerimeterAllowance);

            // 並列処理
            await simulators
                .ForEachAsync(async sim =>
                {
                    await Task.Run(() => sim.CreateInscribeLanes(
                        laneWidth: _parameterStore.LaneWidth,
                        lapWidth: _parameterStore.LapWidth,
                        laneMinLength: minLength,
                        leftOffset: _parameterStore.SidePrevOffset.Value,
                        rightOffset: _parameterStore.SideNextOffset.Value,
                        adjustLapWidth: _applicationStore.LaneArrangementConfigs.EnableLapAdjustment));
                    //_logService.LogWarn($"finished {sim.TargetPolygon.Shell.Points[0].ToString()}");
                }, concurrency: 20);

            var scores = simulators
                .Select(sim => _evaluator.Score(sim.TargetPolygon, sim.Lanes))
                .ToArray();

            if (simulators.Length == 0 
                || scores.Length == 0 
                || simulators.Length != scores.Length
                || simulators.Length != _directions.Count)
            {
                _logService.LogError($"workers: {simulators.Length}, scores: {scores.Length}");
                throw new Exception($"レーンペアリング割計算時に例外が発生しました。{nameof(AllocateDirections)}, {nameof(ArrangeLanesAsync)}が実行されていない可能性があります。");
            }

            // 最適なやつを選ぶ
            var best = -1;
            var bestScore = KWRPConstants.C_INF;
            for (int i = 0; i < simulators.Length; ++i)
            {
                if (scores[i] < bestScore)
                {
                    best = i;
                    bestScore = scores[i];
                }
            }

            if (onlyCalculateDirection)
            {
                return _directions[best];
            }


            // レーン割結果
            var lanes = simulators[best]
                .Lanes
                .AdjustLength(
                    shortenFront: _parameterStore.FrontAllowance + _parameterStore.FrontOffset - _parameterStore.PerimeterAllowance,
                    shortenRear: _parameterStore.RearAllowance + _parameterStore.RearOffset - _parameterStore.PerimeterAllowance,
                    headToRight: _parameterStore.RollerHeadType.Value == Backend.Enums.RollerHeadingType.Right)
                .Where(lane => lane.Ymax - lane.Ymin >= _parameterStore.LaneChangeLength)
                .Select(box => box.ToPolygon()).ToArray();
            var pairedLanes = _laneIntegrator.Integrate(
                orthogonalLanes: lanes, 
                minPairCount: _parameterStore.PairCountMin,
                maxPairCount: _parameterStore.PairCountMax,
                laneGapToleranceFront: _applicationStore.LaneArrangementConfigs.LaneGapToleranceFront,
                laneGapToleranceRear: _applicationStore.LaneArrangementConfigs.LaneGapToleranceRear,
                headToRight: _parameterStore.RollerHeadType.Value == Backend.Enums.RollerHeadingType.Right);
            var result = CreatedLaneResult.By(
                targetPolygon: target.Rotate(_directions[best]),
                lanes: lanes.Select(lane => LaneModel.CreateByOrthogonal(lane, _directions[best])),
                pairedLanes: pairedLanes.Select(p => PairedLaneModel.CreateByOrthogonalWithArrow(
                    orthogonalLanes: p,
                    progressDirectionRad: _directions[best],
                    headToRight: _parameterStore.RollerHeadType.Value == Backend.Enums.RollerHeadingType.Right)));

            // レーンが生成できなかった場合は通知
            if (lanes.Length == 0 || !pairedLanes.Any())
            {
                //_notificationService.Notify(KWRPNotification.Create(
                //    message: $"ローラ作業可能長さが確保できないためレーン生成ができません。\n（作業進捗方向: {(180 / Math.PI *_directions[best]):F2}°）",
                //    type: NotifyMessageType.Warn,
                //    duration: 3.0));
                
                _notificationService.Notify(KWRPNotification.Create(
                    message: string.Format(_languageService.GetString("Domain.Front.RollerWorkLengthNotSecured"), 180 / Math.PI * _directions[best]),
                    type: NotifyMessageType.Warn,
                    duration: 4.0));
            }

            // 結果を格納する
            _canvasItemStore.CreatedLaneResults.Add(result);
            return _directions[best];
        }
    }

    public static class EnumerableExtensions
    {
        public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> action, int concurrency, CancellationToken cancellationToken = default(CancellationToken), bool configureAwait = false)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (action == null) throw new ArgumentNullException("action");
            if (concurrency <= 0) throw new ArgumentOutOfRangeException("concurrencyは1以上の必要があります");

            using (var semaphore = new SemaphoreSlim(initialCount: concurrency, maxCount: concurrency))
            {
                var exceptionCount = 0;
                var tasks = new List<Task>();

                foreach (var item in source)
                {
                    if (exceptionCount > 0) break;
                    cancellationToken.ThrowIfCancellationRequested();

                    await semaphore.WaitAsync(cancellationToken).ConfigureAwait(configureAwait);
                    var task = action(item).ContinueWith(t =>
                    {
                        semaphore.Release();

                        if (t.IsFaulted)
                        {
                            Interlocked.Increment(ref exceptionCount);
                            throw t.Exception;
                        }
                    });
                    tasks.Add(task);
                }

                await Task.WhenAll(tasks.ToArray()).ConfigureAwait(configureAwait);
            }
        }
    }
}
