using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KWRP.Avalonia.Backend.Model.Paramter
{
    public class VRParameterModel
    {
        public VRParameterModel() { }
        public VRParameterModel(
            double laneWidth,
            double frontAllowance,
            double rearAllowance,
            double sideAllowance,
            double offset,
            double lapWidth,
            double laneChangeLength,
            double laneProgressDirection,
            bool isRollerHeadingRight
            )
        {
            LaneWidth = laneWidth;
            FrontAllowance = frontAllowance;
            RearAllowance = rearAllowance;
            SideAllowance = sideAllowance;
            Offset = offset;
            LapWidth = lapWidth;
            LaneChangeLength = laneChangeLength;
            LaneProgressDirection = laneProgressDirection;
            IsRollerHeadingRight = isRollerHeadingRight;
        }

        /// <summary>
        /// レーン幅(m) : レーン割する際の基準幅
        /// </summary>
        [JsonPropertyName("LaneWidth")]
        public double LaneWidth { get; init; }

        /// <summary>
        /// 前方余裕代(m) : レーン前側と転圧領域境界まで離隔する距離
        /// </summary>
        [JsonPropertyName("FrontAllowance")]
        public double FrontAllowance { get; init; }

        /// <summary>
        /// 後方余裕代(m) : レーン後ろ側と転圧領域境界まで離隔する距離
        /// </summary>
        [JsonPropertyName("RearAllowance")]
        public double RearAllowance { get; init; }

        /// <summary>
        /// 側方余裕代(m) : 転圧領域からオフセットする距離
        /// </summary>
        [JsonPropertyName("SideAllowance")]
        public double SideAllowance { get; set; }

        /// <summary>
        /// 初期側方余裕代(m) : レーン割開始位置をどれだけずらすか
        /// </summary>
        [JsonPropertyName("Offset")]
        public double Offset { get; set; }

        /// <summary>
        /// ラップ幅(m) : 隣り合うレーンをどれだけかぶらせるか
        /// </summary>
        [JsonPropertyName("LapWidth")]
        public double LapWidth { get; init; }

        /// <summary>
        /// 作業可能長さ(m) : レーン長さの下限値
        /// </summary>
        [JsonPropertyName("LaneChangeLength")]
        public double LaneChangeLength { get; init; }

        /// <summary>
        /// 作業進捗方向(deg) : 作業がこの方向に進むイメージ
        /// </summary>
        [JsonPropertyName("LaneProgressDirection")]
        public double LaneProgressDirection { get; set; }

        [JsonPropertyName("IsRollerHeadingRight")]
        public bool IsRollerHeadingRight { get; set; }

        
        [JsonIgnore()]
        public double RollerHeading
            => LaneProgressDirection + (IsRollerHeadingRight ? -Math.PI / 2 : +Math.PI / 2);

        public VRParameterModel Clone()
        {
            return new VRParameterModel(
                LaneWidth, FrontAllowance, RearAllowance, SideAllowance,
                Offset, LapWidth, LaneChangeLength,
                LaneProgressDirection, IsRollerHeadingRight);
        }
    }
}
