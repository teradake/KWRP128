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
        public string OutputFIlePrefix => $"{ActivityId:D3}_{ActivityType.GetWorkName()}";

        #region info可視化用
        public int GroupId { get; set; }
        public int AreaID { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public Vec2 InfoPos { get; set; } = new Vec2(0, 0);
        public bool HeadToUp { get; set; }
        public int LaneCount { get; set; }

        public TimeSpan WorkTime { get; set; }
        public TimeSpan AccumulatedWorkTime { get; set; }
        #endregion

        public override string ToString()
        {
            if (OccArea == null || GoalArea == null || WorkArea == null)
                throw new Exception("エリアの設定ができていません");

            var sb = new StringBuilder();
            sb.AppendLine($"activityId,{ActivityId}");
            sb.AppendLine($"workType,{WorkType}");

            var occ = OccArea.Shape.ToClockWise().Points.ReorderPointsToWorkList(Dir);
            for (int i = 0; i < occ.Count; ++i)
            {
                sb.AppendLine($"occArea[{i}],{occ[i].X:F5},{occ[i].Y:F5}");
            }

            var goal = GoalArea.Shape.ToClockWise().Points.ReorderPointsToWorkList(Dir);
            for (int i = 0; i < goal.Count; ++i)
            {
                sb.AppendLine($"goalArea[{i}],{goal[i].X:F5},{goal[i].Y:F5}");
            }

            var work = WorkArea.Shape.ToClockWise().Points.ReorderPointsToWorkList(Dir);
            for (int i = 0; i < work.Count; ++i)
            {
                sb.AppendLine($"workArea[{i}],{work[i].X:F5} , {work[i].Y:F5}");
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
    }


    /// <summary>
    /// アクティビティモデルに関する拡張メソッド
    /// </summary>
    public static class ActivityExtensions
    {
        public static IList<Vec2> ReorderPointsToWorkList(this IReadOnlyList<Vec2> points, double dirRadian)
        {
            var temp = points
                .Select(p => p.Rot(-dirRadian))
                .ToList();

            int p = 0;
            Vec2 start = temp.First();
            for (int i = 0; i < temp.Count; ++i)
            {
                if (Utils.IsPositive(temp[i].Y - start.Y))
                {
                    p = i;
                    start = temp[i];
                    continue;
                }

                if (Utils.IsZero(temp[i].Y - start.Y) && Utils.IsNegative(temp[i].X - start.X))
                {
                    p = i;
                    start = temp[i];
                    continue;
                }
            }

            return Enumerable.Range(0, temp.Count)
                .Select(i => (i + p) % temp.Count)
                .Select(i => points[i])
                .ToList();
        }
    }
}
