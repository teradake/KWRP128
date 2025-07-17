using KWRP.Avalonia.Backend.Enums;
using KWRP.Avalonia.Backend.Models.Roller;
using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Frontend.Models.Stores;
using R3;
using System.Collections.Generic;

namespace KWRP.Avalonia.Frontend.ViewModels.StartUpPage
{
    public class MachinePageViewModel : ViewModelBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogService _logService;
        private readonly MachineStore _machineStore;
        private readonly ParameterStore _parameterStore;

        private bool updated = false;
        private readonly List<RollerWheelType> _wheelTypes;
        public List<RollerWheelType> WheelTypes => _wheelTypes;


        public MachinePageViewModel(
            INotificationService notificationService,
            ILogService logService,
            MachineStore machineStore,
            ParameterStore parameterStore)
        {
            _notificationService = notificationService;
            _logService = logService;
            _machineStore = machineStore;
            _parameterStore = parameterStore;

            _wheelTypes = [RollerWheelType.Single, RollerWheelType.Tandem];

            _logService.SetStatusMessage("重機情報 : 振動ローラーの寸法や自動化作業領域等に関するパラメータが参照できます");
            _logService.LogDebug("init");

            this.PropertyChanged += OnMachinePageViewModelPropertyChanged;
        }

        private void OnMachinePageViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            updated = true;
        }

        public override void Dispose()
        {
            this.PropertyChanged -= OnMachinePageViewModelPropertyChanged;
            if (updated)
            {
                _logService.LogInfo("重機情報を変更しました");
                _parameterStore.Reload();
            }
            base.Dispose();
        }


        public virtual RollerModel VR => _machineStore.CurrentRoller;
        public string MachineName => VR.MachineName;
        public string Description => VR.Description;
        public virtual string FilePath => _machineStore.MachineConfigPath;

        public bool IsTandem => VR.MachineInfo.RollerWheelType == RollerWheelType.Tandem;
        public string WheelType => IsTandem ? "両鉄輪" : "片鉄輪";

        public RollerWheelType Type
        {
            get => VR.MachineInfo.RollerWheelType;
            set
            {
                VR.MachineInfo.RollerWheelType = value;
                OnPropertyChanged(nameof(Type));
                OnPropertyChanged(nameof(WheelType));
                OnPropertyChanged(nameof(RearAllowance));
                OnPropertyChanged(nameof(RearAllowanceDescription));
            }
        }

        //public double LaneWidth => VR.MachineInfo.LaneWidth;
        //public double FrontOverhang => VR.MachineInfo.FrontOverhang;
        //public double RearOverhang => VR.MachineInfo.RearOverhang;
        //public double Wheelbase => VR.MachineInfo.Wheelbase;
        public double LaneWidth
        {
            get => VR.MachineInfo.LaneWidth;
            set
            {
                VR.MachineInfo.LaneWidth = value;
                OnPropertyChanged(nameof(LaneWidth));
            }
        }

        public double FrontOverhang
        {
            get => VR.MachineInfo.FrontOverhang;
            set
            {
                VR.MachineInfo.FrontOverhang = value;
                OnPropertyChanged(nameof(FrontOverhang));
                OnPropertyChanged(nameof(FrontAllowance));
            }
        }
        public double RearOverhang
        {
            get => VR.MachineInfo.RearOverhang;
            set
            {
                VR.MachineInfo.RearOverhang = value;
                OnPropertyChanged(nameof(RearOverhang));
                OnPropertyChanged(nameof(RearAllowance));
            }
        }
        public double Wheelbase
        {
            get => VR.MachineInfo.Wheelbase;
            set
            {
                VR.MachineInfo.Wheelbase = value;
                OnPropertyChanged(nameof(Wheelbase));
                OnPropertyChanged(nameof(RearAllowance));
            }
        }

        //public double FrontRearMargin => VR.Zone.FrontRearMargin;
        //public double LeftRightMargin => VR.Zone.LeftRightMargin;
        //public double LapWidth => VR.Zone.LapWidth;
        //public double LaneChangeLength => VR.Zone.LaneChangeLength;
        //public double LandeChangeLength => VR.Zone.LaneChangeLength;
        //public double FrontEdgeOffset => VR.Zone.FrontEdgeOffset;
        //public double RearEdgeOffset => VR.Zone.BackEdgeOffset;
        //public double WorkAreaLapLength => VR.Zone.WorkAreaLapLength;
        public double FrontRearMargin
        {
            get => VR.Zone.FrontRearMargin;
            set
            {
                VR.Zone.FrontRearMargin = value;
                OnPropertyChanged(nameof(FrontRearMargin));
                OnPropertyChanged(nameof(FrontAllowance));
                OnPropertyChanged(nameof(RearAllowance));
            }
        }
        public double LeftRightMargin
        {
            get => VR.Zone.LeftRightMargin;
            set
            {
                VR.Zone.LeftRightMargin = value;
                OnPropertyChanged(nameof(LeftRightMargin));
                OnPropertyChanged(nameof(LeftRightAllowance));
            }
        }
        public double LapWidth
        {
            get => VR.Zone.LapWidth;
            set
            {
                VR.Zone.LapWidth = value;
                OnPropertyChanged(nameof(LapWidth));
            }
        }
        public double LaneChangeLength
        {
            get => VR.Zone.LaneChangeLength;
            set
            {
                VR.Zone.LaneChangeLength = value;
                OnPropertyChanged(nameof(LaneChangeLength));
            }
        }
        public double FrontEdgeOffset
        {
            get => VR.Zone.FrontEdgeOffset;
            set
            {
                VR.Zone.FrontEdgeOffset = value;
                OnPropertyChanged(nameof(FrontEdgeOffset));
            }
        }
        public double RearEdgeOffset
        {
            get => VR.Zone.BackEdgeOffset;
            set
            {
                VR.Zone.BackEdgeOffset = value;
                OnPropertyChanged(nameof(RearEdgeOffset));
            }
        }
        public double WorkAreaLapLength
        {
            get => VR.Zone.WorkAreaLapLength;
            set
            {
                VR.Zone.WorkAreaLapLength = value;
                OnPropertyChanged(nameof(WorkAreaLapLength));
            }
        }


        // 初期移動アクティビティの拡幅幅
        //public double InitialMoveMarginFront => VR.Zone.InitialMoveMarginFront;
        //public double InitialMoveMarginRear => VR.Zone.InitialMoveMarginBack;
        //public double InitialMoveMarginNext => VR.Zone.InitialMoveMarginNext;
        //public double InitialMoveMarginPrev => VR.Zone.InitialMoveMarginPrev;
        public double InitialMoveMarginFront
        {
            get => VR.Zone.InitialMoveMarginFront;
            set
            {
                VR.Zone.InitialMoveMarginFront = value;
                OnPropertyChanged(nameof(InitialMoveMarginFront));
            }
        }
        public double InitialMoveMarginRear
        {
            get => VR.Zone.InitialMoveMarginBack;
            set
            {
                VR.Zone.InitialMoveMarginBack = value;
                OnPropertyChanged(nameof(InitialMoveMarginRear));
            }
        }
        public double InitialMoveMarginNext
        {
            get => VR.Zone.InitialMoveMarginNext;
            set
            {
                VR.Zone.InitialMoveMarginNext = value;
                OnPropertyChanged(nameof(InitialMoveMarginNext));
            }
        }
        public double InitialMoveMarginPrev
        {
            get => VR.Zone.InitialMoveMarginPrev;
            set
            {
                VR.Zone.InitialMoveMarginPrev = value;
                OnPropertyChanged(nameof(InitialMoveMarginPrev));
            }
        }

        // 占有エリア(readonly)
        public double FrontAllowance => VR.FrontAllowance;
        public double RearAllowance => VR.RearAllowance;
        public double LeftRightAllowance => VR.LeftRightAllowance;

        public string FrontAllowanceDescription => " = 前後マージン + フロントオーバーハング";
        public string RearAllowanceDescription => " = 前後マージン + リアオーバーハング" +
            (IsTandem ? "" : " + ホイールベース");
        public string LeftRightAllowanceDescription => " = 左右マージン";
    }
}
