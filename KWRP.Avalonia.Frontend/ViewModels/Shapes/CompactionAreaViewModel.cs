using Avalonia;
using Avalonia.Collections;
using Avalonia.Media;
using System.Collections.Generic;
using System.Linq;
using Trdk.Geometry;

namespace KWRP.Avalonia.Frontend.ViewModels.Shapes
{
    public class CompactionAreaViewModel
    {
        private readonly Polygon _poly;

        public CompactionAreaViewModel(Polygon poly, bool isHole)
        {
            _poly = poly;

            Points = poly.Points.Select(p => new Point(p.X, p.Y)).ToList();

            Fill = isHole
                ? Brushes.White
                : new LinearGradientBrush
                {
                    GradientStops = new GradientStops
                    {
                        new(Color.FromRgb(0xe2, 0xf0, 0xd9), 0.0),
                        new(Color.FromRgb(0x8a, 0xc2, 0x64), 1.0)
                    },
                    StartPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
                    EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
                };
        }

        public IList<Point> Points { get; }
        public IBrush Fill { get; }
        public IBrush Stroke => Brushes.Black;
    }
}
