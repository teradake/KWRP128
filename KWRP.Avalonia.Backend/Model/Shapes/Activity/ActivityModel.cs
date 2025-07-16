using KWRP.Avalonia.Backend.Enums;
using System.Text;
using Trdk.Geometry;

namespace KWRP.Avalonia.Backend.Model.Shapes.Activity
{
    public class ActivityModel
    {
        public IShape WorkArea { get; set; }
        public IShape OccArea { get; set; }
        public IShape GoalArea { get; set; }

        public int ActivityId { get; set; }
        public int WorkType => ActivityType.ToWorkType();
        public double RefSpeed { get; set; } = 2.5;
        public double EdgeSpeed { get; set; } = 2.0;
        public double Dir { get; set; } = 0.0;  // radian
        public int RepeatNum { get; set; } = 1;
        public bool Comp => ActivityType.ToComp();
        public bool[] Edge { get; set; } = { false, false };
        public RollerActivityType ActivityType { get; set; }
        public string OutputFIlePrefix => $"{ActivityId:D3}_{ActivityType.GetID()}";

        #region info可視化用
        public int GroupId { get; set; }
        public int AreaID { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public Vec2 InfoPos { get; set; } = new Vec2(0, 0);
        public bool HeadToUp { get; set; }
        #endregion

        public override string ToString()
        {
            if (OccArea == null || GoalArea == null || WorkArea == null)
                throw new Exception("エリアの設定ができていません");

            var sb = new StringBuilder();
            sb.AppendLine($"activityId,{ActivityId}");
            sb.AppendLine($"workType,{WorkType}");

            var occ = OccArea.Shape.ToClockWise();
            for (int i = 0; i < occ.Points.Count; ++i)
            {
                sb.AppendLine($"occArea[{i}],{occ.Points[i].X:F5},{occ.Points[i].Y:F5}");
            }

            var goal = GoalArea.Shape.ToClockWise();
            for (int i = 0; i < goal.Points.Count; ++i)
            {
                sb.AppendLine($"goalArea[{i}],{goal.Points[i].X:F5},{goal.Points[i].Y:F5}");
            }

            var work = WorkArea.Shape.ToClockWise();
            for (int i = 0; i < work.Points.Count; ++i)
            {
                sb.AppendLine($"workArea[{i}],{work.Points[i].X:F5} , {work.Points[i].Y:F5}");
            }

            sb.AppendLine($"refSpeed,{RefSpeed:F5}");
            sb.AppendLine($"edgeSpeed,{EdgeSpeed:F5}");
            sb.AppendLine($"dir,{Dir:F6}");     // ±0.001 deg.の誤差に入るはず
            sb.AppendLine($"repeatNum,{RepeatNum}");

            sb.AppendLine($"comp,{TorF(Comp)}");
            sb.AppendLine($"edge,{TorF(Edge[0])},{TorF(Edge[1])}");

            return sb.ToString();
        }

        static string TorF(bool ok) => ok ? "TRUE" : "FALSE";

        public string ToCadScript()
        {
            if (OccArea == null || GoalArea == null || WorkArea == null)
                throw new NullReferenceException("エリア設定ができていません");

            var sb = new StringBuilder();

            // PolyLine
            double lineWidth = 0.2;
            sb.AppendLine("PLINE");
            for (int i = 0; i < WorkArea.Shape.Points.Count; i++)
            {
                sb.AppendLine($"{WorkArea.Shape.Points[i].X:F5},{WorkArea.Shape.Points[i].Y:F5}");
                if (i == 0)
                    sb.AppendLine($"w {lineWidth} {lineWidth}");
            }
            sb.AppendLine("c");

            var c = WorkArea.Shape.Centroid;

            // Arrow
            var seg = Seg2.Create(c, 3.0, Dir);
            var from = seg.Src;
            var to = from + new Vec2(Dir) * 3;
            sb.AppendLine("PLINE");
            sb.AppendLine(from.ToString());
            sb.AppendLine("w 1.85 0");
            sb.AppendLine(to.ToString());
            sb.AppendLine();

            // MultiText
            var width = 3;
            var mTextCorner = Seg2.Create(c, 3.1, Dir).Src + new Vec2(Dir + Math.PI * 0.5) * (width * 0.5);
            sb.AppendLine("MTEXT");
            sb.AppendLine(mTextCorner.ToString());
            sb.AppendLine($"r {Dir * 180 / Math.PI - 90:0.00}");
            sb.AppendLine($"h 0.71");
            sb.AppendLine($"w {width:0.0}");
            sb.AppendLine($"{GroupId}-{AreaID}");
            sb.AppendLine($"L{Length:0.0}");
            sb.AppendLine($"W{Width:0.0}");

            return sb.ToString();
        }
    }

}
