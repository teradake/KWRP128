using KWRP.Avalonia.Backend.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KWRP.Avalonia.Backend.Models.Roller
{
    public class RollerModel : IEquatable<RollerModel>
    {
        /// <summary>
        /// 振動ローラ名称
        /// </summary>
        [JsonPropertyName("Name")]
        public string MachineName { get; init; }

        /// <summary>
        /// 説明用
        /// </summary>
        [JsonPropertyName("Description")]
        public string Description { get; init; }

        /// <summary>
        /// ローラー車体情報
        /// </summary>
        [JsonPropertyName("DimensionalInfo")]
        public RollerMachineInfo MachineInfo { get; init; }


        /// <summary>
        /// 作業エリア生成パラメータ
        /// </summary>
        [JsonPropertyName("ActivityAreaInfo")]
        public RollerZone Zone { get; init; }


        /// <summary>
        /// レーン間移動アクティビティのパラメータ
        /// </summary>
        [JsonPropertyName("Move ActivityParameter")]
        public RollerMovement MoveParameter { get; init; }

        /// <summary>
        /// 無起振転圧アクティビティのパラメータ
        /// </summary>
        [JsonPropertyName("No-Compaction ActivityParameter")]
        public RollerMovement NonCompactionParameter { get; init; }

        /// <summary>
        /// 起振転圧アクティビティのパラメータ
        /// </summary>
        [JsonPropertyName("Compaction ActivityParameter")]
        public RollerMovement CompactionParameter { get; init; }


        /// <summary>
        /// 前方余裕(m) : 占有エリアの前方側のマージン
        /// </summary>
        [JsonIgnore]
        public double FrontAllowance => Zone.FrontRearMargin + MachineInfo.FrontOverhang;

        /// <summary>
        /// 後方余裕(m) : 占有エリアの後方側のマージン
        /// </summary>
        [JsonIgnore]
        public double RearAllowance => Zone.FrontRearMargin + MachineInfo.RearOverhang
                                     + (MachineInfo.RollerWheelType == RollerWheelType.Single ? MachineInfo.Wheelbase : 0);

        /// <summary>
        /// 左右余裕(m) : 占有エリアのの左右方向のマージン
        /// </summary>
        [JsonIgnore]
        public double LeftRightAllowance => Zone.LeftRightMargin;

        /// <summary>
        /// デフォルトのインスタンスを作成するための静的メソッド
        /// </summary>
        /// <returns></returns>
        public static RollerModel CreateDefault()
        {
            return new RollerModel
            {
                MachineName = "CS56B: Default",
                Description = "Rollerのデータファイルが見つからない場合に適用されるデフォルトのローラーです。寸法の単位はm、Speedの単位はkm/hです",
                MachineInfo = RollerMachineInfo.CreateDefault(),
                Zone = RollerZone.CreateDefault(),
                MoveParameter = RollerMovement.CreateDefault(),
                NonCompactionParameter = RollerMovement.CreateDefault(),
                CompactionParameter = RollerMovement.CreateDefault(),
            };
        }

        public RollerModel Clone()
        {
            return new RollerModel
            {
                MachineName = MachineName,
                Description = Description,
                MachineInfo = MachineInfo.Clone(),
                Zone = Zone.Clone(),
                MoveParameter = MoveParameter.Clone(),
                NonCompactionParameter = NonCompactionParameter.Clone(),
                CompactionParameter = CompactionParameter.Clone(),
            };
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as RollerModel);
        }

        public bool Equals(RollerModel? other)
        {
            return other != null &&
                   (MachineInfo == null && other.MachineInfo == null || MachineInfo != null && MachineInfo.Equals(other.MachineInfo)) &&
                   (Zone == null && other.Zone == null || Zone != null && Zone.Equals(other.Zone)) &&
                   (MoveParameter == null && other.MoveParameter == null || MoveParameter != null && MoveParameter.Equals(other.MoveParameter)) &&
                   (NonCompactionParameter == null && other.NonCompactionParameter == null || NonCompactionParameter != null && NonCompactionParameter.Equals(other.NonCompactionParameter)) &&
                   (CompactionParameter == null && other.CompactionParameter == null || CompactionParameter != null && CompactionParameter.Equals(other.CompactionParameter));
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hashCode = 0;
                hashCode = (hashCode * 397) ^ (MachineInfo != null ? MachineInfo.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Zone != null ? Zone.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (MoveParameter != null ? MoveParameter.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (NonCompactionParameter != null ? NonCompactionParameter.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (CompactionParameter != null ? CompactionParameter.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
