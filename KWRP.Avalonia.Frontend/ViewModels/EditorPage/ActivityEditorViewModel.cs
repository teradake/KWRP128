using KWRP.Avalonia.Backend;
using KWRP.Avalonia.Backend.Enums;
using KWRP.Avalonia.Backend.Models;
using KWRP.Avalonia.Backend.Models.Roller;
using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Frontend.Models.Stores;
using KWRP.Avalonia.Frontend.Services;
using ObservableCollections;
using R3;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KWRP.Avalonia.Frontend.ViewModels.EditorPage
{
    public class ActivityEditorViewModel : ViewModelBase
    {
        private readonly ILogService _logService;
        private readonly INavigationService _navigationService;
        private readonly INotificationService _notificationService;
        private readonly ActivityStore _activityStore;
        private readonly MachineStore _machineStore;
        private readonly IActivityService _activityService;
        private readonly IPathService _pathService;
        private readonly ApplicationStore _applicationStore;

        public RollerModel Roller => _machineStore.CurrentRoller;


        public ActivityEditorViewModel(
            ILogService logService,
            INavigationService navigationService,
            ActivityStore activityStore,
            INotificationService notificationService,
            MachineStore machineStore,
            IActivityService activityService,
            IPathService pathService,
            ApplicationStore applicationStore)
        {
            _logService = logService;
            _navigationService = navigationService;
            _activityStore = activityStore;
            _activityService = activityService;
            _notificationService = notificationService;
            _pathService = pathService;
            _machineStore = machineStore;
            _applicationStore = applicationStore;

            // init property
            {
                EnableMove = new BindableReactiveProperty<bool>(_activityStore.EnableMove).AddTo(Disposables);
                EnableNonCompaction = new BindableReactiveProperty<bool>(_activityStore.EnableNonCompaction).AddTo(Disposables);
                EnableCompaction = new BindableReactiveProperty<bool>(_activityStore.EnableCompaction).AddTo(Disposables);
                CanCreateActivity = EnableMove.CombineLatest(EnableCompaction, EnableNonCompaction,
                    (move, compact, nonCompact) => move || compact || nonCompact)
                    .ToBindableReactiveProperty(_activityStore.CanCreateActivity)
                    .AddTo(Disposables);

                InitialMoveMarginRear = new BindableReactiveProperty<double>(Roller.Zone.InitialMoveMarginBack).AddTo(Disposables);
                InitialMoveMarginFront = new BindableReactiveProperty<double>(Roller.Zone.InitialMoveMarginFront).AddTo(Disposables);
                InitialMoveMarginNext = new BindableReactiveProperty<double>(Roller.Zone.InitialMoveMarginNext).AddTo(Disposables);
                InitialMoveMarginPrev = new BindableReactiveProperty<double>(Roller.Zone.InitialMoveMarginPrev).AddTo(Disposables);

                RepeatNumNonCompaction = new BindableReactiveProperty<int>(Roller.NonCompactionParameter.RepeatNum)
                    .EnableValidation<ActivityEditorViewModel>(nameof(RepeatNumNonCompaction))
                    .AddTo(Disposables);
                RepeatNumCompaction = new BindableReactiveProperty<int>(Roller.CompactionParameter.RepeatNum)
                    .EnableValidation<ActivityEditorViewModel>(nameof(RepeatNumCompaction))
                    .AddTo(Disposables);
                RepeatNumMove = new BindableReactiveProperty<int>(Roller.MoveParameter.RepeatNum)
                    .EnableValidation<ActivityEditorViewModel>(nameof(RepeatNumMove))
                    .AddTo(Disposables);

                RefSpeedNonCompaction = new BindableReactiveProperty<double>(Roller.NonCompactionParameter.RefSpeed)
                    .EnableValidation<ActivityEditorViewModel>(nameof(RefSpeedNonCompaction))
                    .AddTo(Disposables);
                RefSpeedCompaction = new BindableReactiveProperty<double>(Roller.CompactionParameter.RefSpeed)
                    .EnableValidation<ActivityEditorViewModel>(nameof(RefSpeedCompaction))
                    .AddTo(Disposables);
                RefSpeedMove = new BindableReactiveProperty<double>(Roller.MoveParameter.RefSpeed)
                    .EnableValidation<ActivityEditorViewModel>(nameof(RefSpeedMove))
                    .AddTo(Disposables);

                EdgeSpeedNonCompaction = new BindableReactiveProperty<double>(Roller.NonCompactionParameter.EdgeSpeed)
                    .EnableValidation<ActivityEditorViewModel>(nameof(EdgeSpeedNonCompaction))
                    .AddTo(Disposables);
                EdgeSpeedCompaction = new BindableReactiveProperty<double>(Roller.CompactionParameter.EdgeSpeed)
                    .EnableValidation<ActivityEditorViewModel>(nameof(EdgeSpeedCompaction))
                    .AddTo(Disposables);
                EdgeSpeedMove = new BindableReactiveProperty<double>(Roller.MoveParameter.EdgeSpeed)
                    .EnableValidation<ActivityEditorViewModel>(nameof(EdgeSpeedMove))
                    .AddTo(Disposables);

                OutputFolderPath = _activityStore.OutputFolderPath.ToBindableReactiveProperty(_activityStore.OutputFolderPath.Value).AddTo(Disposables);
                AreaName = _activityStore.AreaName.ToBindableReactiveProperty(_activityStore.AreaName.Value).AddTo(Disposables);
                LiftName = _activityStore.LiftName.ToBindableReactiveProperty(_activityStore.LiftName.Value).AddTo(Disposables);
                InsertDate = _activityStore.InsertDate.ToBindableReactiveProperty(_activityStore.InsertDate.Value).AddTo(Disposables);
                OutputFolderPrefix = _activityStore.OutputFolderPrefix
                    .ToReadOnlyBindableReactiveProperty(_activityStore.OutputFolderPrefix.CurrentValue)
                    .AddTo(Disposables);

                ActivityCreated = _activityStore
                    .ActivityGroups
                    .ObserveCountChanged()
                    .Select(cnt => cnt > 0)
                    .ToBindableReactiveProperty(_activityStore.ActivityGroups.Count > 0)
                    .AddTo(Disposables);

                GroupIds = _groupIds
                    .CreateView(i => new GroupItem(i, $"Group {i+1}"))
                    .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current)
                    .AddTo(Disposables);
                SelectedGroupIndex = new BindableReactiveProperty<int?>(null).AddTo(Disposables);
                SelectedActivityIndex = new BindableReactiveProperty<int?>(null).AddTo(Disposables);
            }

            // init event
            {
                EnableMove.Subscribe(val => _activityStore.EnableMove = val).AddTo(Disposables);
                EnableNonCompaction.Subscribe(val => _activityStore.EnableNonCompaction = val).AddTo(Disposables);
                EnableCompaction.Subscribe(val => _activityStore.EnableCompaction = val).AddTo(Disposables);

                InitialMoveMarginRear.Subscribe(val => Roller.Zone.InitialMoveMarginBack = val).AddTo(Disposables);
                InitialMoveMarginFront.Subscribe(val => Roller.Zone.InitialMoveMarginFront = val).AddTo(Disposables);
                InitialMoveMarginNext.Subscribe(val => Roller.Zone.InitialMoveMarginNext = val).AddTo(Disposables);
                InitialMoveMarginPrev.Subscribe(val => Roller.Zone.InitialMoveMarginPrev = val).AddTo(Disposables);

                RepeatNumMove.Subscribe(val => Roller.MoveParameter.RepeatNum = val).AddTo(Disposables);
                RepeatNumNonCompaction.Subscribe(val => Roller.NonCompactionParameter.RepeatNum = val).AddTo(Disposables);
                RepeatNumCompaction.Subscribe(val => Roller.CompactionParameter.RepeatNum = val).AddTo(Disposables);

                RefSpeedMove.Subscribe(val => Roller.MoveParameter.RefSpeed = val).AddTo(Disposables);
                RefSpeedNonCompaction.Subscribe(val => Roller.NonCompactionParameter.RefSpeed = val).AddTo(Disposables);
                RefSpeedCompaction.Subscribe(val => Roller.CompactionParameter.RefSpeed = val).AddTo(Disposables);

                EdgeSpeedMove.Subscribe(val => Roller.MoveParameter.EdgeSpeed = val).AddTo(Disposables);
                EdgeSpeedNonCompaction.Subscribe(val => Roller.NonCompactionParameter.EdgeSpeed = val).AddTo(Disposables);
                EdgeSpeedCompaction.Subscribe(val => Roller.CompactionParameter.EdgeSpeed = val).AddTo(Disposables);

                AreaName.Subscribe(val => _activityStore.AreaName.Value = val).AddTo(Disposables);
                LiftName.Subscribe(val => _activityStore.LiftName.Value = val).AddTo(Disposables);
                InsertDate.Subscribe(val => _activityStore.InsertDate.Value = val).AddTo(Disposables);

                _activityStore
                    .ActivityGroups
                    .ObserveCountChanged()
                    .Subscribe(cnt =>
                    {
                        _groupIds.Clear();
                        if (cnt > 0)
                        {
                            _groupIds.AddRange(Enumerable.Range(0, cnt));
                        }
                    })
                    .AddTo(Disposables);

                _groupIds
                    .ObserveCountChanged()
                    .Subscribe(cnt =>
                    {
                        SelectedActivityIndex.Value = null;
                        if (cnt > 0)
                        {
                            //SelectedGroupIndex.Value = 0;
                            //SelectedActivityIndex.Value = 0;
                        }
                    })
                    .AddTo(Disposables);

                SelectedGroupIndex
                    .Subscribe(val =>
                    {
                        SelectedActivityIndex.Value = null;
                        if (val != null)
                        {
                            SelectedActivityIndex.Value = 0;
                            _activityService.SetCurrentGroup((int)val);
                        }
                    })
                    .AddTo(Disposables);

                SelectedActivityIndex
                    .Subscribe(val =>
                    {
                        try
                        {
                            if (val == null)
                            {
                                _activityService.GetActivityModel(-1, -1);
                                return;
                            }
                            if (SelectedGroupIndex.Value is int idx)
                            {
                                int m = _activityStore.ActivityGroups[idx].Length;
                                if (val < 0 || val >= m)
                                {
                                    SelectedActivityIndex.Value = (val % m + m) % m;
                                    return;
                                }
                                _activityService.GetActivityModel(idx, (int)val);
                            }
                        }
                        catch (Exception e)
                        {
                            System.Diagnostics.Debug.WriteLine(e);
                        }
                    })
                    .AddTo(Disposables);
            }


            // init command
            {
                NavigatePrevCommand = new ReactiveCommand(_ => _navigationService.NavigateTo<EditorLayoutViewModel, CanvasViewModel, WorkAreaEditorPanelViewModel>())
                    .AddTo(Disposables);

                CreateActivityCommand = CanCreateActivity
                    .ToReactiveCommand(async _ =>
                    {
                        try
                        {
                            _applicationStore.IsBusy.Value = true;

                            await Task.Run(() =>  _activityService.CreateActivityGroups());

                            if (_activityStore.ActivityGroups.Count > 0)
                            {
                                SelectedGroupIndex.Value = 0;
                            }

                            _notificationService.Notify(KWRPNotification.Create("アクティビティを生成しました", Backend.Enums.NotifyMessageType.Info, 5));

                            var allAcivities = _activityStore.ActivityGroups.SelectMany(act => act);
                            var moves = allAcivities.Where(act => act.ActivityType == Backend.Enums.RollerActivityType.Move).Count();
                            var nonCompactions = allAcivities.Where(act => act.ActivityType == Backend.Enums.RollerActivityType.NonCompaction).Count();
                            var compactions = allAcivities.Where(act => act.ActivityType == Backend.Enums.RollerActivityType.Compaction).Count();
                            _logService.LogInfo($"アクティビティを作成しました " +
                                $"move: {moves} noncompaction: {nonCompactions} compaction: {compactions} group count: {_activityStore.ActivityGroups.Count}");
                        }
                        catch (Exception ex)
                        {
                            _logService.LogError("アクティビティの生成に失敗しました", ex);
                            _notificationService.Notify(KWRPNotification.Create("アクティビティの生成に失敗しました", Backend.Enums.NotifyMessageType.Warn, 5));
                        }
                        finally
                        {
                            _logService.LogDebug("end CreateActivityCommand");
                            _applicationStore.IsBusy.Value = false;
                        }
                    },
                    initialCanExecute: _activityStore.CanCreateActivity)
                    .AddTo(Disposables);

                OutputCommand = OutputFolderPath
                    .Select(path => !string.IsNullOrEmpty(path) && !string.IsNullOrWhiteSpace(path))
                    .ToReactiveCommand(async _ =>
                    {
                        try
                        {
                            _applicationStore.IsBusy.Value = true; 
                            await _activityService.OutputActivitiesAsync();

                            _logService.LogInfo($"アクティビティを保存しました: {Path.Combine(OutputFolderPath.Value!, OutputFolderPrefix.Value)}");
                            _notificationService.Notify(
                                KWRPNotification.Create("アクティビティを出力しました", Backend.Enums.NotifyMessageType.Info, 10)
                                    .WithCommand(header: "フォルダを開く",
                                                 command: () =>
                                                 {
                                                     if (!string.IsNullOrWhiteSpace(OutputFolderPath.Value))
                                                     {
                                                         try
                                                         {
                                                             _pathService.SelectFileInExplorer(Path.Combine(OutputFolderPath.Value, OutputFolderPrefix.Value));
                                                         }
                                                         catch (Exception ex)
                                                         {
                                                             _logService.LogError("フォルダのオープンに失敗しました", ex);
                                                             _notificationService.Notify(
                                                                 KWRPNotification.Create("フォルダを開けませんでした", NotifyMessageType.Warn, 5));
                                                         }
                                                     }
                                                 }));
                        }
                        catch (Exception ex)
                        {
                            _logService.LogError("アクティビティの出力に失敗しました", ex);
                            _notificationService.Notify(KWRPNotification.Create("アクティビティの出力に失敗しました\n" + ex.Message, Backend.Enums.NotifyMessageType.Warn));
                        }
                        finally
                        {
                            _applicationStore.IsBusy.Value = false;
                            _logService.LogDebug("end OutputCommand");
                        }
                    })
                    .AddTo(Disposables);

                SetFolderCommand = new ReactiveCommand(async (_, ct) =>
                {
                    try
                    {
                        _applicationStore.IsBusy.Value = true;
                        await _activityService.SetOutputFolderPathAsync();
                    }
                    catch (Exception ex)
                    {
                        _logService.LogError("出力先選択時にエラーが発生しました", ex);
                    }
                    finally
                    {
                        _applicationStore.IsBusy.Value = false;
                    }
                })
                .AddTo(Disposables);
            }

            _logService.LogDebug("init");
            _logService.SetStatusMessage("アクティビティ設定UI : アクティビティの設定（レーン間移動、無起振、起振）、保存先を設定します。");
            
        }

        public override void Dispose()
        {
            _activityStore.ActivityGroups.Clear();
            base.Dispose();
        }

        public ReactiveCommand NavigatePrevCommand { get; }
        public ReactiveCommand CreateActivityCommand { get; }
        public ReactiveCommand OutputCommand { get; }
        public ReactiveCommand SetFolderCommand { get; }

        public BindableReactiveProperty<bool> CanCreateActivity { get; }
        public BindableReactiveProperty<bool> EnableNonCompaction { get; }
        public BindableReactiveProperty<bool> EnableCompaction { get; }
        public BindableReactiveProperty<bool> EnableMove { get; }

        [Range(1, KWRPConstants.C_MARGIN_MAX)]
        public BindableReactiveProperty<double> InitialMoveMarginRear { get; }
        [Range(1, KWRPConstants.C_MARGIN_MAX)]
        public BindableReactiveProperty<double> InitialMoveMarginFront { get; }
        [Range(1, KWRPConstants.C_MARGIN_MAX)]
        public BindableReactiveProperty<double> InitialMoveMarginPrev { get; }
        [Range(1, KWRPConstants.C_MARGIN_MAX)]
        public BindableReactiveProperty<double> InitialMoveMarginNext { get; }

        [Range(1, KWRPConstants.C_REPEAT_MAX)]
        public BindableReactiveProperty<int> RepeatNumNonCompaction { get; }
        [Range(1, KWRPConstants.C_REPEAT_MAX)]
        public BindableReactiveProperty<int> RepeatNumCompaction { get; }
        [Range(1, KWRPConstants.C_REPEAT_MAX)]
        public BindableReactiveProperty<int> RepeatNumMove { get; }
        [Range(0.0, KWRPConstants.C_SPEED_MAX)]

        public BindableReactiveProperty<double> RefSpeedNonCompaction { get; }
        [Range(0.0, KWRPConstants.C_SPEED_MAX)]
        public BindableReactiveProperty<double> RefSpeedCompaction { get; }
        [Range(0.0, KWRPConstants.C_SPEED_MAX)]
        public BindableReactiveProperty<double> RefSpeedMove { get; }

        [Range(0.0, KWRPConstants.C_SPEED_MAX)]
        public BindableReactiveProperty<double> EdgeSpeedNonCompaction { get; }
        [Range(0.0, KWRPConstants.C_SPEED_MAX)]
        public BindableReactiveProperty<double> EdgeSpeedCompaction { get; }
        [Range(0.0, KWRPConstants.C_SPEED_MAX)]
        public BindableReactiveProperty<double> EdgeSpeedMove { get; }


        public BindableReactiveProperty<string?> OutputFolderPath { get; }
        public BindableReactiveProperty<string?> AreaName { get; }
        public BindableReactiveProperty<string?> LiftName { get; }
        public BindableReactiveProperty<bool> InsertDate { get; }
        public IReadOnlyBindableReactiveProperty<string> OutputFolderPrefix { get; }

        public BindableReactiveProperty<bool> ActivityCreated { get; }
        private readonly ObservableList<int> _groupIds = [];
        public INotifyCollectionChangedSynchronizedViewList<GroupItem> GroupIds { get; }
        public BindableReactiveProperty<int?> SelectedGroupIndex { get; }
        public BindableReactiveProperty<int?> SelectedActivityIndex { get; }


    }
    public record struct GroupItem(int Index, string Tag);
}
