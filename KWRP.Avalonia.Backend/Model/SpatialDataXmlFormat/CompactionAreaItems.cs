using System.Xml.Serialization;

namespace KWRP.Avalonia.Backend.Model.SpatialDataXmlFormat
{
    /// <summary>
    /// 点(X, Y)
    /// </summary>
    public class Point2D
    {
        private double _x;
        private double _y;

        [XmlElement]
        public double X
        {
            get { return _x; }
            set
            {
                _x = Math.Round(value, 4, MidpointRounding.AwayFromZero);
            }
        }

        [XmlElement]
        public double Y
        {
            get { return _y; }
            set
            {
                _y = Math.Round(value, 4, MidpointRounding.AwayFromZero);
            }
        }
    }

    /// <summary>
    /// 点(X, Y, Z)
    /// </summary>
    public class Point3D
    {
        private double _x;
        private double _y;
        private double _z;

        [XmlElement]
        public double X
        {
            get { return _x; }
            set
            {
                _x = Math.Round(value, 4, MidpointRounding.AwayFromZero);
            }
        }

        [XmlElement]
        public double Y
        {
            get { return _y; }
            set
            {
                _y = Math.Round(value, 4, MidpointRounding.AwayFromZero);
            }
        }

        [XmlElement]
        public double Z
        {
            get { return _z; }
            set
            {
                _z = Math.Round(value, 4, MidpointRounding.AwayFromZero);
            }
        }
    }


    /// <summary>
    /// ポリライン
    /// </summary>
    public class Polyline3D
    {
        [XmlArray("Points")]
        [XmlArrayItem("Point3D")]
        public List<Point3D> Points = new List<Point3D>();
    }

    /// <summary>
    /// 穴ありのポリライン
    /// 外側Pointsが一つ、内側Holesは複数のポリラインから構成されることもある
    /// </summary>
    public class PolylineWithHoles
    {
        [XmlElement("Shell")]
        public Polyline3D Shell = new Polyline3D();

        [XmlArray("Holes")]
        [XmlArrayItem("Hole")]
        public List<Polyline3D> Holes = new List<Polyline3D>();
    }

    /// <summary>
    /// 転圧領域に関するデータ
    /// </summary>
    public class CompactionAreaItems
    {
        [XmlElement("LaneProgressDirection")]
        public double LaneProgressDirection { get; set; }

        [XmlElement("IsRollerHeadingToRight")]
        public bool IsRollerHeadingRight { get; set; }

        [XmlElement("NaruseParameter")]
        public LaneProgressDirectionForNaruse NaruseParameter { get; set; } = new LaneProgressDirectionForNaruse();

        [XmlElement("IsRollerMoveAlongDamAxis")]
        public bool IsRollerMoveAlongDamAxis { get; set; } = false;

        [XmlElement("CompactionArea")]
        public PolylineWithHoles WorkArea { get; set; } = new PolylineWithHoles();
    }


    public class DamAxisPoints
    {
        [XmlElement("Points")]
        public Polyline3D Polyline = new Polyline3D();
    }


    public class LaneProgressDirectionForNaruse
    {
        [XmlElement("IsLaneProgressAlongDamAxis")]
        public bool IsLaneProgressAlongDamAxis { get; set; } = false;

        [XmlElement("LeftBankToRightBank")]
        public bool LeftBankToRightBank { get; set; } = false;

        [XmlElement("DamAxis")]
        public DamAxisPoints DamAxis { get; set; } = new DamAxisPoints();
    }
}
