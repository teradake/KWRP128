using KWRP.Avalonia.Backend;
using KWRP.Avalonia.Backend.Services;
using System.Collections.Generic;
using System.Linq;
using Trdk.Geometry;

namespace KWRP.Avalonia.Frontend.Services.LaneArrangement.Evaluate
{
    public class LaneArrangementDiffEvaluationService : ILaneArrangementEvaluationService
    {
        public double Score(PolygonWithHoles polygon, IEnumerable<BoundingBox> orthogonalLanes)
        {
            if (polygon == null || !orthogonalLanes.Any())
            {
                return KWRPConstants.C_INF;
            }

            var groups = orthogonalLanes.GroupBy(box => box.Xmin).ToList();
            var ymax = groups.Select(g => g.Max(box => box.Ymax));
            var ymin = groups.Select(g => g.Min(box => box.Ymin));

            return ymax.Zip(ymax.Skip(1), (l, r) => (l - r) * (l - r)).Sum()
                + ymin.Zip(ymin.Skip(1), (l, r) => (l - r) * (l - r)).Sum();
        }
    }
}
