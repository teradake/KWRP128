using Avalonia;
using Avalonia.Media;
using KWRP.Avalonia.Backend.Model.Shapes;
using System.Collections.Generic;
using Trdk.Geometry;

namespace KWRP.Avalonia.Frontend.ViewModels.Shapes
{
    public class BoundingBoxViewModel
    {
        public IList<Point> Points { get; }
        public IBrush Fill { get; }


        BoundingBoxViewModel(BoundingBox box)
        {
            Points = [
                new Point(box.Xmin, box.Ymin),
                new Point(box.Xmin + box.Width, box.Ymin),
                new Point(box.Xmin + box.Width, box.Ymin + box.Height),
                new Point(box.Xmin, box.Ymin + box.Height),
                ];

            Fill = new SolidColorBrush(Color.FromArgb(150, 247, 199, 210));
        }

        public static BoundingBoxViewModel? Create(BoundingBox? box)
        {
            if (box == null) return null;
            return new BoundingBoxViewModel(box);
        }
    }

}
