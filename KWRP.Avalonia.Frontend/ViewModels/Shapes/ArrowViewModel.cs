using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using Trdk.Geometry;

namespace KWRP.Avalonia.Frontend.ViewModels.Shapes
{
    public class ArrowViewModel
    {
        const double C_ARROW_MAXIMUM_LENGTH = 150.0;
        const double C_ARROW_HEAD_ANGLE = Math.PI / 6; // 30 deg.

        public Point StartPoint { get; }
        public Point EndPoint { get; }
        public IList<Point> ArrowHeadPoints { get; }
        public Direction2 Direction { get; }
        public bool IsVisible { get; }
        public double Length { get; }

        public ArrowViewModel(double startX, double startY, double endX, double endY, bool isVisible=true, double headSize=15.0)
        {
            var angle = Math.Atan2(endY - startY, endX - startX);

            Length = Math.Min(
                Math.Sqrt((endX - startX) * (endX - startX) + (endY - startY) * (endY - startY)),
                C_ARROW_MAXIMUM_LENGTH);

            Direction = Direction2.ByRadian(angle);
            StartPoint = new Point(startX, startY);
            var headPoint = new Point(Length * Math.Cos(angle) + startX, Length * Math.Sin(angle) + startY);
            IsVisible= isVisible;

            {
                var x1 = headPoint.X - headSize * Math.Cos(angle - C_ARROW_HEAD_ANGLE);
                var y1 = headPoint.Y - headSize * Math.Sin(angle - C_ARROW_HEAD_ANGLE);

                var x2 = headPoint.X - headSize * Math.Cos(angle + C_ARROW_HEAD_ANGLE);
                var y2 = headPoint.Y - headSize * Math.Sin(angle + C_ARROW_HEAD_ANGLE);

                ArrowHeadPoints = new List<Point>
                {
                    new Point(x1, y1),
                    headPoint,
                    new Point(x2, y2)
                };

                EndPoint = new Point(
                    x: ArrowHeadPoints.Sum(p => p.X) / ArrowHeadPoints.Count,
                    y: ArrowHeadPoints.Sum(p => p.Y) / ArrowHeadPoints.Count);
            }
        }
    }
}
