using System.Xml.Serialization;

namespace KWRP.Avalonia.Backend.Models.AutomatedConstructionAreaItems
{
    public class RollerDataXML
    {
        [XmlElement("LaneProgressDirection")]
        public double LaneProgressDirection { get; set; }

        [XmlElement("IsRollerHeadingToRight")]
        public bool IsRollerHeadingRight { get; set; }

        [XmlElement("CompactionArea")]
        public PolylineWithHoles WorkArea { get; set; } = new PolylineWithHoles();

        [XmlElement("BaseLinePoint")]
        public Point3D BaseLinePoint { get; set; } = new Point3D { X = KWRPConstants.C_INF, Y = KWRPConstants.C_INF, Z = KWRPConstants.C_INF, };

        [XmlElement("RollerHeadingPattern")]
        public string RollerHeadingPattern { get; set; } = "L";
    }
}
