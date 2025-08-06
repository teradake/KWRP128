using KWRP.Avalonia.Backend.Services;
using KWRP.Infra;
using System;
using System.Text.Json.Serialization;

namespace KWRP.Avalonia.Frontend.Models.Settings
{
    public class LaneArrangementConfigs
    {
        [JsonPropertyName("FitLanesToArea")] public bool EnableLapAdjustment { get; set; } = true;

        /// <summary>
        /// 最適な方向探索用。何度刻みで探索するか
        /// </summary>
        [JsonIgnore] public double IntervalAngleDegree { get; set; } = 0.01;

        /// <summary>
        /// 最適な方向探索用。±何度の範囲を調べるか
        /// </summary>
        [JsonIgnore] public double RangeAngleDegree { get; set; } = 4.0;

        [JsonIgnore] public bool EnableOptimize { get; set; } = true;

        [JsonPropertyName("PairLengthDiffFront")] public double LaneGapToleranceFront { get; set; } = 15.0;

        [JsonPropertyName("PairLengthDiffRear")] public double LaneGapToleranceRear { get; set; } = 5.0;
        [JsonPropertyName("VertexSimplificationDistanceMeter")] public double VertexSimplificationDistance { get; set; } = 0.2;
                
    }
}
