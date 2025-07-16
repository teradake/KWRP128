using Trdk.Geometry;

namespace KWRP.Avalonia.Backend.Model.Shapes
{
    /// <summary>
    /// レーン割結果（生成されたレーン、生成されたペアレーン、スコア）を格納したクラス。
    /// </summary>
    public class CreatedLaneResult
    {
        private readonly PolygonWithHoles _sourcePolygon;
        private readonly List<LaneModel> _lanes;
        private readonly List<PairedLaneModel> _pairedLanes;

        public IEnumerable<LaneModel> Lanes => _lanes;
        public IEnumerable<PairedLaneModel> PairedLanes => _pairedLanes;

        private CreatedLaneResult(PolygonWithHoles polygonWithHoles)
        {
            _sourcePolygon = polygonWithHoles;
            _lanes = [];
            _pairedLanes = [];
        }

        public static CreatedLaneResult By(PolygonWithHoles targetPolygon, IEnumerable<LaneModel> lanes, IEnumerable<PairedLaneModel> pairedLanes)
        {
            var instance = new CreatedLaneResult(targetPolygon);
            instance._lanes.AddRange(lanes);
            instance._pairedLanes.AddRange(pairedLanes);
            return instance;
        }
    }
}
