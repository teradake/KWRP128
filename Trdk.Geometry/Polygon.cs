using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace Trdk.Geometry
{
    public class Polygon
    {
        private readonly ReadOnlyCollection<Vec2> _points;
        private double _area;

        private Polygon(Vec2[] points)
        {
            _points = points.AsReadOnly();
            AaBB = _points.GetBoundingBox();
        }


        public int N => _points.Count;
        public double Area => Math.Abs(_area);
        public bool IsClockwise => _area < 0;
        public IReadOnlyList<Vec2> Points => _points;
        public BoundingBox AaBB { get; }

        public Vec2 this[int index]
        {
            get => _points[(index % N + N) % N];
        }

        /// <summary>
        /// 多角形を構成する線分を取得
        /// </summary>
        /// <param name="ordered">trueのとき、src < dstとなるように線分を生成する。falseの場合はpolygonに登録された点列のまま</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<Seg2> GetSegments(bool ordered = false)
        {
            for (int i = 0; i < N - 1; ++i)
            {
                yield return Seg2.Create(_points[i], _points[i + 1]);
            }
            yield return Seg2.Create(_points[N - 1], _points[0]);
        }

        private Vec2? _centroid = null;
        public Vec2 Centroid => _centroid ??= CalcCentroid();

        /// <summary>
        /// 多角形の重心を計算
        /// </summary>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vec2 CalcCentroid()
        {
            var result = new Vec2(0, 0);
            for (int i = 0; i < N; ++i)
            {
                int j = (i + 1) % N;
                var area = 0.5 * _points[i].Cross(_points[j]);
                result += area * (_points[i] + _points[j]) * (1.0 / 3.0);
            }
            return result * (1.0 / _area);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Polygon Rotate(double radian)
        {
            var points = this._points.Select(p => p.Rot(radian)).ToArray();
            return new Polygon(points) { _area = _area };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Polygon Translate(double dx, double dy)
        {
            var points = this._points.Select(p => p += new Vec2(dx, dy)).ToArray();
            return new Polygon(points) { _area = _area };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Polygon CreatePolygon(Vec2[] points, bool shouldBeClockwise)
        {
            if (points.Length <= 2)
            {
                throw new ArgumentException("頂点数が2以下なので多角形を生成できません");
            }

            var area = points.CalcSignedArea();
            bool isCurrentlyClockwise = area < 0;

            if (isCurrentlyClockwise != shouldBeClockwise)
            {
                points = points.Reverse().ToArray();
                area = -area;
            }

            return new Polygon(points) { _area = area };
        }
        public static Polygon AsClockwise(Vec2[] points, bool simplify = true, double tolerance = 0)
            => simplify
            ? CreatePolygon(points.EliminateDuplicatePoint().EliminateNearPoint(tolerance).EliminateColinearPoint().ToArray(), true)
            : CreatePolygon(points, true);

        public static Polygon AsCounterClockwise(Vec2[] points, bool simplify = true, double tolerance = 0)
            => simplify
            ? CreatePolygon(points.EliminateDuplicatePoint().EliminateNearPoint(tolerance).EliminateColinearPoint().ToArray(), false)
            : CreatePolygon(points, false);
    }


    public static class PointCollectionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double CalcSignedArea(this Vec2[] points)
        {
            double area = 0;
            int n = points.Length;
            for (int i = 1; i < n - 1; ++i)
            {
                area += (points[i] - points[0]).Cross(points[i + 1] - points[0]);
            }
            return area * 0.5;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<Vec2> EliminateDuplicatePoint(this IEnumerable<Vec2> points)
        {
            var result = new List<Vec2>();
            foreach (var point in points)
            {
                if (result.Count == 0 || !result.Last().Equals(point))
                {
                    result.Add(point);
                }
            }
            while (result.Count > 0 && result.Last().Equals(result.First()))
            {
                result.RemoveAt(result.Count - 1);
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<Vec2> EliminateColinearPoint(this IEnumerable<Vec2> points)
        {
            var pnts = points.ToArray();
            var removed = new bool[pnts.Length];

            for (int i = 0; i < pnts.Length; ++i)
            {
                var a = pnts[(i - 1 + pnts.Length) % pnts.Length];
                var b = pnts[i];
                var c = pnts[(i + 1) % pnts.Length];
                if (Utils.ISP(a, b, c) == 2)
                {
                    removed[i] = true;
                }
            }
            return pnts.Where((p, index) => !removed[index]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<Vec2> EliminateNearPoint(this IEnumerable<Vec2> points, double tolerance = 0.05)
        {
            if (Math.Abs(tolerance) <= Constants.C_EPS)
                return points;

            var pnts = points.ToArray();
            var lim = tolerance * tolerance;
            bool updated = false;

            do
            {
                updated = false;
                int n = pnts.Length;
                var result = new List<Vec2>();
                for (int i = 0; i < n;)
                {
                    var cur = pnts[i];
                    var nxt = pnts[(i + 1) % n];
                    if (isNear(cur, nxt))
                    {
                        result.Add(nxt);
                        i += 2;
                        updated = true;
                    }
                    else
                    {
                        result.Add(cur);
                        i += 1;
                    }
                }

                if (result.Count > 2 && isNear(result.First(), result.Last()))
                {
                    result.RemoveAt(result.Count - 1);
                    updated = true;
                }

                pnts = result.ToArray();
            }
            while (updated);

            return pnts;

            bool isNear(Vec2 a, Vec2 b) => (a - b).Dot(a - b) < lim;

        }

        public static BoundingBox GetBoundingBox(this IEnumerable<Vec2> points)
        {
            return new BoundingBox
            {
                Xmin = points.Min(p => p.X),
                Xmax = points.Max(p => p.X),
                Ymin = points.Min(p => p.Y),
                Ymax = points.Max(p => p.Y),
            };
        }


    }
}
