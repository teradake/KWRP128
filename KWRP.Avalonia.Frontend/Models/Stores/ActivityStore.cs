using KWRP.Avalonia.Backend.Model.Shapes.Activity;
using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Frontend.Services.Activity;
using ObservableCollections;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWRP.Avalonia.Frontend.Models.Stores
{
    public class ActivityStore
    {
        private readonly ILogService _logService;
        private readonly WorkAreaStore _workAreaStore;

        public ActivityStore(
            ILogService logService, 
            WorkAreaStore workAreaStore)
        {
            _logService = logService;
            _workAreaStore = workAreaStore;

            _workAreaStore
                .WorkAreas
                .ObserveChanged()
                .Subscribe(e =>ActivityGroups.Clear());

            OutputFolderPrefix = AreaName.CombineLatest(LiftName, InsertDate,
                (area, lift, insertDate) =>
                {
                    var sb = new StringBuilder().Append("WorkList_");
                    if (insertDate) sb.Append($"{DateTime.Now.ToString("yyyyMMdd")}_");
                    if (!string.IsNullOrEmpty(area)) sb.Append($"{area}_");
                    if (!string.IsNullOrEmpty(lift)) sb.Append($"{lift}_");
                    return sb.ToString()[..^1];
                })
                .ToReadOnlyReactiveProperty("WorkList");

            ActivityGroups
                .ObserveClear()
                .Subscribe(_ =>
                {
                    ActivityCards.Clear();
                    SelectedAcitivty.Value = null;
                    CurrentGroupItems.Clear();
                });

            _logService.LogDebug("init");
        }

        public ObservableList<ActivityModel[]> ActivityGroups { get; } = [];
        public ObservableList<ActivityModel> ActivityCards { get; } = [];
                
        public ReactiveProperty<ActivityModel?> SelectedAcitivty { get; } = new(null);
        public ObservableList<ActivityModel> CurrentGroupItems { get; } = [];


        public bool EnableMove { get; set; } = true;
        public bool EnableNonCompaction { get; set; } = true;
        public bool EnableCompaction { get; set; } = true;
        public bool CanCreateActivity => EnableNonCompaction || EnableCompaction || EnableMove;

        public ReactiveProperty<string?> OutputFolderPath { get; } = new(string.Empty);
        public ReactiveProperty<string?> AreaName { get; } = new(string.Empty);
        public ReactiveProperty<string?> LiftName { get; } = new(string.Empty);
        public ReactiveProperty<bool> InsertDate { get; } = new(true);
        public ReadOnlyReactiveProperty<string> OutputFolderPrefix { get; }
    }
}
