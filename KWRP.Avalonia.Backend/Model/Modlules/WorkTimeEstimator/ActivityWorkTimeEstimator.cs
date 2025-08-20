namespace KWRP.Backend.Model.Modlules.WorkTimeEstimator
{
    public class ActivityWorkTimeEstimator
    {
        private readonly ActivityWorkTimeEstimatorOption _option;


        public ActivityWorkTimeEstimator(ActivityWorkTimeEstimatorOption option)
        {
            _option = option;
        }

        public TimeSpan GetMoveWorkTime()
        {
            return _option.MeanTravelTime;
        }

        public TimeSpan GetNonCompactionWorkTime(
            IEnumerable<double> laneLengthListMeter,
            int repeatNum,
            double refSpeedKmPerHour,
            bool[] edgeFlags)
            => Calculate(laneLengthListMeter, repeatNum, refSpeedKmPerHour, edgeFlags, _option.EnableChokobumiForNonCompaction, _option.EnableKubireForNonCompaction);


        public TimeSpan GetCompactionWorkTime(
            IEnumerable<double> laneLengthListMeter,
            int repeatNum,
            double refSpeedKmPerHour,
            bool[] edgeFlags)
            => Calculate(laneLengthListMeter, repeatNum, refSpeedKmPerHour, edgeFlags, _option.EnableChokobumiForCompaction, _option.EnableKubireForCompaction);


        private TimeSpan Calculate(
            IEnumerable<double> laneLengthListMeter, 
            int repeatNum, 
            double refSpeedKmPerHour, 
            bool[] edgeFlags,
            bool enableChokobumi,
            bool enableKubire)
        {
            var distances = laneLengthListMeter
                .Select(d => d - (_option.IsTandemRoller ? _option.WheelbaseMeter : 0))
                .Where(d => d > 0)
                .ToArray();

            var speed = refSpeedKmPerHour * 1000.0 / 3600.0; // m/sec

            var estimatedWorkTime = TimeSpan.Zero;
            {
                // 転圧時間の計算
                foreach (var laneLength in distances)
                {
                    var move = TimeSpan.FromSeconds(laneLength / speed);
                    var switchback = _option.MeanSwitchbackTime * 2;
                    estimatedWorkTime += (move + switchback) * repeatNum;
                }

                // 端部のちょこ踏み時間の計算
                foreach (var edgeFlag in edgeFlags)
                {
                    if (!enableChokobumi) break;
                    if (!edgeFlag) continue;
                    estimatedWorkTime += _option.MeanChokobumiTime * (repeatNum * distances.Length);
                }

                // くびれの時間の計算
                if (enableKubire && _option.MeanKubireLengthMeter > 0)
                {
                    var move = TimeSpan.FromSeconds(_option.MeanKubireLengthMeter / speed) * 2;
                    var switchback = _option.MeanSwitchbackTime * 2;
                    estimatedWorkTime += (move + switchback);
                }
            }
            return estimatedWorkTime;
        }
    }
}
