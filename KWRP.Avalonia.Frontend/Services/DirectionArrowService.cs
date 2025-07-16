using KWRP.Avalonia.Backend;
using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Frontend.Models.Stores;
using KWRP.Avalonia.Frontend.ViewModels.Shapes;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KWRP.Avalonia.Frontend.Services
{
    public interface IDirectionArrowService
    {
        void SetArrowAt(double x, double y);
        Task ReleaseArrowAt(double x, double y);
        void UpdateArrowAt(double x, double y);
    }


    public class DirectionArrowService : IDirectionArrowService
    {
        private readonly CanvasItemStore _canvasItemStore;
        private readonly CanvasStateStore _canvasStateStore;
        private readonly ParameterStore _parameterStore;
        private readonly ILogService _logService;
        private readonly ApplicationStore _applicationStore;
        private readonly ILaneArrangementService _laneArrangementService;

        private double startX;
        private double startY;

        public DirectionArrowService(
            CanvasItemStore canvasItemStore,
            ParameterStore parameterStore,
            CanvasStateStore canvasStateStore,
            ILogService logService,
            ApplicationStore applicationStore,
            ILaneArrangementService laneArrangementService)
        {
            _canvasItemStore = canvasItemStore;
            _parameterStore = parameterStore;
            _canvasStateStore = canvasStateStore;
            _logService = logService;
            _applicationStore = applicationStore;
            _laneArrangementService = laneArrangementService;

            _logService.LogDebug("init");
        }

        public async Task ReleaseArrowAt(double x, double y)
        {
            var arrow = new ArrowViewModel(startX, startY, x, y, isVisible: false);
            _canvasItemStore.DirectionArrow.Value = arrow;
            if (arrow.Length <= KWRPConstants.C_EPS || _canvasItemStore.TargetPolygons.Count == 0)
            {
                return;
            }

            _logService.LogDebug($"release arrow at ({x}, {y})");
            _logService.LogDebug($"direction: {arrow.Direction}");

            if (!_applicationStore.LaneArrangementConfigs.EnableOptimize)
            {
                _parameterStore.ProgresssDirectionRadian.Value = arrow.Direction.RadianValue;
                return;
            }

            var ranges = _applicationStore.LaneArrangementConfigs.RangeAngleDegree;
            var interval = _applicationStore.LaneArrangementConfigs.IntervalAngleDegree;
            if (interval <= KWRPConstants.C_EPS)
            {
                throw new System.Exception("探索刻み幅が小さすぎます");
            }

            int n = Math.Min((int)(ranges / interval), 100000);
            var base_direction = arrow.Direction.DegreeValue - _canvasStateStore.Rot.CurrentValue.DegreeValue;
            var direction_plus = Enumerable.Range(0, n).Select(i => base_direction + i * interval);
            var direction_minus = Enumerable.Range(1, n).Select(i => base_direction - i * interval);
            var directions = direction_plus.Concat(direction_minus).Select(d => Math.PI * d / 180.0);

            var area = _canvasItemStore.TargetPolygons.MaxBy(area => area.Area);
            _laneArrangementService.AllocateDirections(directions.ToArray());
            _parameterStore.ProgresssDirectionRadian.Value = await _laneArrangementService.ArrangeLanesAsync(area, true);

            _logService.LogDebug($"optimized direction: {_parameterStore.ProgresssDirectionRadian.Value * 180 / Math.PI}");
        }

        public void SetArrowAt(double x, double y)
        {
            _logService.LogDebug($"set arrow at ({x}, {y})");

            startX = x;
            startY = y;
        }

        public void UpdateArrowAt(double x, double y)
        {
            _canvasItemStore.DirectionArrow.Value = new ArrowViewModel(startX, startY, x, y);
        }
    }
}
