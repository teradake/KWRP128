using System;
using System.Text.Json.Serialization;

namespace KWRP.Avalonia.Backend.Models.Roller
{
    public class RollerZone : IEquatable<RollerZone>
    {
        /// <summary>
        /// 前後方向マージン(m): 前後方向の障害物検出範囲
        /// </summary>
        [JsonPropertyName("FrontRearMargin")]
        public double FrontRearMargin { get; set; }

        /// <summary>
        /// 左右方向マージン(m): 左右方向の障害物検出範囲
        /// </summary>
        [JsonPropertyName("LeftRightMargin")]
        public double LeftRightMargin { get; set; }

        /// <summary>
        /// 作業可能長さ(m): レーンの最小長さ
        /// </summary>
        [JsonPropertyName("LaneChangeLength")]
        public double LaneChangeLength { get; set; }

        /// <summary>
        /// ラップ幅(m): 2つのレーンの重なり量
        /// </summary>
        [JsonPropertyName("LapWidth")]
        public double LapWidth { get; set; }

        /// <summary>
        /// 前方端部余裕代(m): 端部アクティビティの前方側占有範囲調整量
        /// </summary>
        [JsonPropertyName("FrontEdgeOffset")]
        public double FrontEdgeOffset { get; set; }

        /// <summary>
        /// 後方端部余裕代(m): 端部アクティビティの後方側占有範囲調整量
        /// </summary>
        [JsonPropertyName("BackEdgeOffset")]
        public double BackEdgeOffset { get; set; }

        /// <summary>
        /// 転圧エリアラップ調整値(m): 作業エリアを前後方向でどれだけラップさせるか
        /// </summary>
        [JsonPropertyName("WorkAreaLapLength")]
        public double WorkAreaLapLength { get; set; }

        /// <summary>
        /// 初期配置用拡幅幅 進捗方向側(m): 初期配置用のレーン間移動アクティビティの進捗方向側の幅をどれだけ増やすか
        /// </summary>
        [JsonPropertyName("InitialMoveMarginNext")]
        public double InitialMoveMarginNext { get; set; }

        /// <summary>
        /// 初期配置用拡幅幅 進捗方向反対側(m): 初期配置用のレーン間移動アクティビティの進捗方向反対側の幅をどれだけ増やすか
        /// </summary>
        [JsonPropertyName("InitialMoveMarginPrev")]
        public double InitialMoveMarginPrev { get; set; }

        /// <summary>
        /// 初期配置用拡幅幅 前方(m): 初期配置用のレーン間移動アクティビティの進捗方向側の幅をどれだけ増やすか
        /// </summary>
        [JsonPropertyName("InitialMoveMarginFront")]
        public double InitialMoveMarginFront { get; set; }

        /// <summary>
        /// 初期配置用拡幅幅 後方(m): 初期配置用のレーン間移動アクティビティの進捗方向反対側の幅をどれだけ増やすか
        /// </summary>
        [JsonPropertyName("InitialMoveMarginBack")]
        public double InitialMoveMarginBack { get; set; }


        /// <summary>
        /// デフォルトのインスタンスを作成するための静的メソッド
        /// </summary>
        /// <returns></returns>
        public static RollerZone CreateDefault()
        {
            return new RollerZone
            {
                FrontRearMargin = 1.5,
                LeftRightMargin = 1.5,
                LaneChangeLength = 15.0,
                LapWidth = 0.2,
                FrontEdgeOffset = 0,
                BackEdgeOffset = 0,
                WorkAreaLapLength = 0.5,
                InitialMoveMarginNext = 6,
                InitialMoveMarginPrev = 0.5,
                InitialMoveMarginFront = 2,
                InitialMoveMarginBack = 3,
            };
        }

        public RollerZone Clone()
        {
            return new RollerZone
            {
                FrontRearMargin = FrontRearMargin,
                LeftRightMargin = LeftRightMargin,
                LaneChangeLength = LaneChangeLength,
                LapWidth = LapWidth,
                FrontEdgeOffset = FrontEdgeOffset,
                BackEdgeOffset = BackEdgeOffset,
                WorkAreaLapLength = WorkAreaLapLength,
                InitialMoveMarginNext = InitialMoveMarginNext,
                InitialMoveMarginPrev = InitialMoveMarginPrev,
                InitialMoveMarginFront = InitialMoveMarginFront,
                InitialMoveMarginBack = InitialMoveMarginBack,
            };
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as RollerZone);
        }

        public bool Equals(RollerZone? other)
        {
            return other != null &&
                   FrontRearMargin == other.FrontRearMargin &&
                   LeftRightMargin == other.LeftRightMargin &&
                   LaneChangeLength == other.LaneChangeLength &&
                   LapWidth == other.LapWidth &&
                   FrontEdgeOffset == other.FrontEdgeOffset &&
                   BackEdgeOffset == other.BackEdgeOffset &&
                   WorkAreaLapLength == other.WorkAreaLapLength &&
                   InitialMoveMarginNext == other.InitialMoveMarginNext &&
                   InitialMoveMarginPrev == other.InitialMoveMarginPrev &&
                   InitialMoveMarginBack == other.InitialMoveMarginBack &&
                   InitialMoveMarginFront == other.InitialMoveMarginFront;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hashCode = FrontRearMargin.GetHashCode();
                hashCode = (hashCode * 397) ^ LeftRightMargin.GetHashCode();
                hashCode = (hashCode * 397) ^ LaneChangeLength.GetHashCode();
                hashCode = (hashCode * 397) ^ LapWidth.GetHashCode();
                hashCode = (hashCode * 397) ^ FrontEdgeOffset.GetHashCode();
                hashCode = (hashCode * 397) ^ BackEdgeOffset.GetHashCode();
                hashCode = (hashCode * 397) ^ WorkAreaLapLength.GetHashCode();
                hashCode = (hashCode * 397) ^ InitialMoveMarginNext.GetHashCode();
                hashCode = (hashCode * 397) ^ InitialMoveMarginPrev.GetHashCode();
                hashCode = (hashCode * 397) ^ InitialMoveMarginFront.GetHashCode();
                hashCode = (hashCode * 397) ^ InitialMoveMarginBack.GetHashCode();
                return hashCode;
            }
        }

        public string ToStringInitialMargin()
        {
            return $"front: {InitialMoveMarginFront}\n" +
                $"back: {InitialMoveMarginBack}\n" +
                $"next: {InitialMoveMarginNext}\n" +
                $"prev: {InitialMoveMarginPrev}";
        }
    }
}
