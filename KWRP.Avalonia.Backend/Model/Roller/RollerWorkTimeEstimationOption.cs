using KWRP.Avalonia.Backend.Models.Roller;
using System.Text.Json.Serialization;

namespace KWRP.Avalonia.Backend.Models.Roller
{
    public class WorkTimeEstimationOptions
    {
        [JsonIgnore] public static double MeanSwitchbackTime => 3.0;
        [JsonIgnore] public static double MeanTravelTime => 60.0;
        [JsonIgnore] public static double MeanChokobumiTime => 15.0;
        [JsonIgnore] public static double MeanKubireLength => 16.0;

        [JsonPropertyName("No-Compaction")] public WorkTimeEstimationFlags NonCompaction { get; init; }
        [JsonPropertyName("Compaction")] public WorkTimeEstimationFlags Compaction { get; init; }

        [JsonIgnore] public bool IsDefault { get; set; } = false;

        public WorkTimeEstimationOptions()
        {
            NonCompaction = new WorkTimeEstimationFlags();
            Compaction = new WorkTimeEstimationFlags();
        }

        public static WorkTimeEstimationOptions CreateDefault()
        {
            return new WorkTimeEstimationOptions()
            {
                IsDefault = true,
            };
        }

        public WorkTimeEstimationOptions Clone()
        {
            return new WorkTimeEstimationOptions
            {
                NonCompaction = new WorkTimeEstimationFlags
                {
                    EnableChokobumi = this.NonCompaction.EnableChokobumi,
                    EnableKubire = this.NonCompaction.EnableKubire
                },
                Compaction = new WorkTimeEstimationFlags
                {
                    EnableChokobumi = this.Compaction.EnableChokobumi,
                    EnableKubire = this.Compaction.EnableKubire
                }
            };
        }
    }

    public class WorkTimeEstimationFlags
    {
        [JsonPropertyName("Chokobumi")] public bool EnableChokobumi { get; set; } = true;
        [JsonPropertyName("Kubire")] public bool EnableKubire { get; set; } = true;
    }
}
