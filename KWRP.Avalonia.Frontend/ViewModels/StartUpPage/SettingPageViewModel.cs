using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Frontend.Models.Settings;
using KWRP.Avalonia.Frontend.Models.Stores;

namespace KWRP.Avalonia.Frontend.ViewModels.StartUpPage
{
    public class SettingPageViewModel : ViewModelBase
    {
        private readonly ILogService _logService;
        //private readonly LaneArrangementConfigs _laneArrangementConfigs;
        private readonly ApplicationStore _applicationStore;
        private LaneArrangementConfigs _laneArrangementConfigs;

        public SettingPageViewModel(
            ILogService logService, ApplicationStore applicationStore)
        {
            _logService = logService;
            _applicationStore = applicationStore;
            _laneArrangementConfigs = _applicationStore.LaneArrangementConfigs;

            _logService.LogDebug("init");
        }

        public bool EnableLapAdjustment
        {
            get => _laneArrangementConfigs.EnableLapAdjustment;
            set => _laneArrangementConfigs.EnableLapAdjustment = value;
        }

        public double LaneGapToleranceFront
        {
            get => _laneArrangementConfigs.LaneGapToleranceFront;
            set => _laneArrangementConfigs.LaneGapToleranceFront = value;
        }

        public double LaneGapToleranceRear
        {
            get => _laneArrangementConfigs.LaneGapToleranceRear;
            set => _laneArrangementConfigs.LaneGapToleranceRear = value;
        }

        public bool EnableOptimize
        {
            get => _laneArrangementConfigs.EnableOptimize;
            set => _laneArrangementConfigs.EnableOptimize = value;
        }
    }
}
