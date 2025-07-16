using KWRP.Avalonia.Backend.Enums;
using Trdk.Geometry;
using Trdk.Geometry.NTS;

namespace KWRP.Avalonia.Backend.Model.Shapes.WorkArea
{

    /// <summary>
    /// 作業エリア
    /// 引数が多くて大変なので、このアプリケーション上ではインスタンス化はサービスクラスに任せる。
    /// </summary>
    public class WorkAreaModel : IShape
    {
        private readonly Polygon _polygon;
        private readonly Polygon[] _lanes;
        private readonly Polygon[] _orthogonalLanesOriginal;
        private readonly Polygon[] _orthogonalLanes;
        private readonly WorkAreaAttribute _attribute;
        private readonly bool _headToRight;

        private readonly WorkAreaOrientation _orientation;
        private readonly WorkAreaLaneTrimType _upperTrimType;
        private readonly WorkAreaLaneTrimType _lowerTrimType;
        private readonly RollerWheelType _rollerWheelType;
        private readonly double _wheelbase;
        private readonly double _upMargin;
        private readonly double _downMargin;
        private readonly bool _isEdgeUp;
        private readonly bool _isEdgeDown;

         public Polygon Shape => _polygon;
        public GoalAreaModel GoalArea { get; }
        public Seg2 Arrow { get; }
        public Vec2 InfoCardPos { get; }

        public WorkAreaAttribute Attribute => _attribute;
        public double ProgressDirectionRad { get; }
        public double HeadingDirectionRad { get; }
        public bool IsEdgeFront { get; }
        public bool IsEdgeRear { get; }
        public double FrontMargin { get; }
        public double RearMargin { get; }

        public IList<Polygon> OrthogonalLanesOriginal => _orthogonalLanesOriginal;
        public IList<Polygon> OrthogonalLanes => _orthogonalLanes;
        public double Xmax => _orthogonalLanes[^1].AaBB.Xmax;
        public double Xmin => _orthogonalLanes[0].AaBB.Xmin;
        public double Ymax => _orthogonalLanes.Max(lane => lane.AaBB.Ymax);
        public double Ymin => _orthogonalLanes.Min(lane => lane.AaBB.Ymin);
        public double Width => Xmax - Xmin;
        public double Height => Ymax - Ymin;
        public WorkAreaOrientation Orientation => _orientation;


        public WorkAreaModel(
            Polygon[] orthogonalLanesOriginal,
            WorkAreaAttribute attribute,
            bool headToRight,
            double progressDirectionRad,
            RollerWheelType wheelType,
            double wheelbase,
            double frontMargin,
            double rearMargin,
            bool isEdgeUp = true,
            bool isEdgeDown = true)
        {
            // set basic attribute
            _orthogonalLanesOriginal = orthogonalLanesOriginal;
            _attribute = attribute;
            _headToRight = headToRight;
            _rollerWheelType = wheelType;
            _wheelbase = wheelbase;
            _isEdgeDown = isEdgeDown;
            _isEdgeUp = isEdgeUp;
            FrontMargin = frontMargin;
            RearMargin = rearMargin;

            // set orientation
            {
                _orientation = headToRight
                    ? WorkAreaOrientation.Down
                    : WorkAreaOrientation.Up;
                if (_attribute.IsDirectionSwap)
                {
                    _orientation = _orientation.Swap();
                }
            }

            // set edge flag
            {
                if (_orientation == WorkAreaOrientation.Down)
                {
                    (isEdgeUp, isEdgeDown) = (isEdgeDown, isEdgeUp);
                }
                IsEdgeFront = isEdgeUp;
                IsEdgeRear = isEdgeDown;
            }

            // set margin
            {
                _upMargin = _orientation == WorkAreaOrientation.Up
                    ? frontMargin
                    : rearMargin;
                _downMargin = _orientation == WorkAreaOrientation.Up
                    ? rearMargin
                    : frontMargin;
                if (!_isEdgeUp) _upMargin = 0;
                if (!_isEdgeDown) _downMargin = 0;
            }

            // set trim type
            {
                _upperTrimType = _orientation == WorkAreaOrientation.Up
                    ? _attribute.LaneTrimTypeFront
                    : _attribute.LaneTrimTypeRear;
                _lowerTrimType = _orientation == WorkAreaOrientation.Up
                    ? _attribute.LaneTrimTypeRear
                    : _attribute.LaneTrimTypeFront;
            }

            // set workarea shape
            {
                _orthogonalLanes = InitializeLanes().ToArray();
                _lanes = _orthogonalLanes.Select(lane => lane.Rotate(progressDirectionRad)).ToArray();
                _polygon = PolygonMerger.Merge(_lanes);
            }

            // set goalarea
            GoalArea = InitializeGoal(progressDirectionRad);

            // set arrow
            {
                ProgressDirectionRad = progressDirectionRad;
                HeadingDirectionRad = progressDirectionRad + 0.5 * (_orientation == WorkAreaOrientation.Down ? -Math.PI : Math.PI);
                Arrow = Seg2.Create(_polygon.Centroid, 6.0, HeadingDirectionRad);
            }

            // set origin
            {
                var o = _orientation == WorkAreaOrientation.Down ? Arrow.Dst : Arrow.Src;
                var w = Math.Min(Width, 5.8);
                var d = new Vec2(progressDirectionRad);
                InfoCardPos = o - 0.4 * w * d;
            }
        }

