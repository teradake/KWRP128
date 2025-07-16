using KWRP.Avalonia.Backend.Enums;
using KWRP.Avalonia.Backend.Model.Shapes;
using KWRP.Avalonia.Backend.Model.Shapes.Activity;
using KWRP.Avalonia.Backend.Model.Shapes.WorkArea;
using KWRP.Avalonia.Backend.Models.Roller;
using Trdk.Geometry;
using Trdk.Geometry.NTS;

namespace KWRP.Avalonia.Backend.Services.Activity
{
    public class ActivityBuilder
    {
        enum Direction
        {
            Up, Down, Left, Right
        }

        private RollerModel? _roller = null;
        private WorkAreaModel? _prev = null;
        private WorkAreaModel? _current = null;
        private int activityID = 0;
        private readonly bool[] _edgeFlag;
        private readonly Dictionary<Direction, double> _margin;

        public ActivityBuilder()
        {
            _edgeFlag = [true, true];
            _margin = new Dictionary<Direction, double>
            {
                { Direction.Up, 0.0 },
                { Direction.Down, 0.0 },
                { Direction.Left, 0.0 },
                { Direction.Right, 0.0 }
            };
        }

        public ActivityBuilder SetRoller(RollerModel roller)
        {
            _roller = roller;
            return this;
        }

        public ActivityBuilder SetCurrentWorkArea(WorkAreaModel workArea)
        {
            _prev = _current;
            _current = workArea;

            // seg edge flag
            {
                _edgeFlag[0] = workArea.IsEdgeFront;
                _edgeFlag[1] = workArea.IsEdgeRear;
            }
            return this;
        }

        /// <summary>
        /// 無起振アクティビティを作る
        /// </summary>
        /// <param name="pathId">グループ番号</param>
        /// <param name="areaId">グループのうち何番目の「転圧」アクティビティかを示す番号</param>
        /// <returns></returns>
        public ActivityModel BuildNonCompactionActivity(int pathId, int areaId)
        {
            if (_current == null) throw new Exception("作業エリアが登録されていません");
            if (_roller == null) throw new Exception("ローラが登録されていません");

            // マージンの設定、ローラは上向き
            _margin[Direction.Up] = _roller.FrontAllowance + (_edgeFlag[0] ? _roller.Zone.FrontEdgeOffset : 0);
            _margin[Direction.Down] = _roller.RearAllowance + (_edgeFlag[1] ? _roller.Zone.BackEdgeOffset : 0);
            _margin[Direction.Left] = _roller.LeftRightAllowance;
            _margin[Direction.Right] = _roller.LeftRightAllowance;

            return new ActivityModel()
            {
                ActivityId = activityID++,
                GroupId = pathId,
                AreaID = areaId,
                ActivityType = RollerActivityType.NonCompaction,
                Length = _current.Height,
                Width = _current.Width,
                Dir = _current.HeadingDirectionRad,
                RefSpeed = _roller.NonCompactionParameter.RefSpeed,
                EdgeSpeed = _roller.NonCompactionParameter.EdgeSpeed,
                RepeatNum = _roller.NonCompactionParameter.RepeatNum,
                WorkArea = _current,
                GoalArea = _current,    // 転圧アクティビティの場合、ゴールエリアと作業エリアの形状を同じとする
                OccArea = new OccAreaModel(ExpandLane(_current, _margin)),
                Edge = [_edgeFlag[0], _edgeFlag[1]],
                InfoPos = _current.InfoCardPos,
                HeadToUp = _current.Orientation == WorkAreaOrientation.Up,
            };
        }

        public ActivityModel BuildCompactionActivity(int pathId, int areaId)
        {
            if (_current == null) throw new Exception("作業エリアが登録されていません");
            if (_roller == null) throw new Exception("ローラが登録されていません");

            // マージンの設定、ローラは上向き
            _margin[Direction.Up] = _roller.FrontAllowance + (_edgeFlag[0] ? _roller.Zone.FrontEdgeOffset : 0);
            _margin[Direction.Down] = _roller.RearAllowance + (_edgeFlag[1] ? _roller.Zone.BackEdgeOffset : 0);
            _margin[Direction.Left] = _roller.LeftRightAllowance;
            _margin[Direction.Right] = _roller.LeftRightAllowance;

            return new ActivityModel()
            {
                ActivityId = activityID++,
                GroupId = pathId,
                AreaID = areaId,
                ActivityType = RollerActivityType.Compaction,
                Length = _current.Height,
                Width = _current.Width,
                Dir = _current.HeadingDirectionRad,
                RefSpeed = _roller.CompactionParameter.RefSpeed,
                EdgeSpeed = _roller.CompactionParameter.EdgeSpeed,
                RepeatNum = _roller.CompactionParameter.RepeatNum,
                WorkArea = _current,
                GoalArea = _current,    // 転圧アクティビティの場合、ゴールエリアと作業エリアの形状を同じとする
                OccArea = new OccAreaModel(ExpandLane(_current, _margin)),
                Edge = [_edgeFlag[0], _edgeFlag[1]],
                InfoPos = _current.InfoCardPos,
                HeadToUp = _current.Orientation == WorkAreaOrientation.Up,
            };
        }

