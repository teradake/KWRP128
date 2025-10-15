using Avalonia.Notification;
using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Frontend.Models;
using KWRP.Avalonia.Frontend.Models.Stores;
using KWRP.Avalonia.Frontend.Services.Dxf;
using R3;
using System;
using System.IO;
using System.Threading.Tasks;

namespace KWRP.Avalonia.Frontend.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        private readonly NotificationStore _notificationStore;
        private readonly ApplicationStore _applicationStore;
        private readonly LogStore _logStore;
        private readonly IKWRPApplicationService _appService;
        private readonly DxfHistoryStorageService _dxfHistoryStorageService;


        public MainWindowViewModel(
            NavigationStore navigationStore,
            NotificationStore notificationStore,
            LogStore logStore,
            ApplicationStore applicationStore,
            IKWRPApplicationService appService,
            DxfHistoryStorageService dxfHistoryStorageService)
        {
            _navigationStore = navigationStore;
            _notificationStore = notificationStore;
            _logStore = logStore;
            _applicationStore = applicationStore;
            _appService = appService;
            _dxfHistoryStorageService = dxfHistoryStorageService;
            //_appService = appService;

            CurrentViewModel = _navigationStore
                .ViewModelObservable
                .ToReadOnlyBindableReactiveProperty(_navigationStore.CurrentViewModel)
                .AddTo(Disposables);

            StatusMessage = _logStore
                .StatusMessageObservable
                .ToReadOnlyBindableReactiveProperty("")
                .AddTo(Disposables);

            IsBusy = _applicationStore
                .IsBusy
                .ToReadOnlyBindableReactiveProperty(_applicationStore.IsBusy.Value)
                .AddTo(Disposables);

            AreaFilePath = _applicationStore
                .SpatialDataPath
                .Select(path => Path.GetFileName(path))
                .ToReadOnlyBindableReactiveProperty(Path.GetFileName(_applicationStore.SpatialDataPath.Value))
                .AddTo(Disposables);
        }

        public string AppName => "区割りシステム";
        public string Version => "1.2.4";
        public string Title => $"{AppName} ver {Version}";

        public INotificationMessageManager Manager => _notificationStore.Manager;
        public IReadOnlyBindableReactiveProperty<ViewModelBase?> CurrentViewModel { get; }
        public IReadOnlyBindableReactiveProperty<string> StatusMessage { get; }
        public IReadOnlyBindableReactiveProperty<string> AreaFilePath { get; }

        public IReadOnlyBindableReactiveProperty<bool> IsBusy { get; }

        public async Task LoadDxfHistoryAsync() => await _dxfHistoryStorageService.LoadAsync();
        public async Task SaveDxfHistoryAsync() => await _dxfHistoryStorageService.SaveAsync();
        public async Task SaveLaneArrangementConfigAsync() => await _appService.SaveLaneArrangementConfigAsync();
        public async Task SaveMachineConfigAsync() => await _appService.SaveMachineConfigAsync();
    }
}
