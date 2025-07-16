using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using netDxf;

namespace KWRP.Avalonia.NetDxf
{
    public static class DxfEntityExtensions
    {
        static (Color color, double lineweight) GetEntityStyle(netDxf.Entities.EntityObject entity)
        {
            var acColor = entity.Color.IsByLayer ? entity.Layer.Color : entity.Color;
            var color = Color.FromArgb(255, acColor.R, acColor.G, acColor.B);
            if (color == Colors.White)
            {
                color = Colors.Black;
            }

            var lw = entity.Lineweight switch
            {
                Lineweight.ByLayer => (double)entity.Layer.Lineweight / 100.0,
                _ => (double)entity.Lineweight / 100.0,
            };

            if (lw <= 0) lw = 0.25;

            return (color, lw);
        }

        public static DxfLine ToShape(this netDxf.Entities.Line line)
        {
            var (color, lw) = GetEntityStyle(line);

            return new DxfLine
            {
                Start = new Point(line.StartPoint.X, line.StartPoint.Y),
                End = new Point(line.EndPoint.X, line.EndPoint.Y),
                Color = color,
                LineWeight = lw,
            };
        }

        public static DxfPolyline ToShape(this netDxf.Entities.Polyline2D poly)
        {
            var (color, lw) = GetEntityStyle(poly);

            var points = poly.PolygonalVertexes(20).Select(v => new Point(v.X, v.Y)).ToList();
            if (poly.IsClosed && points.Count > 1)
            {
                points.Add(points[0]);
            }
            return new DxfPolyline
            {
                Points = points,
                Color = color,
                LineWeight = lw,
            };
        }

        public static DxfPolyline ToShape(this netDxf.Entities.Polyline3D poly)
        {
            var (color, lw) = GetEntityStyle(poly);

            var points = poly.PolygonalVertexes(20).Select(v => new Point(v.X, v.Y)).ToList();
            if (poly.IsClosed && points.Count > 1)
            {
                points.Add(points[0]);
            }
            return new DxfPolyline
            {
                Points = points,
                Color = color,
                LineWeight = lw,
            };
        }

        public static DxfCircle ToShape(this netDxf.Entities.Circle circle)
        {
            var (color, lw) = GetEntityStyle(circle);

            return new DxfCircle
            {
                Center = new Point(circle.Center.X, circle.Center.Y),
                Radius = circle.Radius,
                Color = color,
                LineWeight = lw,
            };
        }

        public static DxfArc ToShape(this netDxf.Entities.Arc arc)
        {
            var (color, lw) = GetEntityStyle(arc);

            return new DxfArc
            {
                Center = new Point(arc.Center.X, arc.Center.Y),
                Radius = arc.Radius,
                StartAngle = arc.StartAngle,
                EndAngle = arc.EndAngle,
                Color = color,
                LineWeight = lw,
            };
        }

        public static DxfText ToShape(this netDxf.Entities.Text text)
        {
            var (color, lw) = GetEntityStyle(text);

            return new DxfText
            {
                Content = text.Value,
                Position = new Point(text.Position.X, text.Position.Y),
                Height = text.Height,
                Rotation = text.Rotation,
                Color = color,
                LineWeight = lw,
            };
        }

        public static DxfMText ToShape(this netDxf.Entities.MText text)
        {
            var (color, lw) = GetEntityStyle(text);

            return new DxfMText
            {
                Text = text.Value.Replace("\\P", Environment.NewLine).Replace("\\{", "{").Replace("\\}", "}"),
                Position = new Point(text.Position.X, text.Position.Y),
                Height = text.Height,
                Rotation = text.Rotation,
                Color = color,
                LineWeight = lw,
            };
        }

        public static List<DxfShape> ToShapes(this netDxf.Entities.Insert insert)
        {
            var shapes = new List<DxfShape>();

            var pos = insert.Position;
            var mat = Matrix3.RotationZ(insert.Rotation * Math.PI / 180.0);

            foreach (var entity in insert.Block.Entities.Where(e => e.IsVisible))
            {
                entity.TransformBy(mat, pos);

                if (entity is netDxf.Entities.Line line)
                    shapes.Add(line.ToShape());
                else if (entity is netDxf.Entities.Polyline2D poly)
                    shapes.Add(poly.ToShape());
                else if (entity is netDxf.Entities.Polyline3D poly3)
                    shapes.Add(poly3.ToShape());
                else if (entity is netDxf.Entities.Circle circle)
                    shapes.Add(circle.ToShape());
                else if (entity is netDxf.Entities.Arc arc)
                    shapes.Add(arc.ToShape());
                else if (entity is netDxf.Entities.Text text)
                    shapes.Add(text.ToShape());
                else if (entity is netDxf.Entities.MText mtext)
                    shapes.Add(mtext.ToShape());
                else if (entity is netDxf.Entities.Insert ins)
                    shapes.AddRange(ins.ToShapes());
            }
            return shapes;
        }
    }
}