        public ActivityModel BuildMoveActivity(int pathId, int areaId)
        {
            if (_current == null) throw new Exception("作業エリアが登録されていません");
            if (_roller == null) throw new Exception("ローラが登録されていません");

            // マージンの設定、ローラは上向き
            _margin[Direction.Up] = _roller.FrontAllowance + (_edgeFlag[0] ? _roller.Zone.FrontEdgeOffset : 0);
            _margin[Direction.Down] = _roller.RearAllowance + (_edgeFlag[1] ? _roller.Zone.BackEdgeOffset : 0);
            _margin[Direction.Left] = _roller.LeftRightAllowance;
            _margin[Direction.Right] = _roller.LeftRightAllowance;

            if (_prev == null)
            {
                // 初回の移動アクティビティは、初期拡幅をマージンに反映
                _margin[Direction.Up] += _roller.Zone.InitialMoveMarginFront;
                _margin[Direction.Down] += _roller.Zone.InitialMoveMarginBack;
                _margin[Direction.Left] += _roller.Zone.InitialMoveMarginPrev;
                _margin[Direction.Right] += _roller.Zone.InitialMoveMarginNext;
            }

            var occ = _prev == null
                ? ExpandLane(_current, _margin)
                : ExpandLane(_prev, _current, _margin);

            return new ActivityModel()
            {
                ActivityId = activityID++,
                GroupId = pathId,
                AreaID = areaId,
                ActivityType = RollerActivityType.Move,
                Length = _current.Height,
                Width = _current.Width,
                Dir = _current.HeadingDirectionRad,
                RefSpeed = _roller.MoveParameter.RefSpeed,
                EdgeSpeed = _roller.MoveParameter.EdgeSpeed,
                RepeatNum = 1, 
                WorkArea = _current,
                GoalArea = _current.GoalArea,
                OccArea = new OccAreaModel(occ),
                Edge = [_edgeFlag[0], _edgeFlag[1]],
                InfoPos = _current.InfoCardPos,
                HeadToUp = _current.Orientation == WorkAreaOrientation.Up,
            };
        }

        Polygon ExpandLane(WorkAreaModel workArea, Dictionary<Direction, double> margin)
        {
            if (workArea.Orientation == WorkAreaOrientation.Down)
            {
                // marginの上下をスワップ
                (margin[Direction.Up], margin[Direction.Down]) = (margin[Direction.Down], margin[Direction.Up]);
            }

            var boxes = workArea
                .OrthogonalLanes
                .Select(lane => lane.AaBB)
                .Select(box =>
                {
                    return new BoundingBox
                    {
                        Xmin = box.Xmin - margin[Direction.Left],
                        Xmax = box.Xmax + margin[Direction.Right],
                        Ymin = box.Ymin - margin[Direction.Down],
                        Ymax = box.Ymax + margin[Direction.Up]
                    };
                })
                .Select(box => box.ToPolygon())
                .Select(lane => lane.Rotate(workArea.ProgressDirectionRad))
                .ToArray();

            if (workArea.Orientation == WorkAreaOrientation.Down)
            {
                // marginの上下をスワップ
                (margin[Direction.Up], margin[Direction.Down]) = (margin[Direction.Down], margin[Direction.Up]);
            }

            return PolygonMerger.Merge(boxes);
        }

        Polygon ExpandLane(WorkAreaModel prev, WorkAreaModel curr, Dictionary<Direction, double> margin)
        {
            var occPrev = ExpandLane(prev, margin);
            var occCurr = ExpandLane(curr, margin);
            return PolygonMerger.Merge([occPrev, occCurr]);
        }
    }
}
