using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace KWRP.Avalonia.Backend.Models.AutomatedConstructionAreaItems
{
    [XmlRoot("AutomatedConstructionAreaItems")]
    public class AutomatedConstructionAreaItems
    {
        public string PluginVersion { get; set; } = string.Empty;

        [XmlElement("VRData")]
        public RollerDataXML VRData { get; set; } = new RollerDataXML();

        [XmlElement("BDData")]
        public BullDataXML BDData { get; set; } = new BullDataXML();
    }



    public class Point3D
    {
        private double _x;
        private double _y;
        private double _z;

        [XmlElement]
        public double X
        {
            get => _x;
            set => _x = Math.Round(value, 5, MidpointRounding.AwayFromZero);
        }

        [XmlElement]
        public double Y
        {
            get => _y;
            set => _y = Math.Round(value, 5, MidpointRounding.AwayFromZero);
        }

        [XmlElement]
        public double Z
        {
            get => _z;
            set => _z = Math.Round(value, 5, MidpointRounding.AwayFromZero);
        }
    }

    public class Perimeter
    {
        public Shell Shell { get; set; } = new Shell();
    }

    public class ObstacleArea
    {
        [XmlElement("Shell")]
        public List<Shell> Shells { get; set; } = new List<Shell>();
    }

    public class Shell
    {
        [XmlArray("Points")]
        [XmlArrayItem("Point3D")]
        public List<Point3D> Points { get; set; } = new List<Point3D>();
    }

    #region VR

    public class PolylineWithHoles
    {
        [XmlElement("Shell")]
        public Shell Shell = new Shell();

        [XmlArray("Holes")]
        [XmlArrayItem("Hole")]
        public List<Shell> Holes = new List<Shell>();
    }

    #endregion
}
