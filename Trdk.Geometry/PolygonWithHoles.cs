using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace Trdk.Geometry
{
    /// <summary>
    /// 穴つきのポリゴン
    /// </summary>
    public class PolygonWithHoles
    {
        private readonly ReadOnlyCollection<Polygon> _holes;

        public Polygon Shell { get; }
        public ReadOnlyCollection<Polygon> Holes => _holes;
        public double Area => Shell.Area - _holes.Sum(h => h.Area);
        public double N => Shell.N + _holes.Sum(h => h.N);
        public BoundingBox AaBB => Shell.AaBB;

        private PolygonWithHoles(Polygon poly)
            : this(poly, [])
        {
        }

        private PolygonWithHoles(Polygon shell, IEnumerable<Polygon> holes)
        {
            Shell = shell.ToShell();
            _holes = holes
                .Select(p => p.ToHole())
                .ToArray()
                .AsReadOnly();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<Seg2> GetSegments(bool ordered = false)
        {
            return Shell.GetSegments(ordered).Concat(_holes.SelectMany(h => h.GetSegments(ordered)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PolygonWithHoles Rotate(double radian)
        {
            var shell = Shell.Rotate(radian);
            var holes = _holes.Select(hole => hole.Rotate(radian));
            return Create(shell, holes);
        }

        public static PolygonWithHoles Create(Polygon polygon) => new(polygon);
        public static PolygonWithHoles Create(Polygon polygon, IEnumerable<Polygon> holes) => new(polygon, holes);
    }

    public static class PolygonExtensions
    {
        public static Polygon ToClockWise(this Polygon polygon)
        {
            if (polygon.IsClockwise) return polygon;
            return Polygon.AsClockwise(polygon.Points.ToArray());
        }

        public static Polygon ToCounterClockwise(this Polygon polygon)
        {
            if (!polygon.IsClockwise) return polygon;
            return Polygon.AsCounterClockwise(polygon.Points.ToArray());
        }

        public static Polygon ToShell(this Polygon polygon) => polygon.ToCounterClockwise();
        public static Polygon ToHole(this Polygon polygon) => polygon.ToClockWise();

        public static double FindStartingPosition(this PolygonWithHoles target, double minLength)
        {
            try
            {
                var segments = target
                    .GetSegments()
                    .Where(seg => !seg.IsVertical)
                    .OrderBy(seg => Math.Min(seg.Src.X, seg.Dst.X))
                    .ToArray();
                var ordered = segments
                    .Select(seg => seg.ToOrdered())
                    .ToArray();

                int n = segments.Length;
                for (int i = 0; i < n; ++i)
                {
                    var candidate = new List<(Vec2 l, Vec2 r)>();
                    for (int j = i + 1; j < n; ++j)
                    {
                        // 両方同じ方向を向いているので無視できる。
                        if (!(segments[i].ToRight ^ segments[j].ToRight))
                        {
                            continue;
                        }

                        var xl = Math.Max(ordered[i].Src.X, ordered[j].Src.X);
                        var xr = Math.Min(ordered[i].Dst.X, ordered[j].Dst.X);
                        // tがsより真に右側なのでこれ以上調べない
                        if (xl > xr)
                        {
                            break;
                        }

                        var yl = (segments[j].GetVerticalLineIntersection(xl)! - segments[i].GetVerticalLineIntersection(xl)!).Y;
                        var yr = (segments[j].GetVerticalLineIntersection(xr)! - segments[i].GetVerticalLineIntersection(xr)!).Y;
                        if (!segments[i].ToRight)
                        {
                            yl = -yl; yr = -yr;
                        }

                        if (yl >= minLength || yr >= minLength)
                        {
                            candidate.Add((new Vec2(xl, yl), new Vec2(xr, yr)));
                        }
                    }

                    candidate.Sort((lhs, rhs) => lhs.l.X.CompareTo(rhs.l.X));
                    bool monotone = true;
                    for (int j = 0; j < candidate.Count - 1; ++j)
                    {
                        if (Utils.Sign(candidate[j].r.X - candidate[j + 1].l.X) > 0)
                        {
                            monotone = false;
                            break;
                        }
                    }

#if DEBUG
                    if (candidate.Count > 0)
                    {
                        System.Diagnostics.Debug.WriteLine(candidate.Count);
                        System.Diagnostics.Debug.WriteLine((segments[i].Src, segments[i].Dst));
                        System.Diagnostics.Debug.WriteLine(monotone);
                    }
#endif

                    //if (!monotone || candidate.Count == 0)
                    if (candidate.Count == 0)
                    {
                        continue;
                    }

                    var pair = candidate.First();
                    if (pair.l.Y >= minLength)
                    {
                        return pair.l.X;
                    }
                    if (pair.r.Y >= minLength)
                    {
                        return pair.l.X + (pair.r.X - pair.l.X) / (pair.r.Y - pair.l.Y) * (minLength - pair.l.Y);
                    }
                }
                return target.AaBB.Xmin;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                throw;
            }
        }
    }
}
