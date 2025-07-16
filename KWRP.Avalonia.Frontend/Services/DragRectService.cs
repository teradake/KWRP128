using KWRP.Avalonia.Backend.Model.Shapes;
using KWRP.Avalonia.Backend.Model.Shapes.WorkArea;
using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Frontend.Models.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using Trdk.Geometry;
using Trdk.Geometry.NTS;

namespace KWRP.Avalonia.Frontend.Services
{
    public interface IDragRectService
    {
        void SetDragRectAt(double x, double y);
        void ReleaseDragRectAt(double x, double y);
        void UpdateDragRectAt(double x, double y);
    }

    public class DragRectService : IDragRectService
    {
        private readonly CanvasItemStore _canvasItemStore;
        private readonly CanvasStateStore _canvasStateStore;
        private readonly WorkAreaStore _workAreaStore;
        private readonly ILogService _logService;
        private readonly ApplicationStore _applicationStore;

        private Vec2 start;

        public DragRectService(CanvasItemStore canvasItemStore, CanvasStateStore canvasStateStore, ILogService logService, WorkAreaStore workAreaStore, ApplicationStore applicationStore)
        {
            _canvasItemStore = canvasItemStore;
            _canvasStateStore = canvasStateStore;
            _logService = logService;
            _workAreaStore = workAreaStore;

            start = Vec2.Origin;

            _logService.LogDebug("init");
            _applicationStore = applicationStore;
        }

        public void ReleaseDragRectAt(double x, double y)
        {
            try
            {
                if (_workAreaStore.WorkAreas.Count == 0)
                {
                    return;
                }

                if (_applicationStore.EditorMode.Value != Backend.Enums.EditorMode.WorkAreaDesign)
                {
                    return;
                }

                bool isPoint = false;
                var rect = _canvasItemStore.DragRect.CurrentValue.Shape;
                if (rect == null)
                {
                    // クリック。微小な矩形が得られたと想定する
                    rect = new BoundingBox(new Vec2(x, y), new Vec2(x + 1e-3, y + 1e-3)).ToPolygon();
                    isPoint = true;
                }

                var selected = new List<WorkAreaModel>();
                if (isPoint || !_canvasItemStore.DragRect.CurrentValue.ToLeft)
                {
                    foreach (var workArea in _workAreaStore.WorkAreas.Select(kv => kv.Key))
                    {
                        if (rect.IsIntersect(workArea.Shape))
                        {
                            selected.Add(workArea);
                        }
                    }
                }
                else
                {
                    foreach (var workArea in _workAreaStore.WorkAreas.Select(kv => kv.Key))
                    {
                        if (Utils.IsZero(rect.GetIntersectionArea(workArea.Shape) - workArea.Shape.Area))
                        {
                            selected.Add(workArea);
                        }
                    }
                }

                foreach (var workArea in selected)
                {
                    _workAreaStore.WorkAreas[workArea].IsSelected ^= true;
                    if (_workAreaStore.SelectedWorkAreas.Contains(workArea))
                    {
                        _workAreaStore.SelectedWorkAreas.Remove(workArea);
                    }
                    else
                    {
                        _workAreaStore.SelectedWorkAreas.Add(workArea);
                    }
                }

                _logService.LogDebug($"release dragrect at ({x}, {y})");
                _logService.LogDebug($"selected ({selected.Count})");
            }
            catch (Exception e)
            {
                _logService.LogWarn("drag rect error" + e.Message);
            }
            finally
            {
                _canvasItemStore.DragRect.Value = new DragRect(Vec2.Origin, Vec2.Origin, isVisible: false);
            }
        }

        public void SetDragRectAt(double x, double y)
        {
            _logService.LogDebug($"start dragrect at ({x}, {y})");
            start = new Vec2(x, y);
        }

        public void UpdateDragRectAt(double x, double y)
        {
            _canvasItemStore.DragRect.Value = new DragRect(
                start: start,
                end: new Vec2(x, y),
                canvasRotationRadian: _canvasStateStore.Rot.CurrentValue.RadianValue,
                isVisible: true);
        }
    }
}
