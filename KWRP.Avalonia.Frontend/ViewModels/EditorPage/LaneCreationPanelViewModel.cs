using KWRP.Avalonia.Backend;
using KWRP.Avalonia.Backend.Enums;
using KWRP.Avalonia.Backend.Model.Shapes;
using KWRP.Avalonia.Backend.Models;
using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Frontend.Models.Stores;
using KWRP.Avalonia.Frontend.Services;
using KWRP.Avalonia.Frontend.ViewModels.StartUpPage;
using Microsoft.Extensions.DependencyInjection;
using ObservableCollections;
using R3;
using System;
using System.Linq;
using Trdk.Geometry;

namespace KWRP.Avalonia.Frontend.ViewModels.EditorPage
{
    public class LaneCreationPanelViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly MachineStore _machineStore;
        private readonly ParameterStore _parameterStore;
        private readonly ILogService _logService;
        private readonly CanvasItemStore _canvasItemStore;
        private readonly CanvasStateStore _canvasStateStore;
        private readonly ILaneArrangementService _laneArrangementService;
        private readonly INotificationService _notificationService;
        private readonly IServiceProvider _serviceProvider;

        public LaneCreationPanelViewModel(
            INavigationService navigationService,
            MachineStore machineStore,
            ParameterStore parameterStore,
            ILogService logService,
            CanvasItemStore canvasItemStore,
            CanvasStateStore canvasStateStore,
            INotificationService notificationService,
            ILaneArrangementService laneArrangementService,
            IServiceProvider serviceProvider)
        {
            _navigationService = navigationService;
            _machineStore = machineStore;
            _parameterStore = parameterStore;
            _logService = logService;
            _canvasItemStore = canvasItemStore;
            _canvasStateStore = canvasStateStore;
            _laneArrangementService = laneArrangementService;
            _notificationService = notificationService;
            _serviceProvider = serviceProvider;


            // initialize parameters
            {
                IsDirectionSelectionMode = _canvasStateStore
                    .IsDirectionSelectionMode
                    .ToReadOnlyBindableReactiveProperty(_canvasStateStore.IsDirectionSelectionMode.Value)
                    .AddTo(Disposables);
                IsOptimizing = _canvasStateStore
                    .IsOptimizing
                    .ToReadOnlyBindableReactiveProperty(_canvasStateStore.IsOptimizing.Value)
                    .AddTo(Disposables);

                LaneCreated = _canvasItemStore
                    .Lanes
                    .ObserveCountChanged()
                    .Select(cnt => cnt > 0)
                    .ToBindableReactiveProperty(_canvasItemStore.Lanes.Count > 0)
                    .AddTo(Disposables);
                TargetRegisterd = _canvasItemStore
                    .TargetPolygons
                    .ObserveCountChanged()
                    .Select(cnt => cnt > 0)
                    .ToBindableReactiveProperty(_canvasItemStore.TargetPolygons.Count > 0)
                    .AddTo(Disposables);
                CanArrangeLane = _parameterStore
                    .CanArrangeLane
                    .ToBindableReactiveProperty(_parameterStore.CanArrangeLane.Value)
                    .AddTo(Disposables); 

                LaneWidth = new BindableReactiveProperty<double>(_parameterStore.LaneWidth).AddTo(Disposables);
                FrontOffset = new BindableReactiveProperty<double>(_parameterStore.FrontOffset).AddTo(Disposables);
                RearOffset = new BindableReactiveProperty<double>(_parameterStore.RearOffset).AddTo(Disposables);
                SidePrevOffset = _parameterStore.SidePrevOffset.ToBindableReactiveProperty(_parameterStore.SidePrevOffset.Value).AddTo(Disposables);
                SideNextOffset = _parameterStore.SideNextOffset.ToBindableReactiveProperty(_parameterStore.SideNextOffset.Value).AddTo(Disposables);
                LapWidth = new BindableReactiveProperty<double>(_parameterStore.LapWidth).AddTo(Disposables);
                LaneChangeLength = new BindableReactiveProperty<double>(_parameterStore.LaneChangeLength).AddTo(Disposables);
                RollerHeadType = new BindableReactiveProperty<RollerHeadingType>(_parameterStore.RollerHeadType).AddTo(Disposables);
                ProgressDirectionDegree = _parameterStore
                    .ProgresssDirectionRadian
                    .Select(d => Utils.RoundDegree(d * 180.0 / Math.PI))
                    .ToBindableReactiveProperty()
                    .AddTo(Disposables);
                RollerHeadingDirectionDegree = ProgressDirectionDegree
                    .CombineLatest(RollerHeadType, (d, typ) => d + typ.ToAngleRadian() * 180.0 / Math.PI)
                    .Select(Utils.RoundDegree)
                    .ToReadOnlyBindableReactiveProperty()
                    .AddTo(Disposables);
                PairMinCount = new BindableReactiveProperty<int>(_parameterStore.PairCountMin).AddTo(Disposables);
                PairMaxCount = new BindableReactiveProperty<int>(_parameterStore.PairCountMax).AddTo(Disposables);

                _logService.LogDebug("init params");
            }

            // register commands
            {
                NavigateNextCommand = LaneCreated
                    .ToReactiveCommand(
                        execute: _ => _navigationService.NavigateTo<EditorLayoutViewModel, CanvasViewModel, WorkAreaEditorPanelViewModel>(),
                        initialCanExecute: _canvasItemStore.Lanes.Count > 0)
                    .AddTo(Disposables);

                LaneArrangementCommand = TargetRegisterd
                    .Zip(CanArrangeLane, (a, b) => a && b)
                    .ToReactiveCommand(_ =>
                    {
                        try
                        {
                            _logService.LogDebug("lane div");
                            _canvasItemStore.Lanes.Clear();
                            _canvasItemStore.CreatedLaneResults.Clear();
                            foreach (var target in _canvasItemStore.TargetPolygons)
                            {
                                _laneArrangementService.AllocateDirections(_parameterStore.ProgresssDirectionRadian.Value);
                                _laneArrangementService.ArrangeLanesAsync(target);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logService.LogError($"レーン割に失敗しました: {ex}");
                            _notificationService.Notify(KWRPNotification.Create(
                                message: $"レーン割に失敗しました: {ex.Message}",
                                type: NotifyMessageType.Warn,
                                duration: 5));
                        }
                    })
                    .AddTo(Disposables);

                ChangeHeadingCommand = TargetRegisterd
                    .ToReactiveCommand(_ =>
                    {
                        if (RollerHeadType.Value == RollerHeadingType.ToLeft)
                        {
                            RollerHeadType.Value = RollerHeadingType.ToRight;
                        }
                        else if (RollerHeadType.Value == RollerHeadingType.ToRight)
                        {
                            RollerHeadType.Value = RollerHeadingType.ToLeft;
                        }
                    })
                    .AddTo(Disposables);
            }

            // register events
            {
                /// パラメータの変更を検知すると、parameterStoreに値を保存し、レーン割実行コマンドを実行する
                /// 連続で変更があった場合は、一定時間後にまとめて実行する
                Observable.CombineLatest(
                    LaneWidth.Where(_ => CanArrangeLane.Value).Do(v => _parameterStore.LaneWidth = v).Select(v => Unit.Default),
                    FrontOffset.Where(_ => CanArrangeLane.Value).Do(v => _parameterStore.FrontOffset = v).Select(v => Unit.Default),
                    RearOffset.Where(_ => CanArrangeLane.Value).Do(v => _parameterStore.RearOffset = v).Select(v => Unit.Default),
                    SidePrevOffset.Where(_ => CanArrangeLane.Value).Do(v => _parameterStore.SidePrevOffset.Value = v).Select(v => Unit.Default),
                    SideNextOffset.Where(_ => CanArrangeLane.Value).Do(v => _parameterStore.SideNextOffset.Value = v).Select(v => Unit.Default),
                    LapWidth.Where(_ => CanArrangeLane.Value).Do(v => _parameterStore.LapWidth = v).Select(v => Unit.Default),
                    LaneChangeLength.Where(_ => CanArrangeLane.Value).Do(v => _parameterStore.LaneChangeLength = v).Select(v => Unit.Default),
                    RollerHeadType.Where(_ => CanArrangeLane.Value).Do(v => _parameterStore.RollerHeadType = v).Select(v => Unit.Default),
                    ProgressDirectionDegree.Where(_ => CanArrangeLane.Value).Do(v => _parameterStore.ProgresssDirectionRadian.Value = v * Math.PI / 180).Select(v => Unit.Default),
                    PairMinCount.Where(_ => CanArrangeLane.Value).Do(v => _parameterStore.PairCountMin = v).Select(v => Unit.Default),
                    PairMaxCount.Where(_ => CanArrangeLane.Value).Do(v => _parameterStore.PairCountMax = v).Select(v => Unit.Default))
                    .Skip(1)
                    .ThrottleLast(TimeSpan.FromMilliseconds(KWRPConstants.C_LANEARRANGE_THROTTLE_MSEC))
                    .Where(_ => LaneArrangementCommand.CanExecute())
                    .Subscribe(_ =>
                    {
                        LaneArrangementCommand.Execute(Unit.Default);
                    }).AddTo(Disposables);

                // ParameterFileLoaded.OnNextが発行されたら
                // UIのパラメータを更新し、レーン割計算を実行
                _parameterStore
                    .ParameterFileLoaded
                    .Where(_ => LaneArrangementCommand.CanExecute())
                    .Subscribe(_ =>
                    {
                        try
                        {
                            CanArrangeLane.Value = false;
                            // パラメータの更新
                            LaneWidth.Value = _parameterStore.LaneWidth;
                            FrontOffset.Value = _parameterStore.FrontOffset;
                            RearOffset.Value = _parameterStore.RearOffset;
                            SidePrevOffset.Value = _parameterStore.SidePrevOffset.Value;
                            LapWidth.Value = _parameterStore.LapWidth;
                            LaneChangeLength.Value = _parameterStore.LaneChangeLength;
                            RollerHeadType.Value = _parameterStore.RollerHeadType;
                            ProgressDirectionDegree.Value = _parameterStore.ProgresssDirectionRadian.Value * 180 / Math.PI;
                            PairMinCount.Value = _parameterStore.PairCountMin;
                            PairMaxCount.Value = _parameterStore.PairCountMax;
                        }
                        finally
                        {
                            CanArrangeLane.Value = true;
                        }
                        LaneArrangementCommand.Execute(Unit.Default);
                    }).AddTo(Disposables);

                _logService.LogDebug("init events");
            }


            _logService.SetStatusMessage("レーン割UI : 「レーン割実行」ボタンを押すか、振動ローラーパラメータを変更することで区割りが実行できます");
            _logService.LogDebug("init");
            
        }

        public override void Dispose()
        {
            _settingPage?.Dispose();
            base.Dispose();
        }

        public BindableReactiveProperty<bool> LaneCreated { get; }      // レーン生成済みか
        public BindableReactiveProperty<bool> TargetRegisterd { get; }  // レーン割対象のポリゴンがあるか
        public BindableReactiveProperty<bool> CanArrangeLane { get; }   // レーン割計算を実行して良いか
        public IReadOnlyBindableReactiveProperty<bool> IsDirectionSelectionMode { get; }    // UI方向選択モードか
        public IReadOnlyBindableReactiveProperty<bool> IsOptimizing { get; }

        public ReactiveCommand NavigateNextCommand { get; }
        public ReactiveCommand ChangeHeadingCommand { get; }
        public ReactiveCommand LaneArrangementCommand { get; }

        public BindableReactiveProperty<double> LaneWidth { get; }
        public BindableReactiveProperty<double> FrontOffset { get; }
        public BindableReactiveProperty<double> RearOffset { get; }
        public BindableReactiveProperty<double> SidePrevOffset { get; }
        public BindableReactiveProperty<double> SideNextOffset { get; }
        public BindableReactiveProperty<double> LapWidth { get; }
        public BindableReactiveProperty<double> LaneChangeLength { get; }
        public BindableReactiveProperty<double> ProgressDirectionDegree { get; }
        public IReadOnlyBindableReactiveProperty<double> RollerHeadingDirectionDegree { get; }
        public BindableReactiveProperty<RollerHeadingType> RollerHeadType { get; }

        public BindableReactiveProperty<int> PairMinCount { get; }
        public BindableReactiveProperty<int> PairMaxCount { get; }

        private ViewModelBase? _settingPage = null;
        public ViewModelBase SettingPage => _settingPage ??= _serviceProvider.GetRequiredService<SettingPageViewModel>();
    }
}