using KWRP.Avalonia.Backend;
using KWRP.Avalonia.Backend.Enums;
using KWRP.Avalonia.Backend.Models.Roller;
using KWRP.Avalonia.Backend.Services;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWRP.Avalonia.Frontend.Models.Stores
{
    public class ParameterStore
    {
        private readonly MachineStore _machineStore;
        private readonly ILogService _logService;

        public void Reload()
        {
            var vr = _machineStore.CurrentRoller;

            LaneWidth = vr.MachineInfo.LaneWidth;
            FrontAllowance = vr.FrontAllowance;
            RearAllowance = vr.RearAllowance;
            SidePrevAllowance = vr.LeftRightAllowance;
            SideNextAllowance = vr.LeftRightAllowance;
            LapWidth = vr.Zone.LapWidth;
            LapLength = vr.Zone.WorkAreaLapLength;
            LaneChangeLength = vr.Zone.LaneChangeLength;
        }

        public ParameterStore(
            MachineStore machineStore,
            ILogService logService)
        {
            _machineStore = machineStore;
            _logService = logService;

            // initialize parameters
            {
                var vr = _machineStore.CurrentRoller;

                LaneWidth = vr.MachineInfo.LaneWidth;
                FrontAllowance = vr.FrontAllowance;
                RearAllowance = vr.RearAllowance;
                SidePrevAllowance = vr.LeftRightAllowance;
                SideNextAllowance = vr.LeftRightAllowance;
                PerimeterAllowance = vr.LeftRightAllowance;
                LapWidth = vr.Zone.LapWidth;
                LapLength = vr.Zone.WorkAreaLapLength;
                LaneChangeLength = vr.Zone.LaneChangeLength;

                RollerHeadType = RollerHeadingType.ToLeft;
                FrontOffset = 0.0;
                RearOffset = 0.0;
                SidePrevOffset = new(0.0);
                SideNextOffset = new(0.0);
                ProgresssDirectionRadian = new(0.0);

                _pairCountMin = KWRPConstants.C_DEFAULT_PAIRCOUNT_MIN;
                _pairCountMax = KWRPConstants.C_DEFAULT_PAIRCOUNT_MAX;
            }

            _logService.LogDebug("init");
        }

        /// <summary>
        /// レーン幅(m) : レーン割する際の基準幅
        /// </summary>
        public double LaneWidth { get; set; }

        /// <summary>
        /// 前方余裕代(m) : レーン前側と転圧領域境界まで離隔する距離
        /// </summary>
        public double FrontAllowance { get; private set; }
        public double FrontOffset { get; set; }

        /// <summary>
        /// 後方余裕代(m) : レーン後ろ側と転圧領域境界まで離隔する距離
        /// </summary>
        public double RearAllowance { get; private set; }
        public double RearOffset { get; set; }

        /// <summary>
        /// 側方余裕代(m) : 転圧領域からオフセットする距離
        /// </summary>
        public double SidePrevAllowance { get; private set; }
        public ReactiveProperty<double> SidePrevOffset { get; }

        public double SideNextAllowance { get; private set; }
        public ReactiveProperty<double> SideNextOffset { get; }

        public double PerimeterAllowance { get; }

        /// <summary>
        /// ラップ幅(m) : 隣り合うレーンをどれだけかぶらせるか
        /// </summary>
        public double LapWidth { get; set; }
        public double LapLength { get; set; }
        public double LapLengthActual => LapLength + (_machineStore.CurrentRoller.MachineInfo.RollerWheelType == RollerWheelType.Single ? 0 : _machineStore.CurrentRoller.MachineInfo.Wheelbase);

        /// <summary>
        /// 作業可能長さ(m) : レーン長さの下限値
        /// </summary>
        public double LaneChangeLength { get; set; }

        /// <summary>
        /// 作業進捗方向(deg) : 作業がこの方向に進むイメージ。東を0度としてcounterclockwise
        /// </summary>
        public ReactiveProperty<double> ProgresssDirectionRadian { get;  }

        ///// <summary>
        ///// 作業進捗方向を北方向としたときに、ローラの向きが右か
        ///// </summary>


        public RollerHeadingType RollerHeadType { get; set; }

        /// <summary>
        /// ローラの向いている方向。東を0度としてcounterclockwise
        /// </summary>
        public double RollerHeading => ProgresssDirectionRadian.Value + RollerHeadType.ToAngleRadian();

        /// <summary>
        /// レーンペアリング時のペア数最小値
        /// </summary>
        public int PairCountMin
        {
            get => _pairCountMin;
            set
            {
                _pairCountMin = Math.Clamp(value, 1, PairCountMax);
                _pairCountMax = Math.Max(_pairCountMax, _pairCountMin);
            }
        }
        private int _pairCountMin;

        /// <summary>
        /// レーンペアリング時のペア数最大値
        /// </summary>
        public int PairCountMax
        {
            get => _pairCountMax;
            set
            {
                _pairCountMax = Math.Max(_pairCountMin, value);
            }
        }
        private int _pairCountMax;



        private readonly Subject<Unit> _subject = new();
        public Observable<Unit> ParameterFileLoaded => _subject;

        public ReactiveProperty<bool> CanArrangeLane { get; } = new(true);
        public void PublishParameterFileLoaded() => _subject.OnNext(Unit.Default);



    }
}