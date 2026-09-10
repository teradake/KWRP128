using KWRP.Avalonia.Backend.Enums;
using KWRP.Avalonia.Backend.Model.Shapes.WorkArea;
using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Frontend.Models;
using KWRP.Avalonia.Frontend.Models.Stores;
using KWRP.Avalonia.Frontend.ViewModels.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trdk.Geometry;

namespace KWRP.Avalonia.Frontend.Services
{
    public class WorkAreaCutService : IWorkAreaCutService
    {
        private readonly IWorkAreaService _workAreaService;
        private readonly ILogService _logService;
        private readonly WorkAreaStore _workAreaStore;
        private readonly ParameterStore _parameterStore;
        private readonly MachineStore _machineStore;

        public WorkAreaCutService(WorkAreaStore workAreaStore, ParameterStore parameterStore, MachineStore machineStore, ILogService logService, IWorkAreaService workAreaService)
        {
            _workAreaStore = workAreaStore;
            _parameterStore = parameterStore;
            _machineStore = machineStore;
            _logService = logService;
            _workAreaService = workAreaService;
        }

        public void InitializeCutProperties(int count)
        {
            foreach (var prop in _workAreaStore.CutProperties)
            {
                prop.PropertyChanged -= OnCutPropertyChanged;
            }

            _workAreaStore.CutProperties.Clear();
            for (int i = 0; i < count; i++)
            {
                _workAreaStore.CutProperties.Add(new Models.CutProperty()
                {
                    Percentage = 100.0 / count,
                });
            }

            foreach (var prop in _workAreaStore.CutProperties)
            {
                prop.PropertyChanged += OnCutPropertyChanged;
            }
        }

        private void OnCutPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RefleshCutProperties();
        }

        public void RefleshCutProperties()
        {
            foreach (var prop in _workAreaStore.CutProperties)
            {
                prop.PropertyChanged -= OnCutPropertyChanged;
            }

            double sum = _workAreaStore.CutProperties.Sum(p => p.Ratio);
            foreach (var p in _workAreaStore.CutProperties)
            {
                p.Percentage = 100.0 * p.Ratio / sum;
            }
            //var newItems = _workAreaStore.CutProperties.Select(p => new CutProperty
            //{
            //    Ratio = p.Ratio,
            //    Percentage = 100.0 * p.Ratio / sum,
            //});

            //_workAreaStore.CutProperties.Clear();
            //_workAreaStore.CutProperties.AddRange(newItems);

            foreach (var prop in _workAreaStore.CutProperties)
            {
                prop.PropertyChanged += OnCutPropertyChanged;
            }
        }

        public void Cut()
        {
            try
            {
                foreach (var workArea in _workAreaStore.SelectedWorkAreas)
                {
                    if (_workAreaStore.WorkAreas.ContainsKey(workArea))
                    {
                        _workAreaStore.WorkAreas[workArea].IsSelected = false;
                    }
                    _workAreaStore.WorkAreas.Remove(workArea);
                }

                var hist = new WorkAreaHistory();
                double[] ratio = _workAreaStore.CutProperties.Select(p => (double)p.Ratio).Reverse().ToArray();
                double sum = ratio.Sum();
                foreach (var workArea in _workAreaStore.SelectedWorkAreas)
                {
                    var ok = true;
                    var lower = workArea.OrthogonalLanesOriginal.Max(lane => lane.AaBB.Ymin);
                    var upper = workArea.OrthogonalLanesOriginal.Min(lane => lane.AaBB.Ymax);
                    var length = upper - lower;

                    double from = lower, to;
                    var newWorkAreas = new List<WorkAreaModel>();
                    for (int i = 0; i < ratio.Length; ++i)
                    {
                        to = from + length * ratio[i] / sum;

                        //if (i == 0) from = double.MinValue;
                        //if (i == ratio.Length - 1) to = double.MaxValue;
                        if (i != ratio.Length - 1) to += _parameterStore.LapLengthActual * 0.5;
                        if (i != 0) from -= _parameterStore.LapLengthActual * 0.5;

                        if (to - from < _parameterStore.LaneChangeLength)
                        {
                            ok = false;
                            break;
                        }
                        var clamped = workArea
                            .OrthogonalLanesOriginal
                            .Select(lane => lane
                                .AaBB
                                .ClampByY(ymin: i == 0 ? double.MinValue : from, 
                                          ymax: i == ratio.Length - 1 ? double.MaxValue : to)
                                .ToPolygon())
                            .ToArray();

                        var attr = workArea.Attribute.Copy();
                        var isUp = workArea.IsEdgeFront;
                        var isDown = workArea.IsEdgeRear;
                        if (workArea.Orientation == WorkAreaOrientation.Down)
                            (isUp, isDown) = (isDown, isUp);

                        newWorkAreas.Add(new WorkAreaModel(
                            orthogonalLanesOriginal: clamped,
                            attribute: attr.SetIsSelected(false),
                            headToRight: _parameterStore.RollerHeadType.Value == Backend.Enums.RollerHeadingType.ToRight,
                            progressDirectionRad: _parameterStore.ProgresssDirectionRadian.Value,
                            wheelType: _machineStore.CurrentRoller.MachineInfo.RollerWheelType,
                            wheelbase: _machineStore.CurrentRoller.MachineInfo.Wheelbase,
                            frontMargin: _parameterStore.FrontAllowance + _parameterStore.FrontOffset - _parameterStore.PerimeterAllowance,
                            rearMargin: _parameterStore.RearAllowance + _parameterStore.RearOffset - _parameterStore.PerimeterAllowance,
                            isEdgeUp: (i == ratio.Length - 1) && isUp,
                            isEdgeDown: i == 0 && isDown));

                        if (i != ratio.Length - 1) to -= _parameterStore.LapLengthActual * 0.5;
                        if (i != 0) from += _parameterStore.LapLengthActual * 0.5;

                        from = to;
                    }

                    if (!ok)
                    {
                        _workAreaStore.WorkAreas.Add(workArea, workArea.ToViewModel());
                        continue;
                    }
                    foreach (var newWorkArea in newWorkAreas)
                    {
                        _workAreaStore.WorkAreas.Add(newWorkArea, newWorkArea.ToViewModel());
                        hist.Added.Add(newWorkArea);
                    }
                    hist.Removed.Add(workArea);
                }

                _workAreaStore.History.AddLast(hist);
            }
            catch (Exception ex)
            {
                _logService.LogError("作業エリア分割失敗", ex);
            }
            finally
            {
                _workAreaService.ClearSelectedWorkAreas();
            }
        }
    }
}
