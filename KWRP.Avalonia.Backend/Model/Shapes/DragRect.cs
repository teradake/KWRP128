using Trdk.Geometry;

namespace KWRP.Avalonia.Backend.Model.Shapes
{
    public class DragRect
    {
        public bool ToLeft { get; }
        public bool IsVisible { get; }
        public Polygon? Shape { get; }
        public Vec2 Start { get; }

        public DragRect(Vec2 start, Vec2 end, double canvasRotationRadian=0, bool isVisible = true)
        {
            Start = start;

            var s = start.Rot(canvasRotationRadian);
            var t = end.Rot(canvasRotationRadian);
            ToLeft = t.X > s.X;
            IsVisible = isVisible;
            try
            {
                Shape = new BoundingBox(s, t).ToPolygon().Rotate(-canvasRotationRadian);
            }
            catch
            {
                Shape = null;
                IsVisible = false;
            }
        }
    }
}
