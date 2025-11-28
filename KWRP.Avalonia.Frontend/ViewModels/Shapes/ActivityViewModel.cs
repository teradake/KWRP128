using Avalonia;
using Avalonia.Collections;
using Avalonia.Media;
using KWRP.Avalonia.Backend.Enums;
using KWRP.Avalonia.Backend.Model.Shapes.Activity;
using KWRP.Frontend.Models.Localizer;
using KWRP.Frontend.Services.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Trdk.Geometry;

namespace KWRP.Avalonia.Frontend.ViewModels.Shapes
{
    public class ActivityViewModel
    {
        private readonly ActivityModel _model;

        public ActivityViewModel(ActivityModel model)
        {
            _model = model;

            PointsWorkArea = _model.WorkArea.Shape.Points.Select(p => new Point(p.X, p.Y)).ToList();
            PointsGoalArea = _model.GoalArea.Shape.Points.Select(p => new Point(p.X, p.Y)).ToList();
            PointsOccArea = _model.OccArea.Shape.Points.Select(p => new Point(p.X, p.Y)).ToList();
        }

        public string ActivityType => _model.ActivityType.ToTag();
        public string ActivityId => _model.ActivityId.ToString();
        public string PathID => _model.GroupId.ToString();
        //public string WorkType => _model.WorkType.ToString() + $"({(_model.WorkType == 2 ? "移動" : "転圧")})";
        public string WorkType => _model.ActivityType.ToDisplayCategory();
        public string RefSpeed => _model.RefSpeed.ToString("0.00 km/h");
        public string EdgeSpeed => _model.EdgeSpeed.ToString("0.00 km/h");
        public string Dir => (Utils.RoundRadian(_model.Dir) * 180 / Math.PI).ToString("0.0°");
        public string RepeatNum => _model.RepeatNum.ToString();
        public string Comp => _model.Comp ? Localizer.Instance["Domain.Front.Yes"] : Localizer.Instance["Domain.Front.No"];
        public string EdgeFront => _model.Edge[0] ? "O" : "X";
        public string EdgeRear => _model.Edge[1] ? "O" : "X";
        public string Length => _model.Length.ToString("0.0 m");
        public string Width => _model.Width.ToString("0.0 m");

        //public string WorkTime => $"{_model.WorkTime.TotalMinutes:F1}分（累計 {WorkTimeAcc}）";
        public string WorkTime => String.Format(Localizer.Instance["Domain.Front.WorkTimeTotal"], _model.WorkTime.TotalMinutes, WorkTimeAcc);
        public string WorkTimeAcc => $" {(int)_model.AccumulatedWorkTime.TotalHours:D2}:{_model.AccumulatedWorkTime.Minutes:D2}";

        // dummy
        public IList<Point> PointsDummy => PointsWorkArea;
        public IBrush StrokeDummy => Brushes.Transparent;
        public IBrush FillDummy => Brushes.Transparent;

        // for check
        public IList<Point> PointsWorkArea { get; }
        public IList<Point> PointsGoalArea { get; }
        public IList<Point> PointsOccArea { get; }

        public IBrush StrokeWorkArea => Brushes.Magenta;
        public IBrush StrokeGoalArea => Brushes.Blue;
        public IBrush StrokeOccArea => Brushes.Red;

        public AvaloniaList<double> StrokeDashArrayWorkArea => [3, 1];
        public AvaloniaList<double> StrokeDashArrayGoalArea => [1, 1];
        public AvaloniaList<double> StrokeDashArrayOccArea => [2, 1];

    }
}
