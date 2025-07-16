using NetTopologySuite.Geometries;

namespace Trdk.Geometry.NTS
{
    public static class BooleanExtensions
    {
        public static NetTopologySuite.Geometries.Geometry GetDiff(this Trdk.Geometry.Polygon polygon, IEnumerable<Trdk.Geometry.Polygon> holes)
        {
            var result = (NetTopologySuite.Geometries.Geometry)polygon.ToNTSPolygon();
            foreach (var hole in holes)
            {
                result = result.Difference(hole.ToNTSPolygon());
            }
            return result;
        }

        public static NetTopologySuite.Geometries.Geometry MergePolygons(params Trdk.Geometry.Polygon[] polygons)
        {
            if (polygons.Length == 0)
            {
                throw new ArgumentException("At least one polygon is required to merge.");
            }
            var result = (NetTopologySuite.Geometries.Geometry)polygons[0].ToNTSPolygon();
            for (int i = 1; i < polygons.Length; i++)
            {
                result = result.Union(polygons[i].ToNTSPolygon());
            }
            return result;
        }

        public static NetTopologySuite.Geometries.Geometry GetIntersect(this Trdk.Geometry.Polygon polygon, IEnumerable<Trdk.Geometry.Polygon> holes)
        {
            var result = (NetTopologySuite.Geometries.Geometry)polygon.ToNTSPolygon();
            foreach (var hole in holes)
            {
                result = result.Intersection(hole.ToNTSPolygon());
            }
            return result;
        }

        public static bool IsIntersect(this Trdk.Geometry.Polygon polyA, Trdk.Geometry.Polygon polyB)
        {
            return GetIntersectionArea(polyA, polyB) > 0;
        }

        public static double GetIntersectionArea(this Trdk.Geometry.Polygon polyA, Trdk.Geometry.Polygon polyB)
        {
            var ntsPolyA = polyA.ToNTSPolygon();
            var ntsPolyB = polyB.ToNTSPolygon();
            return (ntsPolyA.Intersection(ntsPolyB)).Area;
        }
    }
}
