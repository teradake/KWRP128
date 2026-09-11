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
        public Point3D? BaseLinePoint { get; set; } 

        [XmlElement("RollerHeadingPattern")]
        public string? RollerHeadingPattern { get; set; }

        [XmlIgnore]
        public bool HasBaseLinePoint => BaseLinePoint is not null;

        [XmlIgnore]
        public bool HasRollerHeadingPattern => RollerHeadingPattern is not null;
    }
}
