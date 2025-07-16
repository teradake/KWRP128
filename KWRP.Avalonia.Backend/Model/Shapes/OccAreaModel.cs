using Trdk.Geometry;

namespace KWRP.Avalonia.Backend.Model.Shapes
{
    public class OccAreaModel : IShape
    {
        public Polygon Shape { get; }

        public OccAreaModel(Polygon shape)
        {
            Shape = shape;
        }
    }
}
