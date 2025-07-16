using Trdk.Geometry;

namespace KWRP.Avalonia.Backend.Model.Shapes
{
    public class LaneModel : IShape
    {
        private readonly Polygon _polygon;
        private readonly Polygon _orthogonalPolygon;

        public Polygon Shape => _polygon;
        public Polygon Orthogonal => _orthogonalPolygon;
        public double Length => _orthogonalPolygon.AaBB.Height;

        private LaneModel(Polygon polygon, Polygon orthogonalPolygon)
        {
            _polygon = polygon;
            _orthogonalPolygon = orthogonalPolygon;
        }

        public static LaneModel CreateByOrthogonal(Polygon orthognalLane, double progressDirectionRad)
            => new(orthognalLane.Rotate(progressDirectionRad), orthognalLane);

        public static LaneModel CreateByOrthogonal(BoundingBox orthogonalLane, double progressDirectionRad)
            => CreateByOrthogonal(orthogonalLane.ToPolygon(), progressDirectionRad);
    }
}
