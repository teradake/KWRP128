using KWRP.Avalonia.Backend.Models;
using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Frontend.Models.Stores;
using KWRP.Avalonia.Frontend.Services;
using KWRP.Avalonia.Frontend.ViewModels.StartUpPage;
using ObservableCollections;
using R3;
using System;
using System.Linq;

namespace KWRP.Avalonia.Frontend.ViewModels.EditorPage
{
    public class EditorLayoutViewModel : ViewModelBase
    {
        private readonly NavigationTwoPanelStore _navigationTwoPanelStore;
        private readonly INavigationService _navigationService;
        private readonly ILogService _logService;
        private readonly ILaneArrangementParameterService _laneArrangementParameterService;
        private readonly CanvasItemStore _canvasItemStore;
        private readonly IPathService _pathService;
        private readonly ApplicationStore _applicationStore;
        private readonly INotificationService _notificationService;

        public EditorLayoutViewModel(
            NavigationTwoPanelStore navigationTwoPanelStore,
            INavigationService navigationService,
            ILogService logService,
            CanvasItemStore canvasItemStore,
            ILaneArrangementParameterService laneArrangementParameterService,
            IPathService pathService,
            ApplicationStore applicationStore,
            INotificationService notificationService)
        {
            _navigationTwoPanelStore = navigationTwoPanelStore;
            _navigationService = navigationService;
            _logService = logService;
            _notificationService = notificationService;
            _canvasItemStore = canvasItemStore;
            _pathService = pathService;
            _applicationStore = applicationStore;
            _laneArrangementParameterService = laneArrangementParameterService;

            LeftViewModel = _navigationTwoPanelStore
                .ObservableLeftViewModel
                .ToReadOnlyBindableReactiveProperty()
                .AddTo(Disposables);
            RightViewModel = _navigationTwoPanelStore
                .ObservableRightViewModel
                .ToBindableReactiveProperty()
                .AddTo(Disposables);

            IsLaneEditor = RightViewModel
                .Select(vm => vm != null && vm.GetType() == typeof(LaneCreationPanelViewModel))
                .ToBindableReactiveProperty()
                .AddTo(Disposables);
            IsWorkAreaEditor = RightViewModel
                .Select(vm => vm != null && vm.GetType() == typeof(WorkAreaEditorPanelViewModel))
                .ToBindableReactiveProperty()
                .AddTo(Disposables);
            IsActivityEditor = RightViewModel
                .Select(vm => vm != null && vm.GetType() == typeof(ActivityEditorViewModel))
                .ToBindableReactiveProperty()
                .AddTo(Disposables);
            TargetRegistered = _canvasItemStore
                    .TargetPolygons
                    .ObserveCountChanged()
                    .Select(cnt => cnt > 0)
                    .ToBindableReactiveProperty(_canvasItemStore.TargetPolygons.Count > 0)
                    .AddTo(Disposables);

            LaneArrangementCommand = IsLaneEditor.CombineLatest(TargetRegistered, (a, b) => a && b)
                .ToReactiveCommand(_ =>
                {
                    if (RightViewModel.Value is LaneCreationPanelViewModel vm)
                    {
                        vm.LaneArrangementCommand.Execute(_);
                    }
                })
                .AddTo(Disposables);

            LoadLaneArrangementParamCommand = IsLaneEditor
                .ToReactiveCommand(async _ =>
                {
                    try
                    {
                        _applicationStore.IsBusy.Value = true;
                        _logService.LogDebug("load params");
                        if (await _laneArrangementParameterService.LoadParamsAsync())
                        {
                            _logService.LogInfo($"パラメータを読込みました");
                            _notificationService.Notify(KWRPNotification.Create("パラメータを読込みました", Backend.Enums.NotifyMessageType.Info, 3));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logService.LogWarn($"パラメータ読み込みに失敗しました: {ex}");
                        _notificationService.Notify(KWRPNotification.Create("パラメータを読込みに失敗しました" + ex.Message, Backend.Enums.NotifyMessageType.Warn, 5));
                    }
                    finally
                    {
                        _applicationStore.IsBusy.Value = false;
                    }
                })
                .AddTo(Disposables);

            SaveLaneArrangementParamCommand = IsLaneEditor
                .ToReactiveCommand(async _ =>
                {
                    try
                    {
                        _applicationStore.IsBusy.Value = true;
                        _logService.LogDebug("save param");
                        var path = await _laneArrangementParameterService.SaveParamsAsync();

                        if (path == null)
                            return;

                        _notificationService.Notify(KWRPNotification.Create("パラメータを保存しました", Backend.Enums.NotifyMessageType.Info, 5)
                            .WithCommand("フォルダを開く", () =>
                            {
                                if (_pathService.IsPathValid(path))
                                {
                                    _pathService.SelectFileInExplorer(path);
                                }
                            }));
                    }
                    catch (Exception ex)
                    {
                        _logService.LogWarn($"パラメータ保存に失敗しました: {ex}");
                        _notificationService.Notify(KWRPNotification.Create("パラメータの保存に失敗しました\n" + ex.Message, Backend.Enums.NotifyMessageType.Warn, 5));
                    }
                    finally
                    {
                        _applicationStore.IsBusy.Value = false;
                    }
                })
                .AddTo(Disposables);

            NavigateTopCommand = new ReactiveCommand(_ => _navigationService.NavigateTo<StartUpViewModel>());


            _navigationTwoPanelStore
                .ObservableRightViewModel
                .Subscribe(mode =>
                {
                    if (mode is LaneCreationPanelViewModel) _applicationStore.EditorMode.Value = Backend.Enums.EditorMode.LaneDesign;
                    else if (mode is WorkAreaEditorPanelViewModel) _applicationStore.EditorMode.Value = Backend.Enums.EditorMode.WorkAreaDesign;
                    else if (mode is ActivityEditorViewModel) _applicationStore.EditorMode.Value = Backend.Enums.EditorMode.ActivityDesign;
                    else _applicationStore.EditorMode.Value = Backend.Enums.EditorMode.None;
                })
                .AddTo(Disposables);
            //if (RightViewModel.Value != null && RightViewModel.Value is LaneCreationPanelViewModel) _applicationStore.EditorMode.Value = Backend.Enums.EditorMode.LaneDesign;
            //else if (RightViewModel.Value != null && RightViewModel.Value is WorkAreaEditorPanelViewModel) _applicationStore.EditorMode.Value = Backend.Enums.EditorMode.WorkAreaDesign;
            //else if (RightViewModel.Value != null && RightViewModel.Value is ActivityEditorViewModel) _applicationStore.EditorMode.Value = Backend.Enums.EditorMode.ActivityDesign;
            //else _applicationStore.EditorMode.Value = Backend.Enums.EditorMode.None;
        }

        public ReactiveCommand NavigateTopCommand { get; }
        public ReactiveCommand LaneArrangementCommand { get; }
        public ReactiveCommand LoadLaneArrangementParamCommand { get; }
        public ReactiveCommand SaveLaneArrangementParamCommand { get; }

        public IReadOnlyBindableReactiveProperty<ViewModelBase?> LeftViewModel { get; }
        public BindableReactiveProperty<ViewModelBase?> RightViewModel { get; }

        public BindableReactiveProperty<bool> TargetRegistered { get; }     // 転圧領域ポリゴンが読込み済みか

        public BindableReactiveProperty<bool> IsLaneEditor { get; }
        public BindableReactiveProperty<bool> IsWorkAreaEditor { get; }
        public BindableReactiveProperty<bool> IsActivityEditor { get; }




        public override void Dispose()
        {
            _navigationTwoPanelStore.SetRightViewModel(null);
            base.Dispose();
        }
    }
}
