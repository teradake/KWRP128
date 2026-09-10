using KWRP.Avalonia.Backend.Constants;
using KWRP.Avalonia.Backend.Enums;
using KWRP.Avalonia.Backend.Models;
using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Frontend.Models.Stores;
using KWRP.Avalonia.Frontend.Services;
using KWRP.Avalonia.Frontend.ViewModels.EditorPage;
using KWRP.Backend.Enums;
using KWRP.Backend.Services;
using R3;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
        private readonly ILanguageService _languageService;

        public ReactiveCommand NextCommand { get; }
        //public ReactiveCommand<FileType> LoadCommand { get; }   // ダイアログを開いてファイルを選択
        public ReactiveCommand<FileType> LoadByMeterCommand { get; }
        public ReactiveCommand<FileType> LoadByMilliMeterCommand { get; }
        public ReactiveCommand<string> LoadFromPathCommand { get; } // デバッグ用。xamlから渡されたパスのファイルを選択する
        public ReactiveCommand ClearDxfCommand { get; }

        public string RollerName => _machineStore.CurrentRoller.MachineName;
        public IReadOnlyBindableReactiveProperty<string> FilePath { get; }
        public IReadOnlyBindableReactiveProperty<string> DxfTitleName { get; }
        public BindableReactiveProperty<bool> IsDxfExist { get; }
        
        // 日本語/英語のみ対応予定なので言語切り替えまわりはboolで管理する方針
        public BindableReactiveProperty<bool> IsCurrentLangJa { get; }
        public IReadOnlyBindableReactiveProperty<string> CurrentLangLabel { get; }
        public ReactiveCommand<bool> ToggleLanguageComamnd { get; }
        

        public TopPageViewModel(
            INavigationService navigationService,
            INotificationService notificationService,
            ILogService logService,
            IDataLoader dataLoader,
            ApplicationStore applicationStore,
            MachineStore machineStore,
            DxfStore dxfStore,
            ILanguageService languageService)
        {
            _navigationService = navigationService;
            _notificationService = notificationService;
            _logService = logService;
            _dataLoader = dataLoader;
            _applicationStore = applicationStore;
            _machineStore = machineStore;
            _dxfStore = dxfStore;
            _languageService = languageService;


            IsCurrentLangJa = new BindableReactiveProperty<bool>(_languageService.CurrentLanguage.Trim().ToLower() == "ja")
                .AddTo(Disposables);
            CurrentLangLabel = IsCurrentLangJa.Select(b => b ? "日本語" : "English").ToReadOnlyBindableReactiveProperty(_languageService.CurrentLanguage.Trim().ToLower())
                .AddTo(Disposables);
            ToggleLanguageComamnd = new ReactiveCommand<bool>(b =>
            {
                string lang = IsCurrentLangJa.Value ? "ja" : "en";
                _languageService.LoadLanguage(lang);
            }).AddTo(Disposables);

            Observable.FromEvent(
                h => _languageService.LanguageChanged += h,
                h => _languageService.LanguageChanged -= h)
                .Subscribe(_ =>
                {
                    IsCurrentLangJa.Value = _languageService.CurrentLanguage.Trim().ToLower() == "ja";
                })
                .AddTo(Disposables);

            FilePath = _applicationStore
                .SpatialDataPath
                .ToReadOnlyBindableReactiveProperty(_applicationStore.SpatialDataPath.Value)
                .AddTo(Disposables);

            IsDxfExist = _dxfStore
                .SelectedOption
                .Select(v => v != null)
                .ToBindableReactiveProperty(_dxfStore.SelectedOption.Value != null)
                .AddTo(Disposables);

            DxfTitleName = _dxfStore
                .SelectedOption
                .Select(v => v?.Title ?? _languageService.GetString("Domain.Front.Unregistered"))
                .ToReadOnlyBindableReactiveProperty(_dxfStore.SelectedOption.Value?.Title ?? _languageService.GetString("Domain.Front.Unregistered"))
                .AddTo(Disposables);

            ClearDxfCommand = IsDxfExist
                .ToReactiveCommand(_ => _dxfStore.SelectedOption.Value = null)
                .AddTo(Disposables);

            NextCommand = _applicationStore
                .IsBusy
                .Select(b => !b)
                .ToReactiveCommand(_ =>
                {
                    _navigationService.NavigateTo<EditorLayoutViewModel, CanvasViewModel, LaneCreationPanelViewModel>();
                })
                .AddTo(Disposables);

            //LoadCommand = _applicationStore
            //    .IsBusy
            //    .Select(b => !b)
            //    .ToReactiveCommand<FileType>(async (type, ct) =>
            //    {
            //        try
            //        {
            //            _applicationStore.IsBusy.Value = true;
            //            await _dataLoader.ExecuteLoadFromDialogAsync(type);
            //        }
            //        catch (Exception ex)
            //        {
            //            _logService.LogError($"error", ex);
            //            _notificationService.Notify(KWRPNotification.Create(ex.Message, NotifyMessageType.Warn, 6));
            //        }
            //        finally
            //        {
            //            _applicationStore.IsBusy.Value = false;
            //        }
            //    })
            //    .AddTo(Disposables);

            LoadByMeterCommand = CreateLoadCommand(LengthUnitType.Meter);
            LoadByMilliMeterCommand = CreateLoadCommand(LengthUnitType.Millimeter);

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

            _logService.SetStatusMessage("Domain.Front.TopPageDescription");
            _logService.LogDebug("init");
        }

        private ReactiveCommand<FileType> CreateLoadCommand(LengthUnitType unitType)
        {
            return _applicationStore
                .IsBusy
                .Select(b => !b)
                .ToReactiveCommand<FileType>((type, ct) => LoadAsync(type, unitType, ct))
                .AddTo(Disposables);
        }

        private async ValueTask LoadAsync(FileType type, LengthUnitType unitType, CancellationToken ct)
        {
            try
            {
                _applicationStore.IsBusy.Value = true;
                await _dataLoader.ExecuteLoadFromDialogAsync(type, unitType);
            }
            catch (Exception ex)
            {
                _logService.LogError("error", ex);
                _notificationService.Notify(
                    KWRPNotification.Create(ex.Message, NotifyMessageType.Warn, 6));
            }
            finally
            {
                _applicationStore.IsBusy.Value = false;
            }
        }
    }
}
