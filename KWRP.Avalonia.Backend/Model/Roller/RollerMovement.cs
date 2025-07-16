using System;
using System.Text.Json.Serialization;

namespace KWRP.Avalonia.Backend.Models.Roller
{
    public class RollerMovement : IEquatable<RollerMovement>
    {
        [JsonPropertyName("RefSpeed")]
        public double RefSpeed { get; set; }

        [JsonPropertyName("EdgeSpeed")]
        public double EdgeSpeed { get; set; }

        [JsonPropertyName("RepeatNum")]
        public int RepeatNum { get; set; }

        public static RollerMovement CreateDefault()
        {
            return new RollerMovement
            {
                RefSpeed = 3.0,
                EdgeSpeed = 1.5,
                RepeatNum = 1,
            };
        }

        public RollerMovement Clone()
        {
            return new RollerMovement
            {
                RefSpeed = this.RefSpeed,
                EdgeSpeed = this.EdgeSpeed,
                RepeatNum = this.RepeatNum,
            };
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as RollerMovement);
        }

        public bool Equals(RollerMovement? other)
        {
            return other != null &&
                   RefSpeed == other.RefSpeed &&
                   EdgeSpeed == other.EdgeSpeed &&
                   RepeatNum == other.RepeatNum;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hashCode = RefSpeed.GetHashCode();
                hashCode = (hashCode * 397) ^ EdgeSpeed.GetHashCode();
                hashCode = (hashCode * 397) ^ RepeatNum.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"(repeatNum, refSpeed, edgeSpeed) = ({RepeatNum}, {RefSpeed:f3}, {EdgeSpeed:f3})";
        }
    }
}
