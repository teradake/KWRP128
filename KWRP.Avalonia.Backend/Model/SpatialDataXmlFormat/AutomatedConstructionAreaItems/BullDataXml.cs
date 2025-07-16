using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace KWRP.Avalonia.Backend.Models.AutomatedConstructionAreaItems
{
    [XmlRoot("CompactionAreaItems")]
    public class BullDataXML
    {
        public double LaneProgressDirection { get; set; }
        public bool IsBullHeadingToRight { get; set; }
        public Point3D BaseLinePoint { get; set; } = new Point3D();
        public Perimeter OuterPerimeter { get; set; } = new Perimeter();
        public ObstacleArea ObstacleArea { get; set; } = new ObstacleArea();
    }

}
