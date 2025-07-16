using Avalonia;
using Avalonia.Media;
using KWRP.Avalonia.Backend.Model.Shapes;
using System.Collections.Generic;
using System.Linq;

namespace KWRP.Avalonia.Frontend.ViewModels.Shapes
{
    public class PairedLaneViewModel
    {
        private readonly PairedLaneModel _model;

        public PairedLaneViewModel(PairedLaneModel model)
        {
            _model = model;

            Points = _model.Shape.Points.Select(p => new Point(p.X, p.Y)).ToList();
            Arrow = new ArrowViewModel(
                model.Arrow!.Src.X,
                model.Arrow!.Src.Y,
                model.Arrow!.Dst.X,
                model.Arrow!.Dst.Y,
                isVisible: true,
                headSize: 1.8
                );
        }

        public IList<Point> Points { get; }
        public IBrush Fill => new SolidColorBrush(Color.FromRgb(0xff, 0xff, 0xff), 0.8);
        public IBrush Stroke => Brushes.Black;
        public ArrowViewModel Arrow { get; }
    }
}
