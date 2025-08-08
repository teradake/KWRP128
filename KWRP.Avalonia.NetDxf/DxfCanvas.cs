using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace KWRP.Avalonia.NetDxf
{
    public class DxfCanvas : Control
    {
        public List<DxfShape> Shapes { get; set; } = [];

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            foreach (var shape in Shapes)
            {
                var pen = new Pen(new SolidColorBrush(shape.Color), shape.LineWeight);

                switch (shape)
                {
                    case DxfLine line:
                        context.DrawLine(pen, line.Start, line.End);
                        break;
                    case DxfPolyline poly:
                        for (int i = 0; i < poly.Points.Count - 1; ++i)
                        {
                            context.DrawLine(pen, poly.Points[i], poly.Points[i + 1]);
                        }
                        break;
                    case DxfCircle c:
                        Rect bounds = new Rect(c.Center.X - c.Radius, c.Center.Y - c.Radius, c.Radius * 2, c.Radius * 2);
                        context.DrawEllipse(null, pen, c.Center, c.Radius, c.Radius);
                        break;
                    case DxfArc arc:
                        {
                            var startRad = Math.PI * arc.StartAngle / 180;
                            var endRad = Math.PI * arc.EndAngle / 180;

                            var start = new Point(
                                arc.Center.X + arc.Radius * Math.Cos(startRad),
                                arc.Center.Y + arc.Radius * Math.Sin(startRad));

                            var end = new Point(
                                arc.Center.X + arc.Radius * Math.Cos(endRad),
                                arc.Center.Y + arc.Radius * Math.Sin(endRad));

                            var sweep = (arc.EndAngle - arc.StartAngle + 360) % 360;
                            var isLargeArc = sweep > 180;

                            var geom = new StreamGeometry();
                            using (var ctx = geom.Open())
                            {
                                ctx.BeginFigure(start, false);
                                ctx.ArcTo(end, new Size(arc.Radius, arc.Radius), 0,
                                          isLargeArc, SweepDirection.Clockwise);
                            }

                            context.DrawGeometry(null, pen, geom);
                        }
                        break;
                    case DxfText txt:
                        {
                            var formatted = new FormattedText(
                                textToFormat: txt.Content,
                                culture: System.Globalization.CultureInfo.CurrentCulture,
                                flowDirection: FlowDirection.LeftToRight,
                                typeface: Typeface.Default,
                                emSize: txt.Height,
                                foreground: pen.Brush);

                            // DXFはY軸正方向が上向きなのに対して、Avaloniaのコントロールでは下向きなので回転+反転処理
                            var transform =
                                Matrix.CreateTranslation(-txt.Position.X, -txt.Position.Y)
                                * Matrix.CreateRotation(-txt.Rotation * Math.PI / 180.0)
                                * Matrix.CreateScale(1, -1)
                                * Matrix.CreateTranslation(txt.Position.X, txt.Position.Y);

                            using var modifier = context.PushTransform(transform);
                            context.DrawText(formatted, new Point(txt.Position.X, txt.Position.Y - formatted.Height));  // TEXTはアンカー点が左下。左上になるよう調整
                        }
                        break;
                    case DxfMText txt:
                        {
                            var formatted = new FormattedText(
                                textToFormat: txt.Text,
                                culture: System.Globalization.CultureInfo.CurrentCulture,
                                flowDirection: FlowDirection.LeftToRight,
                                typeface: Typeface.Default,
                                emSize: txt.Height,
                                foreground: pen.Brush);

                            // DXFはY軸正方向が上向きなのに対して、Avaloniaのコントロールでは下向きなので回転+反転処理
                            var transform =
                                 Matrix.CreateTranslation(-txt.Position.X, -txt.Position.Y)
                                * Matrix.CreateRotation(-txt.Rotation * Math.PI / 180.0)
                                * Matrix.CreateScale(1, -1)
                                * Matrix.CreateTranslation(txt.Position.X, txt.Position.Y);

                            using var modifier = context.PushTransform(transform);

                            context.DrawText(formatted, txt.Position);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}