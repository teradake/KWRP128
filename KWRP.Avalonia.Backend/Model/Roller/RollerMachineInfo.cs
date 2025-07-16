using KWRP.Avalonia.Backend.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KWRP.Avalonia.Backend.Models.Roller
{
    public class RollerMachineInfo : IEquatable<RollerMachineInfo>
    {
        /// <summary>
        /// ローラー種別。片鉄輪:Single、両鉄輪:Tandem
        /// </summary>
        [JsonPropertyName("WheelType: Single:0 Tandem:1")]
        public RollerWheelType RollerWheelType { get; set; }

        /// <summary>
        /// 鉄輪幅
        /// </summary>
        [JsonPropertyName("LaneWidth")]
        public double LaneWidth { get; set; }

        /// <summary>
        /// ホイールベース。前方と後方の車輪中心間の距離のこと
        /// </summary>
        [JsonPropertyName("Wheelbase")]
        public double Wheelbase { get; set; }

        /// <summary>
        /// フロントオーバーハング。車両の前部バンパーからフロントホイールの中心までの距離のこと
        /// </summary>
        [JsonPropertyName("FrontOverhang")]
        public double FrontOverhang { get; set; }

        /// <summary>
        /// リアオーバーハング。車両の後部バンパーからリアホイールの中心までの距離のこと
        /// </summary>
        [JsonPropertyName("RearOverhang")]
        public double RearOverhang { get; set; }


        /// <summary>
        /// デフォルトのインスタンスを作成するための静的メソッド
        /// </summary>
        /// <returns></returns>
        public static RollerMachineInfo CreateDefault()
        {
            return new RollerMachineInfo
            {
                RollerWheelType = RollerWheelType.Tandem,
                LaneWidth = 2.13,
                FrontOverhang = 0.0,
                RearOverhang = 0.0,
                Wheelbase = 2.0,
            };
        }

        /// <summary>
        /// 今のモデルをクローンする
        /// </summary>
        /// <returns></returns>
        public RollerMachineInfo Clone()
        {
            return new RollerMachineInfo
            {
                RollerWheelType = this.RollerWheelType,
                LaneWidth = this.LaneWidth,
                Wheelbase = this.Wheelbase,
                FrontOverhang = this.FrontOverhang,
                RearOverhang = this.RearOverhang,
            };
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as RollerMachineInfo);
        }

        public bool Equals(RollerMachineInfo? other)
        {
            return other != null &&
                   RollerWheelType == other.RollerWheelType &&
                   LaneWidth == other.LaneWidth &&
                   Wheelbase == other.Wheelbase &&
                   FrontOverhang == other.FrontOverhang &&
                   RearOverhang == other.RearOverhang;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hashCode = (int)RollerWheelType;
                hashCode = (hashCode * 397) ^ LaneWidth.GetHashCode();
                hashCode = (hashCode * 397) ^ Wheelbase.GetHashCode();
                hashCode = (hashCode * 397) ^ FrontOverhang.GetHashCode();
                hashCode = (hashCode * 397) ^ RearOverhang.GetHashCode();
                return hashCode;
            }
        }
    }
}
