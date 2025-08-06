namespace Trdk.Geometry.NTS
{
    public static class PolygonMerger
    {
        public static Trdk.Geometry.Polygon Merge(IEnumerable<Trdk.Geometry.Polygon> polygons)
        {
            if (!polygons.Any())
            {
                throw new ArgumentException("At least one polygon is required to merge.");
            }

            var ntsPolygons = polygons.Select(p => p.ToNTSPolygon()).ToArray();
            var union = new NetTopologySuite.Operation.Union.UnaryUnionOp(ntsPolygons).Union();
            if (union is NetTopologySuite.Geometries.Polygon mergedPolygon)
            {
                return mergedPolygon.ToPolygonWithHoles(PolygonMergerOptions.Tolerance).Shell;
            }
            else
            {
                throw new InvalidOperationException("The union operation did not return a valid polygon.");
            }
        }

        public static double CalculateMergedArea(this IEnumerable<Trdk.Geometry.Polygon> polygons, IEnumerable<Trdk.Geometry.Polygon>? holes = null)
        {
            if (!polygons.Any())
            {
                return 0.0;
            }

            NetTopologySuite.Geometries.Geometry p = polygons.First().ToNTSPolygon();
            foreach (var poly in polygons.Skip(1))
            {
                p = p.Union(poly.ToNTSPolygon());
            }

            if (holes != null)
            {
                foreach (var hole in holes)
                {
                    p = p.Difference(hole.ToNTSPolygon());
                }
            }

            return p.Area;
        }
    }
}
