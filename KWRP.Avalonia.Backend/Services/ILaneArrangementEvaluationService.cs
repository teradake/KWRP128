using Trdk.Geometry;

namespace KWRP.Avalonia.Backend.Services
{
    public interface ILaneArrangementEvaluationService
    {
        double Score(PolygonWithHoles polygon, IEnumerable<BoundingBox> orthogonalLanes);
    }
}
