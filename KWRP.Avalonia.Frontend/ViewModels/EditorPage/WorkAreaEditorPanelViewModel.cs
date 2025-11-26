using KWRP.Avalonia.Backend.Enums;
using KWRP.Avalonia.Backend.Model.Shapes.WorkArea;
using KWRP.Avalonia.Backend.Models;
using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Frontend.Models.Stores;
using KWRP.Avalonia.Frontend.Services;
using KWRP.Avalonia.Frontend.ViewModels.Shapes;
using KWRP.Avalonia.Frontend.ViewModels.StartUpPage;
using Microsoft.Extensions.DependencyInjection;
using ObservableCollections;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWRP.Avalonia.Frontend.ViewModels.EditorPage
{
    public class WorkAreaEditorPanelViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly CanvasItemStore _canvasItemStore;
        private readonly WorkAreaStore _workAreaStore;
        private readonly IWorkAreaService _workAreaService;
        private readonly ILogService _logService;
        private readonly INotificationService _notificationService;
        private readonly IServiceProvider _serviceProvider;

        public WorkAreaEditorPanelViewModel(
            INavigationService navigationService,
            CanvasItemStore canvasItemStore,
            WorkAreaStore workAreaStore,
            IWorkAreaService workAreaService,
            ILogService logService,
            INotificationService notificationService,
            IServiceProvider serviceProvider)
        {
            _navigationService = navigationService;
            _canvasItemStore = canvasItemStore;
            _workAreaStore = workAreaStore;
            _workAreaService = workAreaService;
            _logService = logService;
            _notificationService = notificationService;
            _serviceProvider = serviceProvider;


            WorkAreaCreated = _workAreaStore
                .WorkAreas
                .ObserveCountChanged()
                .Select(cnt => cnt > 0)
                .ToBindableReactiveProperty(_workAreaStore.WorkAreas.Count > 0)
                .AddTo(Disposables);

            WorkAreaCount = _workAreaStore
                .WorkAreas
                .ObserveCountChanged()
                .ToReadOnlyBindableReactiveProperty(_workAreaStore.WorkAreas.Count)
                .AddTo(Disposables);

            SelectedWorkAreaCount = _workAreaStore
                .SelectedWorkAreas
                .ObserveCountChanged()
                .ToBindableReactiveProperty(_workAreaStore.SelectedWorkAreas.Count)
                .AddTo(Disposables);

            NavigateNextCommand = WorkAreaCreated
                .ToReactiveCommand(_ => _navigationService.NavigateTo<EditorLayoutViewModel, CanvasViewModel, ActivityEditorViewModel>())
                .AddTo(Disposables);

            NavigatePrevCommand = new ReactiveCommand(_ => _navigationService.NavigateTo<EditorLayoutViewModel, CanvasViewModel, LaneCreationPanelViewModel>())
                .AddTo(Disposables);

            ClearCommand = SelectedWorkAreaCount
                .Select(cnt => cnt > 0)
                .ToReactiveCommand(_ => _workAreaService.ClearSelectedWorkAreas())
                .AddTo(Disposables);

            UndoCommand = _workAreaStore
                .History
                .ObserveCountChanged()
                .Select(cnt => cnt > 0)
                .ToReactiveCommand(_ => _workAreaService.Undo(), initialCanExecute: _workAreaStore.History.Count > 0)
                .AddTo(Disposables);

            RefleshCommand = new ReactiveCommand(_ => _workAreaService.InitializeWorkAreas()).AddTo(Disposables);

            GoalAreaCommand = CreateWorkAreaCommand<GoalAreaLocation>((attr, value) => attr.SetGoalArea(value));
            LaneTrimFrontCommand = CreateWorkAreaCommand<WorkAreaLaneTrimType>((attr, value) => attr.SetLaneTrimTypeFront(value));
            LaneTrimRearCommand = CreateWorkAreaCommand<WorkAreaLaneTrimType>((attr, value) => attr.SetLaneTrimTypeRear(value));
            RollerDirectionSwapCommand = CreateWorkAreaCommand<bool>((attr, value) => attr.SetIsDirectionSwap(value));

            // initialization
            if (_canvasItemStore.LanesUpdated)
            {
                try
                {
                    _workAreaService.InitializeWorkAreas();
                    _logService.LogInfo("作業エリア初期化");
                }
                catch (Exception ex)
                {
                    _logService.LogError("作業エリア初期化に失敗", ex);
                    _notificationService.Notify(KWRPNotification.Create(
                        message: $"作業エリア初期化に失敗しました。{ex.Message}",
                        NotifyMessageType.Warn,
                        duration: 10));
                    _workAreaService.ClearSelectedWorkAreas();
                }
            }

            _logService.LogDebug($"init: workareas: {WorkAreaCount.Value}({_workAreaStore.WorkAreas.Count})");
            _logService.SetStatusMessage("Domain.Front.WorkAreaSettingsUI");
        }


        public ReactiveCommand NavigateNextCommand { get; }
        public ReactiveCommand NavigatePrevCommand { get; }
        public ReactiveCommand ClearCommand { get; }
        public ReactiveCommand UndoCommand { get; }
        public ReactiveCommand RefleshCommand { get; }
        public ReactiveCommand<GoalAreaLocation> GoalAreaCommand { get; }
        public ReactiveCommand<WorkAreaLaneTrimType> LaneTrimFrontCommand { get; }
        public ReactiveCommand<WorkAreaLaneTrimType> LaneTrimRearCommand { get; }
        public ReactiveCommand<bool> RollerDirectionSwapCommand { get; }

        public BindableReactiveProperty<bool> WorkAreaCreated { get; }
        public IReadOnlyBindableReactiveProperty<int> WorkAreaCount { get; }
        public BindableReactiveProperty<int> SelectedWorkAreaCount { get; }

        private ViewModelBase? _cutEditor = null;
        public ViewModelBase CutEditor => _cutEditor ??= _serviceProvider.GetRequiredService<WorkAreaCutEditorPanelViewModel>();


        ReactiveCommand<T> CreateWorkAreaCommand<T>(Func<WorkAreaAttribute, T, WorkAreaAttribute> updateFunc)
        {
            return _workAreaStore
                .SelectedWorkAreas
                .ObserveCountChanged()
                .Select(cnt => cnt > 0)
                .ToReactiveCommand<T>(value =>
                {
                    _workAreaService.UpdateWorkAreas(workArea => workArea.CloneBy(
                        attribute: updateFunc(workArea.Attribute, value).SetIsSelected(true)));
                }, 
                initialCanExecute: _workAreaStore.SelectedWorkAreas.Count > 0)
                .AddTo(Disposables);
        }

        public override void Dispose()
        {
            _workAreaService.ClearSelectedWorkAreas();
            _cutEditor?.Dispose();
            base.Dispose();
        }
    }
}
