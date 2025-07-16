using KWRP.Avalonia.Backend;
using KWRP.Avalonia.Backend.Services;
using System.Collections.Generic;
using System.Linq;
using Trdk.Geometry;

namespace KWRP.Avalonia.Frontend.Services.LaneArrangement.Evaluate
{
    public class LaneArrangementCoverageEvaluationService : ILaneArrangementEvaluationService
    {
        public double Score(PolygonWithHoles polygon, IEnumerable<BoundingBox> orthogonalLanes)
        {
            if (polygon == null || !orthogonalLanes.Any())
            {
                return KWRPConstants.C_INF;
            }

            // とりあえず雑な充填率で評価
            return -orthogonalLanes.Sum(box => box.Area) / polygon.Area;

            // 変動係数とかでばらつき評価できるかも？
            //var laneLengthList = orthogonalLanes.Select(box => box.Ymax - box.Ymin).ToList();
            //var mu = laneLengthList.Sum() / laneLengthList.Count;
            //var sigma = Math.Sqrt(laneLengthList.Sum(v => (v - mu) * (v - mu)) / laneLengthList.Count);
            //var CV = sigma / mu;
        }
    }
}
