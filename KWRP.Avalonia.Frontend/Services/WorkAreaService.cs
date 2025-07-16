using KWRP.Avalonia.Backend.Model.Shapes.WorkArea;
using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Frontend.Models;
using KWRP.Avalonia.Frontend.Models.Stores;
using KWRP.Avalonia.Frontend.ViewModels.Shapes;
using System;
using System.Linq;
using Trdk.Geometry;

namespace KWRP.Avalonia.Frontend.Services
{
    public class WorkAreaService : IWorkAreaService
    {
        private readonly WorkAreaStore _workAreaStore;
        private readonly CanvasItemStore _canvasItemStore;
        private readonly ParameterStore _parameterStore;
        private readonly MachineStore _machineStore;
        private readonly ILogService _logService;

        public WorkAreaService(
            WorkAreaStore workAreaStore, 
            CanvasItemStore canvasItemStore,
            ILogService logService,
            ParameterStore parameterStore,
            MachineStore machineStore   )
        {
            _workAreaStore = workAreaStore;
            _canvasItemStore = canvasItemStore;
            _logService = logService;
            _parameterStore = parameterStore;
            _machineStore = machineStore;

            _logService.LogDebug("init");
        }

        public void ClearSelectedWorkAreas()
        {
            foreach (var workArea in _workAreaStore.SelectedWorkAreas)
            {
                if (_workAreaStore.WorkAreas.ContainsKey(workArea))
                {
                    _workAreaStore.WorkAreas[workArea].IsSelected = false;
                }
            }
            _workAreaStore.SelectedWorkAreas.Clear();
        }

        public void InitializeWorkAreas()
        {
            _workAreaStore.WorkAreas.Clear();

            foreach (var pairedLane in _canvasItemStore.PairedLanes)
            {
                try
                {
                    var headToRight = _parameterStore.RollerHeadType == Backend.Enums.RollerHeadingType.ToRight;
                    var frontMargin = _parameterStore.FrontAllowance + _parameterStore.FrontOffset - _parameterStore.PerimeterAllowance;
                    var rearMargin = _parameterStore.RearAllowance + _parameterStore.RearOffset - _parameterStore.PerimeterAllowance;
                    if (headToRight)
                        (frontMargin, rearMargin) = (rearMargin, frontMargin);

                    var lanes = pairedLane.OrthogonalLanes
                        .Select(lane => lane.AaBB)
                        .Select(box => new BoundingBox { Xmin = box.Xmin, Xmax = box.Xmax, Ymin = box.Ymin - rearMargin, Ymax = box.Ymax + frontMargin })
                        .Select(box => box.ToPolygon())
                        .ToArray();

                    var workArea = new WorkAreaModel(
                        orthogonalLanesOriginal: lanes,
                        attribute: WorkAreaAttribute.Default,
                        headToRight: headToRight,
                        progressDirectionRad: _parameterStore.ProgresssDirectionRadian.Value,
                        wheelType: _machineStore.CurrentRoller.MachineInfo.RollerWheelType,
                        wheelbase: _machineStore.CurrentRoller.MachineInfo.Wheelbase,
                        frontMargin: _parameterStore.FrontAllowance + _parameterStore.FrontOffset - _parameterStore.PerimeterAllowance,
                        rearMargin: _parameterStore.RearAllowance + _parameterStore.RearOffset - _parameterStore.PerimeterAllowance);
                    _workAreaStore.WorkAreas.Add(workArea, workArea.ToViewModel());
                }
                catch (Exception e)
                {
                    _logService.LogError("Failed to create work area", e);
                }
            }
            _canvasItemStore.LanesUpdated = false;
        }

        public void Undo()
        {
            ClearSelectedWorkAreas();

            if (!_workAreaStore.History.Any())
            {
                return;
            }

            var hist = _workAreaStore.History.RemoveLast();
            foreach (var area in hist.Added)
            {
                if (_workAreaStore.WorkAreas.ContainsKey(area))
                {
                    _workAreaStore.WorkAreas.Remove(area);
                }
            }
            foreach (var area in hist.Removed)
            {
                _workAreaStore.WorkAreas.Add(area, area.ToViewModel());
                _workAreaStore.WorkAreas[area].IsSelected = false;
            }
        }

        public void UpdateWorkAreas(Func<WorkAreaModel, WorkAreaModel> workAreaUpdateFunc)
        {
            foreach (var selectedWorkArea in _workAreaStore.SelectedWorkAreas)
            {
                _workAreaStore.WorkAreas.Remove(selectedWorkArea);
            }

            var newWorkAreas = _workAreaStore
                .SelectedWorkAreas
                .Select(workAreaUpdateFunc)
                .ToList();

            var hist = new WorkAreaHistory
            {
                Added = newWorkAreas.ToList(), 
                Removed = _workAreaStore.SelectedWorkAreas.ToList(),
            };
            _workAreaStore.History.AddLast(hist);

            _workAreaStore.SelectedWorkAreas.Clear();
            foreach (var newWorkArea in newWorkAreas)
            {
                _workAreaStore.WorkAreas.Add(newWorkArea, newWorkArea.ToViewModel());
                _workAreaStore.SelectedWorkAreas.Add(newWorkArea);
            }
        }

 
    }
}
