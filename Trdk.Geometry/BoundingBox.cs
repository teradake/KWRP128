namespace Trdk.Geometry
{
    public class BoundingBox
    {
        public double Xmin { get; init; }
        public double Xmax {  get; init; }
        public double Ymin {  get; init; }
        public double Ymax {  get; init; }

        public double Width => Xmax - Xmin;
        public double Height => Ymax - Ymin;
        public double Area => Width * Height;
        public Vec2 Center => new Vec2(Xmin + Width * 0.5, Ymin + Height * 0.5);

        public static BoundingBox Identity => new()
        {
            Xmin = Constants.C_INF,
            Xmax = -Constants.C_INF,
            Ymin = Constants.C_INF,
            Ymax = -Constants.C_INF,
        };

        public BoundingBox YMirrored => new()
        {
            Xmin = Xmin,
            Xmax = Xmax,
            Ymin = -Ymax,
            Ymax = -Ymin,
        };

        public BoundingBox()
        {
        }
        public BoundingBox(Vec2 A, Vec2 B)
        {
            Xmin = Math.Min(A.X, B.X);
            Xmax = Math.Max(A.X, B.X);
            Ymin = Math.Min(A.Y, B.Y);
            Ymax = Math.Max(A.Y, B.Y);
        }

        public BoundingBox Expand(double dx, double dy)
        {
            return new BoundingBox
            {
                Xmin = Xmin - dx,
                Xmax = Xmax + dx,
                Ymin = Ymin - dy,
                Ymax = Ymax + dy,
            };
        }

        public BoundingBox ExpandByRatio(double r)
        {
            var dx = (Xmax - Xmin) * (r - 1) * 0.5;
            var dy = (Ymax - Ymin) * (r - 1) * 0.5;
            return Expand(dx, dy);
        }

        public BoundingBox Update(BoundingBox box)
        {
            return new BoundingBox
            {
                Xmin = Math.Min(Xmin, box.Xmin),
                Xmax = Math.Max(Xmax, box.Xmax),
                Ymin = Math.Min(Ymin, box.Ymin),
                Ymax = Math.Max(Ymax, box.Ymax),
            };
        }

        public BoundingBox ClampByY(double ymin, double ymax)
        {
            return new BoundingBox
            {
                Xmin = Xmin,
                Xmax = Xmax,
                Ymin = Math.Clamp(Ymin, ymin, ymax),
                Ymax = Math.Clamp(Ymax, ymin, ymax),
            };
        }

        public BoundingBox Update(Polygon polygon) => Update(polygon.AaBB);

        public bool IsIntersect(BoundingBox box)
        {
            return Math.Max(Xmin, box.Xmin) <= Math.Min(Xmax, box.Xmax) 
                && Math.Max(Ymin, box.Ymin) <= Math.Min(Ymax, box.Ymax);
        }

        public bool IsInside(Vec2 vec)
            => vec.X >= Xmin 
            && vec.X <= Xmax
            && vec.Y >= Ymin 
            && vec.Y <= Ymax;

        public Polygon ToPolygon(bool isHole=false)
        {
            var points = new Vec2[]
            {
                new(Xmin, Ymin),
                new(Xmax, Ymin),
                new(Xmax, Ymax),
                new(Xmin, Ymax),
            };
            if (isHole)
            {
                return Polygon.AsClockwise(points);
            }
            return Polygon.AsCounterClockwise(points);
        }

        public override string ToString()
        {
            return $"BoundingBox [Xmin: {Xmin}, Xmax: {Xmax}, Ymin: {Ymin}, Ymax: {Ymax}]";
        }
    }
}
