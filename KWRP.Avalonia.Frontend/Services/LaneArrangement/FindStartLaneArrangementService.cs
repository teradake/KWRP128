using KWRP.Avalonia.Backend;
using KWRP.Avalonia.Backend.Model.Modlules.LaneFilter;
using KWRP.Avalonia.Backend.Model.Modlules.LaneIntegration;
using KWRP.Avalonia.Backend.Model.Modules.LaneArrangement;
using KWRP.Avalonia.Backend.Model.Shapes;
using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Frontend.Models.Stores;
using KWRP.Backend.Services;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trdk.Geometry;

namespace KWRP.Avalonia.Frontend.Services.LaneArrangement
{
    public class FindStartLaneArrangementService : ILaneArrangementService
    {
        private readonly ParameterStore _parameterStore;
        private readonly CanvasItemStore _canvasItemStore;
        private readonly ILogService _logService;
        private readonly ILaneArrangementEvaluationService _evaluator;
        private readonly ILanguageService _languageService;

        private readonly List<double> _directions = [];

        public FindStartLaneArrangementService(
            ParameterStore parameterStore,
            CanvasItemStore canvasItemStore,
            ILogService logService,
            ILaneArrangementEvaluationService evaluator,
            ILanguageService languageService)
        {
            _parameterStore = parameterStore;
            _canvasItemStore = canvasItemStore;
            _logService = logService;
            _evaluator = evaluator;

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
                .Select(p => new FindStartLaneUniformLapAdjustmentArranger(p, _logService, _languageService))
                .ToArray();

            
            var minLength = _parameterStore.LaneChangeLength
                + (_parameterStore.FrontAllowance + _parameterStore.FrontOffset - _parameterStore.PerimeterAllowance)
                + (_parameterStore.RearAllowance + _parameterStore.RearOffset - _parameterStore.PerimeterAllowance);

            // R3.toObservableをつかうのもよいかも
            // 並列処理も検討する
            var scores = simulators
                .Select(sim =>
                {
                    sim.CreateInscribeLanes(
                        laneWidth: _parameterStore.LaneWidth,
                        lapWidth: _parameterStore.LapWidth,
                        laneMinLength: minLength,
                        leftOffset: _parameterStore.SidePrevOffset.Value,
                        rightOffset: _parameterStore.SideNextOffset.Value);
                    return _evaluator.Score(sim.TargetPolygon, sim.Lanes);
                })
                .ToArray();

            if (simulators.Length == 0 
                || scores.Length == 0 
                || simulators.Length != scores.Length
                || simulators.Length != _directions.Count)
            {
                _logService.LogError($"workers: {simulators.Length}, scores: {scores.Length}");
                throw new Exception($"レーンペアリング割計算時に例外が発生しました。{AllocateDirections}, {ArrangeLanesAsync}が実行されていない可能性があります。");
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

            // レーン割結果
            var lanes = simulators[best]
                .Lanes
                .AdjustLength(
                    shortenFront: _parameterStore.FrontAllowance + _parameterStore.FrontOffset - _parameterStore.PerimeterAllowance,
                    shortenRear: _parameterStore.RearAllowance + _parameterStore.RearOffset - _parameterStore.PerimeterAllowance,
                    headToRight: _parameterStore.RollerHeadType == Backend.Enums.RollerHeadingType.ToRight)
                .Where(lane => lane.Ymax - lane.Ymin >= _parameterStore.LaneChangeLength)
                .Select(box => box.ToPolygon()).ToArray();
            var pairedLanes = NaiveDfsLaneIntegrator.Integrate(
                orthogonalLanes: lanes, 
                minPairCount: _parameterStore.PairCountMin,
                maxPairCount: _parameterStore.PairCountMax);
            var result = CreatedLaneResult.By(
                targetPolygon: target.Rotate(_directions[best]),
                lanes: lanes.Select(lane => LaneModel.CreateByOrthogonal(lane, _directions[best])),
                pairedLanes: pairedLanes.Select(p => PairedLaneModel.CreateByOrthogonalWithArrow(
                    orthogonalLanes: p,
                    progressDirectionRad: _directions[best],
                    headToRight: _parameterStore.RollerHeadType == Backend.Enums.RollerHeadingType.ToRight)));
                    

            // 結果を格納する
            _canvasItemStore.CreatedLaneResults.Add(result);
            return _directions[best];
        }
    }
}
