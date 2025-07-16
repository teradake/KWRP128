using Trdk.Geometry;

namespace KWRP.Avalonia.Backend.Model.Shapes
{
    public class GoalAreaModel : IShape
    {
        public Polygon Shape { get; }

        public GoalAreaModel(Polygon shape)
        {
            Shape = shape;
        }
    }
}
