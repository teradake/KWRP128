using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Frontend.Models;
using KWRP.Avalonia.Frontend.Models.Stores;
using ObservableCollections;
using R3;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWRP.Avalonia.Frontend.ViewModels.EditorPage
{
    public class WorkAreaCutEditorPanelViewModel : ViewModelBase
    {
        private readonly ILogService _logService;
        private readonly IWorkAreaService _workAreaService;
        private readonly IWorkAreaCutService _workAreaCutService;
        private readonly WorkAreaStore _workAreaStore;

        public WorkAreaCutEditorPanelViewModel(ILogService logService, IWorkAreaService workAreaService, WorkAreaStore workAreaStore, IWorkAreaCutService workAreaCutService)
        {
            _logService = logService;
            _workAreaService = workAreaService;
            _workAreaStore = workAreaStore;
            _workAreaCutService = workAreaCutService;

            CutCount = _workAreaStore
                .CutCount
                .ToBindableReactiveProperty(_workAreaStore.CutCount.Value)
                .AddTo(Disposables);

            CutCount
                .Subscribe(cnt =>
                {
                    _workAreaCutService.InitializeCutProperties(cnt);
                })
                .AddTo(Disposables);

            SelectedWorkAreaCount = _workAreaStore
                .SelectedWorkAreas
                .ObserveCountChanged()
                .ToBindableReactiveProperty(_workAreaStore.SelectedWorkAreas.Count)
                .AddTo(Disposables);

            CutCommand = SelectedWorkAreaCount
                .CombineLatest(CutCount, (a, cut) => a > 0 && cut > 1)
                .ToReactiveCommand(_ =>
                {
                    try
                    {
                        _workAreaCutService.Cut();
                    }
                    catch (Exception ex)
                    {
                        _logService.LogError(ex.ToString());
                    }
                }, initialCanExecute: _workAreaStore.SelectedWorkAreas.Count > 0 && _workAreaStore.CutCount.Value > 0)
                .AddTo(Disposables);

            CutProperties = _workAreaStore
                .CutProperties
                .CreateView(prop => prop)
                .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current)
                .AddTo(Disposables);
        }

        public ReactiveCommand CutCommand { get; }
        public BindableReactiveProperty<int> CutCount { get; }
        public BindableReactiveProperty<int> SelectedWorkAreaCount { get; }

        public INotifyCollectionChangedSynchronizedViewList<CutProperty> CutProperties { get; }
    }


    //public class WorkAreaCutEditorPanelViewModel : ViewModelBase
    //{
    //    private readonly ILogService _logService;
    //    private readonly IWorkAreaService _workAreaService;
    //    private readonly WorkAreaStore _workAreaStore;

    //    public WorkAreaCutEditorPanelViewModel(
    //        ILogService logService,
    //        WorkAreaStore workAreaStore,
    //        IWorkAreaService workAreaService)
    //    {
    //        _logService = logService;
    //        _workAreaStore = workAreaStore;
    //        _workAreaService = workAreaService;

    //        CutCount = _workAreaStore
    //            .CutCount
    //            .ToBindableReactiveProperty(_workAreaStore.CutCount.Value)
    //            .AddTo(Disposables);

    //        CutCount
    //            .Subscribe(cnt =>
    //            {
    //                _workAreaService.InitializeCutProperties(cnt);
    //            })
    //            .AddTo(Disposables);

    //        CutCommand = CutCount
    //            .Select(val => val > 0)
    //            .ToReactiveCommand(_ =>
    //            {
    //                var p = CutProperties.Select(p => p.Ratio).ToList();
    //            })
    //            .AddTo(Disposables);

    //        CutProperties = _workAreaStore
    //            .CutProperties
    //            .CreateView(prop => prop)
    //            .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current)
    //            .AddTo(Disposables);

    //        _workAreaStore
    //            .CutProperties
    //            .ObserveChanged()
    //            .Subscribe(p =>
    //            {
    //                System.Diagnostics.Debug.WriteLine("prop changed2");
    //            })
    //            .AddTo(Disposables);

    //        _workAreaStore
    //            .Obser


    //        _logService.LogDebug("init");
    //    }

    //    public ReactiveCommand CutCommand { get; }

    //    public BindableReactiveProperty<int> CutCount { get; } 
    //    public INotifyCollectionChangedSynchronizedViewList<CutProperty> CutProperties { get; }
    //}


    //public class CutPropertyViewModel : ViewModelBase
    //{
    //    private readonly CutProperty _prop;

    //    public CutPropertyViewModel(CutProperty prop)
    //    {
    //        _prop = prop;

    //        Ratio = new BindableReactiveProperty<int>(_prop.Ratio).AddTo(Disposables);
    //    }

    //    public BindableReactiveProperty<int> Ratio { get; }
    //    public string Percentage => $"{_prop.Percentage:0.0}%";

    //    public override void Dispose()
    //    {
    //        System.Diagnostics.Debug.WriteLine("disposed cutproperty");
    //        base.Dispose();
    //    }
    //}
}
