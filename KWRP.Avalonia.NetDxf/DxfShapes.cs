using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using KWRP.NetDxf;

namespace KWRP.Avalonia.NetDxf
{
    public abstract class DxfShape
    {
        public Color Color { get; set; } = Colors.Black;
        public double LineWeight { get; set; } = 1.0;
    }

    internal class DxfLine : DxfShape
    {
        public Point Start { get; set; }
        public Point End { get; set; }
    }

    internal class DxfPolyline : DxfShape
    {
        public IReadOnlyList<Point> Points { get; set; } = [];
    }

    internal class DxfCircle : DxfShape
    {
        public Point Center { get; set; }
        public double Radius { get; set; }
    }

    internal class DxfArc : DxfShape
    {
        public Point Center { get; set; }
        public double Radius { get; set; }
        public double StartAngle { get; set; } // Degrees
        public double EndAngle { get; set; }   // Degrees
    }

    internal class DxfMText : DxfShape
    {
        public string Text { get; set; } = string.Empty;
        public Point Position { get; set; }
        public double Height { get; set; }
        public double Rotation { get; set; }
    }

    internal class DxfText : DxfShape
    {
        public string Content { get; set; } = string.Empty;
        public Point Position { get; set; }
        public double Height { get; set; }
        public double Rotation { get; set; }
    }

    internal class DxfImage : DxfShape
    {
        public string FilePath { get; set; } = string.Empty;
        public Point Position { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Rotation { get; set; }

        private Bitmap? _bitmap;
        public Bitmap? Bitmap
        {
            // 遅延評価
            get
            {
                if (_bitmap == null && File.Exists(FilePath))
                {
                    try
                    {
                        _bitmap = BitmapLoader.Load(FilePath);
                    }
                    catch
                    {
                        _bitmap = null;
                    }
                }
                return _bitmap;
            }
        }
    }
}
