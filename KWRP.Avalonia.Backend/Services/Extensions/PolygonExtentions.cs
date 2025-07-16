using System.Linq;
using System.Text;
using Trdk.Geometry;

namespace KWRP.Avalonia.Backend.Services.Extensions
{
    public static class PolygonExtentions
    {
        public static string ToSlopeLine(this Polygon polygon)
        {
            var sb = new StringBuilder();
            foreach (var p in polygon.Points.Append(polygon.Points.First()))
            {
                sb.AppendLine($"{p.X:F6},{p.Y:F6}");
            }
            return sb.ToString();
        }
    }
}
