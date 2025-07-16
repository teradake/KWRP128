using KWRP.Avalonia.Backend.Model.Shapes.WorkArea;
using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Frontend.ViewModels.Shapes;
using ObservableCollections;
using R3;
using System.Collections;
using System.Collections.Generic;

namespace KWRP.Avalonia.Frontend.Models.Stores
{
    public class WorkAreaStore
    {
        private readonly CanvasItemStore _canvasItemStore;
        private readonly ILogService _logService;


        public WorkAreaStore(CanvasItemStore canvasItemStore, ILogService logService)
        {
            _canvasItemStore = canvasItemStore;
            _logService = logService;

            _canvasItemStore
                .PairedLanes
                .ObserveClear()
                .Subscribe(_ =>
                {
                    WorkAreas.Clear();
                });

            WorkAreas
                .ObserveClear()
                .Subscribe(_ =>
                {
                    SelectedWorkAreas.Clear();
                    History.Clear();
                    CutProperties.Clear();
                });

            _logService.LogDebug("init");
        }

        //public ObservableHashSet<WorkAreaModel> WorkAreas { get; } = [];
        public ObservableDictionary<WorkAreaModel, WorkAreaViewModel> WorkAreas { get; } = [];
        public ObservableHashSet<WorkAreaModel> SelectedWorkAreas { get; } = [];

        public ReactiveProperty<int> CutCount { get; } = new(0);
        public ObservableList<CutProperty> CutProperties { get; } = [];

        public ObservableFixedSizeRingBuffer<WorkAreaHistory> History { get; } = new(50);
    }

    public class WorkAreaHistory
    {
        public IList<WorkAreaModel> Added { get; init; } = [];
        public IList<WorkAreaModel> Removed { get; init; } = [];
    }
}
