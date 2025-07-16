namespace Trdk.Geometry
{
 
    /// <summary>
    /// 線分クラス
    /// </summary>
    public class Seg2
    {
        private readonly Vec2 _unit_vec;
        private readonly double _len;

        public Vec2 Src { get; }
        public Vec2 Dst { get; }

        Seg2(Vec2 src, Vec2 dst)
        {
            Src = src;
            Dst = dst;

            var d = dst - src;
            _unit_vec = d.Normalized;
            _len = d.Length;
        }


        public Vec2 UnitVec => _unit_vec;
        public double Length => _len;
        public bool IsVertical => Utils.IsZero(Math.Max(Math.Abs(_unit_vec.X), Math.Abs((Dst - Src).X)));
        public bool ToRight => Utils.IsNegative(Src.X - Dst.X);
        public Seg2 ToOrdered() => CreateOrdered(Src, Dst);

        public static Seg2 Create(Vec2 src, Vec2 dst) => new Seg2(src, dst);
        public static Seg2 CreateOrdered(Vec2 src, Vec2 dst)
        {
            var d = dst - src;
            if (src.CompareTo(dst) <= 0)
            {
                return new Seg2(src, dst);
            }
            return new Seg2(dst, src);
        }

        public static Seg2 Create(Vec2 center, double length, double directionRad)
        {
            var arm = new Vec2(Math.Cos(directionRad), Math.Sin(directionRad)) * (length * 0.5);
            return new Seg2(center - arm, center + arm);
        }
    }
 
    
    public static class SegmentExtensions
    {
        /// <summary>
        /// 線分と直線y=aとの交点を返す。交点がない場合nullを返す
        /// </summary>
        /// <param name="seg"></param>
        /// <param name="a">直線y=a</param>
        /// <returns></returns>
        public static Vec2? GetVerticalLineIntersection(this Seg2 seg, double a)
        {
            var s = seg.Src;
            var t = seg.Dst;
            if (s.CompareTo(t) > 0)
            {
                (s, t) = (t, s);
            }

            if (a < s.X || a > t.X || seg.IsVertical)
            {
                return null;
            }

            return new Vec2(
                x: a, 
                y: (t.Y - s.Y) / (t.X - s.X) * (a - s.X) + s.Y);
        }
    }
}