        GoalAreaModel InitializeGoal(double progressDirectionRad)
        {
            var lane = _attribute.GoalAreaLocation == GoalAreaLocation.Prev
                ? _orthogonalLanes.First()
                : _orthogonalLanes.Last();
            var box = lane.AaBB;

            if (_orientation == WorkAreaOrientation.Down)
            {
                box = box.YMirrored;
            }

            var ymax = _rollerWheelType == RollerWheelType.Single
                ? box.Ymin
                : box.Ymin + _wheelbase;

            var goal = new BoundingBox
            {
                Xmin = box.Xmin,
                Xmax = box.Xmax,
                Ymin = ymax - _wheelbase,
                Ymax = ymax,
            };

            if (_orientation == WorkAreaOrientation.Down)
            {
                goal = goal.YMirrored;
            }
            return new GoalAreaModel(goal.ToPolygon().Rotate(progressDirectionRad));
        }


        IEnumerable<Polygon> InitializeLanes()
        {
            try
            {
                var lanes = _orthogonalLanesOriginal
                    .Select(lane => lane.AaBB)
                    .Select(box => new BoundingBox
                    {
                        Xmin = box.Xmin,
                        Xmax = box.Xmax,
                        Ymin = box.Ymin + _downMargin,
                        Ymax = box.Ymax - _upMargin,
                    })
                    .Select(box => box.ToPolygon())
                    .ToList();

                var upper = _upperTrimType == WorkAreaLaneTrimType.Cut
                    ? lanes.Min(lane => lane.AaBB.Ymax)
                    : double.MaxValue;

                var lower = _lowerTrimType == WorkAreaLaneTrimType.Cut
                    ? lanes.Max(lane => lane.AaBB.Ymin)
                    : double.MinValue;

                if (Utils.IsZero(upper - lower))
                {
                    throw new Exception();
                }

                return lanes
                    .Select(lane => lane.AaBB.ClampByY(lower, upper))
                    .Select(aabb => aabb.ToPolygon());
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                throw;
            }
        }


        public WorkAreaModel CloneBy(WorkAreaAttribute attribute)
        {
            return new WorkAreaModel(
                orthogonalLanesOriginal: this._orthogonalLanesOriginal,
                attribute: attribute,
                headToRight: this._headToRight,
                progressDirectionRad: this.ProgressDirectionRad,
                wheelType: this._rollerWheelType,
                wheelbase: this._wheelbase,
                frontMargin: this.FrontMargin,
                rearMargin: this.RearMargin,
                isEdgeDown: this._isEdgeDown,
                isEdgeUp: this._isEdgeUp);
        }
    }

}
