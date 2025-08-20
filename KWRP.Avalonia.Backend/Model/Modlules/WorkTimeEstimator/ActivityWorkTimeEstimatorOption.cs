using KWRP.Avalonia.Backend.Models.Roller;
using System.Diagnostics;

namespace KWRP.Backend.Model.Modlules.WorkTimeEstimator
{
     [DebuggerDisplay("RefSpeed={RefSpeedKmPerHour} km/h, Travel={MeanTravelTime.TotalSeconds} s")]
    public sealed class ActivityWorkTimeEstimatorOption
    {
        /// <summary>折返し時間 [sec]</summary>
        public TimeSpan MeanSwitchbackTime { get; set; } = TimeSpan.FromSeconds(3.0);

        /// <summary>移動時間 [sec]</summary>
        public TimeSpan MeanTravelTime { get; set; } = TimeSpan.FromSeconds(60.0);

        /// <summary>ちょこ踏み時間 [sec]</summary>
        public TimeSpan MeanChokobumiTime { get; set; } = TimeSpan.FromSeconds(15.0);

        /// <summary>くびれ距離 [m]</summary>
        public double MeanKubireLengthMeter { get; set; } = 16.0;

        /// <summary>起振時にちょこ踏みを考慮するか</summary>
        public bool EnableChokobumiForCompaction { get; set; } = true;

        /// <summary>無起振時にちょこ踏みを考慮するか</summary>
        public bool EnableChokobumiForNonCompaction { get; set; } = true;

        /// <summary>起振時にくびれを考慮するか</summary>
        public bool EnableKubireForCompaction { get; set; } = true;

        /// <summary>無起振時にくびれを考慮するか</summary>
        public bool EnableKubireForNonCompaction { get; set; } = true;


        /// <summary>ローラの車種が両鉄輪ならTrue</summary>
        public bool IsTandemRoller { get; set; } = false;

        /// <summary>ローラのホイールベース</summary>
        public double WheelbaseMeter { get; set; } = 3.0;


        public static ActivityWorkTimeEstimatorOption BuildByRoller(RollerModel roller)
        {
            return new ActivityWorkTimeEstimatorOption()
            {
                MeanSwitchbackTime = roller.WorkTimeEstimationOptions.MeanSwitchbackTime,
                MeanChokobumiTime = roller.WorkTimeEstimationOptions.MeanChokobumiTime,
                MeanTravelTime = roller.WorkTimeEstimationOptions.MeanTravelTime,
                MeanKubireLengthMeter = roller.WorkTimeEstimationOptions.MeanKubireLengthMeter,
                EnableChokobumiForCompaction = roller.WorkTimeEstimationOptions.Compaction.EnableChokobumi,
                EnableChokobumiForNonCompaction = roller.WorkTimeEstimationOptions.NonCompaction.EnableChokobumi,
                EnableKubireForCompaction = roller.WorkTimeEstimationOptions.Compaction.EnableKubire,
                EnableKubireForNonCompaction = roller.WorkTimeEstimationOptions.NonCompaction.EnableKubire,
                IsTandemRoller = roller.MachineInfo.RollerWheelType == Avalonia.Backend.Enums.RollerWheelType.Tandem,
                WheelbaseMeter = roller.MachineInfo.Wheelbase,
            };
        }
    }
}
