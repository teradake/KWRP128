using KWRP.Avalonia.Backend;
using KWRP.Avalonia.Backend.Model.Shapes;
using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Frontend.ViewModels.Shapes;
using ObservableCollections;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using Trdk.Geometry;
using Trdk.Geometry.NTS;

namespace KWRP.Avalonia.Frontend.Models.Stores
{
    public class CanvasItemStore
    {
        private readonly ILogService _logService;

        public CanvasItemStore(
            ILogService logService)
        {
            _logService = logService;

            AreaOfCompactionArea = CompactionAreas
                .ObserveCountChanged()
                .Select(_ => CompactionAreas.CalculateMergedArea(Holes))
                .ToReadOnlyReactiveProperty();
            
            LanesCount = PairedLanes
                .ObserveCountChanged()
                .ThrottleFirstLast(TimeSpan.FromMilliseconds(500))
                .Select(_ => PairedLanes.Where(p => p != null).SelectMany(p => p.OrthogonalLanes).Count())
                .ToReadOnlyReactiveProperty();
            AreaOfLanes = PairedLanes
                .ObserveCountChanged()
                .ThrottleFirstLast(TimeSpan.FromMilliseconds(500))
                .Select(_ => PairedLanes.Where(p => p != null).Select(p => p.Shape).CalculateMergedArea())
                .ToReadOnlyReactiveProperty();

            DragRect = new ReactiveProperty<DragRect>(new(Vec2.Origin, Vec2.Origin, isVisible: false));

            RulerPoints
                .ObserveAdd()
                .Where(e => e.Index > 0)
                .Subscribe(e => RulerLinks.Add(Seg2.Create(RulerPoints[e.Index - 1], e.Value)));

            RulerPoints
                .ObserveClear()
                .Subscribe(e => RulerLinks.Clear());

            // エリア描画用のAaBB更新処理
            CompactionAreas
                .ObserveCountChanged()
                .Subscribe(_ =>
                {
                    if (CompactionAreas.Count > 0)
                    {
                        var box = BoundingBox.Identity;
                        foreach (var area in CompactionAreas)
                        {
                            box = box.Update(area);
                        }
                        AreaBoundingBox.Value = box.Expand(5, 5);
                    }
                    else
                    {
                        AreaBoundingBox.Value = null;
                    }
                });

            // エリアリセット時に、エリアに依存する情報もクリアする
            CompactionAreas
                .ObserveReset()
                .Where(p => p.IsClear)
                .Subscribe(_ =>
                {
                    Holes.Clear();
                    OffsetCompactionAreas.Clear();
                    TargetPolygons.Clear();
                    CreatedLaneResults.Clear();
                    Lanes.Clear();
                });

            Lanes
                .ObserveReset()
                .Where(p => p.IsClear)
                .Subscribe(_ =>
                {
                    PairedLanes.Clear();
                });

            Lanes
                .ObserveCountChanged()
                .Where(cnt => cnt > 0)
                .Subscribe(_ => LanesUpdated = true);

            CreatedLaneResults
                .ObserveAdd()
                .Subscribe(result =>
                {
                    Lanes.AddRange(result.Value.Lanes);
                    PairedLanes.AddRange(result.Value.PairedLanes);
                });

            _logService.LogDebug("init");
        }

        public ReactiveProperty<DragRect> DragRect { get; }

        public ReactiveProperty<ArrowViewModel> DirectionArrow { get; } = new(new ArrowViewModel(0, 0, 0, 0, isVisible: false));


        public ObservableList<Vec2> RulerPoints { get; } = [];
        public ObservableList<Seg2> RulerLinks { get; } = [];

        public ObservableList<Polygon> CompactionAreas { get; } = [];
        public ObservableList<Polygon> Holes { get; } = [];
        public ObservableList<Polygon> OffsetCompactionAreas { get; } = [];
        public ObservableList<PolygonWithHoles> TargetPolygons { get; } = [];
        public ObservableList<CreatedLaneResult> CreatedLaneResults { get; } = [];
        public ObservableList<LaneModel> Lanes { get; } = [];
        public ObservableList<PairedLaneModel> PairedLanes { get; } = [];
        public bool LanesUpdated { get; set; }
        public ReactiveProperty<BoundingBox?> AreaBoundingBox { get; private set; } = new(BoundingBox.Identity);

        public ReadOnlyReactiveProperty<double> AreaOfCompactionArea { get; } 
        public ReadOnlyReactiveProperty<double> AreaOfLanes { get; } 
        public ReadOnlyReactiveProperty<int> LanesCount { get; }
    }
}
