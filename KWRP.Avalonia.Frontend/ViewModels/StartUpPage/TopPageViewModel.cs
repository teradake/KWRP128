using KWRP.Avalonia.Backend.Enums;
using KWRP.Avalonia.Backend.Models;
using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Frontend.Models.Stores;
using KWRP.Avalonia.Frontend.Services;
using KWRP.Avalonia.Frontend.ViewModels.EditorPage;
using R3;
using System;
using System.Linq;

namespace KWRP.Avalonia.Frontend.ViewModels.StartUpPage
{
    public class TopPageViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly INotificationService _notificationService;
        private readonly ILogService _logService;
        private readonly ApplicationStore _applicationStore;
        private readonly IDataLoader _dataLoader;
        private readonly MachineStore _machineStore;
        private readonly DxfStore _dxfStore;

        public ReactiveCommand NextCommand { get; }
        public ReactiveCommand<FileType> LoadCommand { get; }   // ダイアログを開いてファイルを選択
        public ReactiveCommand<string> LoadFromPathCommand { get; } // デバッグ用。xamlから渡されたパスのファイルを選択する

        public IReadOnlyBindableReactiveProperty<string> FilePath { get; }
        public string RollerName => _machineStore.CurrentRoller.MachineName;
        //public string DxfTitleName => _dxfStore.SelectedOption.Value?.Title ?? "未登録";
        public IReadOnlyBindableReactiveProperty<string> DxfTitleName { get; }

        public TopPageViewModel(
            INavigationService navigationService,
            INotificationService notificationService,
            ILogService logService,
            IDataLoader dataLoader,
            ApplicationStore applicationStore,
            MachineStore machineStore,
            DxfStore dxfStore)
        {
            _navigationService = navigationService;
            _notificationService = notificationService;
            _logService = logService;
            _dataLoader = dataLoader;
            _applicationStore = applicationStore;
            _machineStore = machineStore;
            _dxfStore = dxfStore;

            FilePath = _applicationStore
                .SpatialDataPath
                .ToReadOnlyBindableReactiveProperty(_applicationStore.SpatialDataPath.Value)
                .AddTo(Disposables);

            DxfTitleName = _dxfStore
                .SelectedOption
                .Select(v => v?.Title ?? "未登録")
                .ToReadOnlyBindableReactiveProperty(_dxfStore.SelectedOption.Value?.Title ?? "未登録")
                .AddTo(Disposables);

            NextCommand = _applicationStore
                .IsBusy
                .Select(b => !b)
                .ToReactiveCommand(_ =>
                {
                    _navigationService.NavigateTo<EditorLayoutViewModel, CanvasViewModel, LaneCreationPanelViewModel>();
                })
                .AddTo(Disposables);

            LoadCommand = _applicationStore
                .IsBusy
                .Select(b => !b)
                .ToReactiveCommand<FileType>(async (type, ct) =>
                {
                    try
                    {
                        _applicationStore.IsBusy.Value = true;
                        await _dataLoader.ExecuteLoadFromDialogAsync(type);
                    }
                    catch (Exception ex)
                    {
                        _logService.LogError($"error", ex);
                        _notificationService.Notify(KWRPNotification.Create(ex.Message, NotifyMessageType.Warn, 6));
                    }
                    finally
                    {
                        _applicationStore.IsBusy.Value = false;
                    }
                })
                .AddTo(Disposables);

            LoadFromPathCommand = _applicationStore
                .IsBusy
                .Select(b => !b)
                .ToReactiveCommand<string>(async (path, ct) =>
                {
                    try
                    {
                        _applicationStore.IsBusy.Value = true;
                        await _dataLoader.ExecuteLoadFromFileAsync(path);
                    }
                    catch (Exception ex)
                    {
                        _logService.LogError($"error", ex);
                        _notificationService.Notify(KWRPNotification.Create(ex.Message, NotifyMessageType.Warn, 6));
                    }
                    finally
                    {
                        _applicationStore.IsBusy.Value = false;
                    }
                })
                .AddTo(Disposables);

            _logService.SetStatusMessage("TopPage : 転圧領域xmlファイルを読込んでNextボタンを押すことで区割りを開始します");
            _logService.LogDebug("init");
        }
    }
}
