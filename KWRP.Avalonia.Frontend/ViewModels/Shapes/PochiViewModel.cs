using Avalonia;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trdk.Geometry;

namespace KWRP.Avalonia.Frontend.ViewModels.Shapes
{
    public class PochiViewModel
    {
        private readonly Vec2 _v;
        private readonly double _len = 3;

        public PochiViewModel(Vec2 v)
        {
            _v = v;

            StartA = new Point(v.X - _len, v.Y - _len);
            EndA = new Point(v.X + _len, v.Y + _len);

            StartB = new Point(v.X - _len, v.Y + _len);
            EndB = new Point(v.X + _len, v.Y - _len);
        }

        public double X => _v.X;
        public double Y => _v.Y;
        public Point StartA { get; }
        public Point StartB { get; }
        public Point EndA { get; }
        public Point EndB { get; }
    }

    public class PochiLinkViewModel
    {
        private readonly Vec2 _a;
        private readonly Vec2 _b;

        public PochiLinkViewModel(Vec2 a, Vec2 b)
        {
            _a = a;
            _b = b;

            Start = new Point(a.X, a.Y);
            End = new Point(b.X, b.Y);
        }

        public Point Start { get; }
        public Point End { get; }
        public string Length => $"{(_b - _a).Length:f1}m";
        public Vec2 Center => (_a + _b) * 0.5;
        public IBrush Stroke => Brushes.Gray;
        public IBrush Foreground => Brushes.Red;
        public IBrush BackGround => new SolidColorBrush(Color.FromArgb(0x33, 0x55, 0x22, 0x55));    // Transparent green
    }
}
