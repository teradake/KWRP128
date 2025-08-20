using System.Diagnostics;

namespace KWRP.Backend.Model.Modlules.WorkTimeEstimator
{
     [DebuggerDisplay("RefSpeed={RefSpeedKmPerHour} km/h, Travel={MeanTravelTime.TotalSeconds} s")]
    internal sealed class ActivityWorkTimeEstimatorOption
    {
        /// <summary>折返し時間 [sec]</summary>
        public TimeSpan MeanSwitchbackTime { get; set; }

        /// <summary>移動時間 [sec]</summary>
        public TimeSpan MeanTravelTime { get; set; }

        /// <summary>ちょこ踏み時間 [sec]</summary>
        public TimeSpan MeanChokobumiTime { get; set; }

        /// <summary>くびれ距離 [m]</summary>
        public double MeanKubireLengthMeter { get; set; }

        /// <summary>起振時にちょこ踏みを考慮するか</summary>
        public bool EnableChokobumiForCompaction { get; set; }

        /// <summary>無起振時にちょこ踏みを考慮するか</summary>
        public bool EnableChokobumiForNonCompaction { get; set; }

        /// <summary>起振時にくびれを考慮するか</summary>
        public bool EnableKubireForCompaction { get; set; }

        /// <summary>無起振時にくびれを考慮するか</summary>
        public bool EnableKubireForNonCompaction { get; set; }

        /// <summary>走行距離の一覧 [m]</summary>
        public double[] LaneLengthListMeter { get; set; } = [];

        /// <summary>転圧回数</summary>
        public int RepeatNum { get; set; }

        /// <summary>参照速度 [km/h]</summary>
        public double RefSpeedKmPerHour
        {
            get => _refSpeed;
            set => _refSpeed = value;
        }
        public double RedSpeedMeterPerSec => _refSpeed * 1000.0 / 3600.0; // km/h to m/s
        private double _refSpeed;

        /// <summary>端部フラグ</summary>
        public bool[] EdgeFlag { get; set; } = [false, false];
    }
}
