using Avalonia;
using Avalonia.Media;
using KWRP.Avalonia.Backend.Model.Shapes;
using System.Collections.Generic;
using System.Linq;

namespace KWRP.Avalonia.Frontend.ViewModels.Shapes
{
    public class LaneViewModel
    {
        private readonly LaneModel _model;

        public LaneViewModel(LaneModel model)
        {
            _model = model;

            Points = _model.Shape.Points.Select(p => new Point(p.X, p.Y)).ToList();
        }

        public IList<Point> Points { get; }
        public IBrush Fill => new SolidColorBrush(Colors.LemonChiffon, 1.0);
        public IBrush Stroke => Brushes.Black;
        public string Length => _model.Length.ToString("F0.00");
    }
}
