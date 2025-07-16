using Trdk.Geometry;
using Trdk.Geometry.NTS;

namespace KWRP.Avalonia.Backend.Model.Shapes
{
    public class PairedLaneModel : IShape
    {
        private readonly Polygon _polygon;
        private readonly Polygon[] _lanes;
        private readonly Polygon[] _orthogonalLanes;

        public Polygon Shape => _polygon;
        public IEnumerable<Polygon> OrthogonalLanes => _orthogonalLanes;
        public Seg2? Arrow { get; private set; }

        private PairedLaneModel(Polygon[] lanes, Polygon[] orthogonalLanes)
        {
            _lanes = lanes;
            _orthogonalLanes = orthogonalLanes;
            _polygon = PolygonMerger.Merge(lanes);
        }

        public static PairedLaneModel CreateByOrthogonal(
            IEnumerable<Polygon> orthogonalLanes, 
            double progressDirectionRad)
        {
            var lanes = orthogonalLanes.Select(lane => lane.Rotate(progressDirectionRad));
            return new PairedLaneModel(lanes.ToArray(), orthogonalLanes.ToArray());
        }

        public static PairedLaneModel CreateByOrthogonalWithArrow(
            IEnumerable<Polygon> orthogonalLanes,
            double progressDirectionRad,
            bool headToRight)
        {
            var instance = CreateByOrthogonal(orthogonalLanes, progressDirectionRad);

            var arrowDir = progressDirectionRad + 0.5 * (headToRight ? -Math.PI : Math.PI);
            var arrow = Seg2.Create(instance._polygon.Centroid, 6, arrowDir);
            instance.Arrow = arrow;

            return instance;
        }
    }
}
