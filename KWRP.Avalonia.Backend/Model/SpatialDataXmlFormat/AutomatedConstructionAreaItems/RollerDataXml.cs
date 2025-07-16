using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
