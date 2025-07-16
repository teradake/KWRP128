namespace Trdk.Geometry.NTS
{
    public static class NTSExtensions
    {
        public static NetTopologySuite.Geometries.LinearRing ToNTSLinearRing(this Trdk.Geometry.Polygon poly)
        {
            var coordinates = poly
                .Points
                .Append(poly.Points.First())
                .Select(p => new NetTopologySuite.Geometries.Coordinate(p.X, p.Y))
                .ToArray();
            return new NetTopologySuite.Geometries.LinearRing(coordinates);
        }

        public static NetTopologySuite.Geometries.Polygon ToNTSPolygon(this Trdk.Geometry.Polygon polygon)
        {
            var shell = polygon.ToNTSLinearRing();
            return new NetTopologySuite.Geometries.Polygon(shell);
        }

        public static IEnumerable<NetTopologySuite.Geometries.Polygon> MakeHole(this Trdk.Geometry.Polygon polygon, IEnumerable<Trdk.Geometry.Polygon> holes)
        {
            return polygon.GetDiff(holes).ToNTSPolygonList();
        }

        

        public static NetTopologySuite.Geometries.Polygon ToNTSPolygon(this PolygonWithHoles polygon)
        {
            var shell = polygon.Shell.ToNTSLinearRing();
            var holes = polygon.Holes.Select(hole => hole.ToNTSLinearRing()).ToArray();
            return new NetTopologySuite.Geometries.Polygon(shell, holes);
        }

        public static PolygonWithHoles ToPolygonWithHoles(this NetTopologySuite.Geometries.Polygon ntsPolygon, double tolerance=0.0)
        {
            var shell = Polygon.AsCounterClockwise(
                points: ntsPolygon.Shell.Coordinates.Select(p => new Trdk.Geometry.Vec2(p.X, p.Y)).ToArray(),
                tolerance: tolerance);

            if (ntsPolygon.Holes.Length == 0)
            {
                return PolygonWithHoles.Create(shell);
            }

            var holes = ntsPolygon.Holes.Select(hole =>
            {
                return Polygon.AsCounterClockwise(
                    points: hole.Coordinates.Select(p => new Trdk.Geometry.Vec2(p.X, p.Y)).ToArray(),
                    tolerance: tolerance);
            }).ToArray();

            return PolygonWithHoles.Create(shell, holes);
        }

        public static IEnumerable<NetTopologySuite.Geometries.Polygon> ToNTSPolygonList(this NetTopologySuite.Geometries.Geometry geom)
        {
            if (geom is NetTopologySuite.Geometries.GeometryCollection geometryCollection)
            {
                foreach (var geometry in geometryCollection)
                {
                    if (geometry is NetTopologySuite.Geometries.Polygon polygon)
                    {
                        yield return polygon;
                    }
                }
            }
            else if (geom is NetTopologySuite.Geometries.Polygon polygon)
            {
                yield return polygon;
            }
        }
    }
}
