using Avalonia;
using Avalonia.Media;
using KWRP.Avalonia.Backend.Model.Shapes;
using System.Collections.Generic;
using System.Linq;

namespace KWRP.Avalonia.Frontend.ViewModels.Shapes
{
    public class GoalAreaViewModel
    {
        private readonly GoalAreaModel _model;

        public GoalAreaViewModel(GoalAreaModel model)
        {
            _model = model;

            Points = _model.Shape.Points.Select(p => new Point(p.X, p.Y)).ToList();
        }

        public IList<Point> Points { get; }
        public IBrush Fill => Brushes.White;
        public IBrush Stroke => Brushes.Black;
    }
}
