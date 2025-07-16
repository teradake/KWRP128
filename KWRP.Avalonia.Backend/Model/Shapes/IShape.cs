using Trdk.Geometry;

namespace KWRP.Avalonia.Backend.Model.Shapes
{
    public interface IShape
    {
        Polygon Shape { get; }
    }
}
