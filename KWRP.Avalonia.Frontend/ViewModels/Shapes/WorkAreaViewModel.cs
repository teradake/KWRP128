using Avalonia;
using Avalonia.Media;
using KWRP.Avalonia.Backend.Model.Shapes.WorkArea;
using System.Collections.Generic;
using System.Linq;

namespace KWRP.Avalonia.Frontend.ViewModels.Shapes
{
    public class WorkAreaViewModel : ViewModelBase
    {
        internal WorkAreaModel Model { get; }

        public WorkAreaViewModel(WorkAreaModel model)
        {
            Model = model;

            Points = Model.Shape.Points.Select(p => new Point(p.X, p.Y)).ToList();
            Arrow = new ArrowViewModel(
                Model.Arrow.Src.X, Model.Arrow.Src.Y,
                Model.Arrow.Dst.X, Model.Arrow.Dst.Y,
                headSize: 2.0
                );
            GoalArea = new GoalAreaViewModel(Model.GoalArea);

            _isSelected = model.Attribute.IsSelected;
        }

        public IList<Point> Points { get; }
        public ArrowViewModel Arrow { get; }
        public GoalAreaViewModel GoalArea { get; }

        public IBrush Fill => IsSelected
            ? new SolidColorBrush { Color = Colors.Gold, Opacity = 0.75 }
            : Model.Attribute.IsDirectionSwap 
                ? new SolidColorBrush { Color = Colors.LightSkyBlue, Opacity = 0.75 }
                : new SolidColorBrush { Color = Colors.PowderBlue, Opacity = 0.75 };

        public IBrush Stroke => IsSelected
            ? new SolidColorBrush { Color = Colors.Red }
            : new SolidColorBrush { Color = Colors.Blue };

        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                    OnPropertyChanged(nameof(Fill));
                    OnPropertyChanged(nameof(Stroke));
                }
            }
        }
        //public bool IsSelected { get; init; }
    }

    public static class WorkAreaExtensions
    {
        public static WorkAreaViewModel ToViewModel(this WorkAreaModel model)
        {
            return new WorkAreaViewModel(model);
        }
    }
}
