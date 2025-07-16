using Avalonia;
using Avalonia.Media;
using KWRP.Avalonia.Backend.Model.Shapes;
using System.Collections.Generic;
using System.Linq;
using Trdk.Geometry;

namespace KWRP.Avalonia.Frontend.ViewModels.Shapes
{
    public class DragRectViewModel
    {
        private readonly DragRect _rect;

        public Vec2 Start => _rect.Start;
        public IList<Point> Points { get; }
        public IBrush Fill { get; }
        public IBrush Stroke { get; }
        public bool IsVisible => _rect.IsVisible;


        public DragRectViewModel(DragRect rect)
        {
            _rect = rect;

            Points = rect.Shape?.Points.Select(p => new Point(p.X, p.Y)).ToList()
                ?? [];

            Fill = rect.ToLeft
                ? new SolidColorBrush(Color.FromArgb(0x33, 0xFF, 0x32, 0x32))
                : new SolidColorBrush(Color.FromArgb(0x33, 0x32, 0x32, 0xFF));

            Stroke = rect.ToLeft
                ? new SolidColorBrush(Color.FromArgb(0xEE, 0xFF, 0x32, 0x32))
                : new SolidColorBrush(Color.FromArgb(0xEE, 0x32, 0x32, 0xFF));
        }
    }
}
