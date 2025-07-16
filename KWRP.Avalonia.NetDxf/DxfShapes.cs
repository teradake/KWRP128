using Avalonia;
using Avalonia.Media;

namespace KWRP.Avalonia.NetDxf
{
    public abstract class DxfShape
    {
        public Color Color { get; set; } = Colors.Black;
        public double LineWeight { get; set; } = 1.0;
    }

    public class DxfLine : DxfShape
    {
        public Point Start { get; set; }
        public Point End { get; set; }
    }

    public class DxfPolyline : DxfShape
    {
        public IReadOnlyList<Point> Points { get; set; } = [];
    }

    public class DxfCircle : DxfShape
    {
        public Point Center { get; set; }
        public double Radius { get; set; }
    }

    public class DxfArc : DxfShape
    {
        public Point Center { get; set; }
        public double Radius { get; set; }
        public double StartAngle { get; set; } // Degrees
        public double EndAngle { get; set; }   // Degrees
    }

    public class DxfMText : DxfShape
    {
        public string Text { get; set; }
        public Point Position { get; set; }
        public double Height { get; set; }
        public double Rotation { get; set; }
    }

    public class DxfText : DxfShape
    {
        public string Content { get; set; } = "";
        public Point Position { get; set; }
        public double Height { get; set; }
        public double Rotation { get; set; }
    }

}
