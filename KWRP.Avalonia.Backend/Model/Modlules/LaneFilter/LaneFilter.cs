using Trdk.Geometry;

namespace KWRP.Avalonia.Backend.Model.Modlules.LaneFilter
{
    public static class LaneFilter
    {
        public static IEnumerable<Polygon> AdjustLength(
            this IEnumerable<Polygon> orthogonalLanes, 
            double shortenFront,
            double shortenRear,
            bool headToRight)
        {
            foreach (var lane in orthogonalLanes)
            {
                if (headToRight)
                {
                    (shortenFront, shortenRear) = (shortenRear, shortenFront);
                }
                var ymin = lane.AaBB.Ymin + shortenRear;
                var ymax = lane.AaBB.Ymax - shortenFront;
                yield return new BoundingBox
                {
                    Xmin = lane.AaBB.Xmin,
                    Xmax = lane.AaBB.Xmax,
                    Ymin = ymin,
                    Ymax = ymax
                }.ToPolygon();
            }
        }

        public static IEnumerable<BoundingBox> AdjustLength(
            this IEnumerable<BoundingBox> orthogonalLanes,
            double shortenFront,
            double shortenRear, 
            bool headToRight)
        {
            if (headToRight)
            {
                (shortenFront, shortenRear) = (shortenRear, shortenFront);
            }
            foreach (var lane in orthogonalLanes)
            {
                var ymin = lane.Ymin + shortenRear;
                var ymax = lane.Ymax - shortenFront;
                yield return new BoundingBox
                {
                    Xmin = lane.Xmin,
                    Xmax = lane.Xmax,
                    Ymin = ymin,
                    Ymax = ymax
                };
            }
        }
    }

}
