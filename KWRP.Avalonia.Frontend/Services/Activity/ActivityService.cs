using KWRP.Avalonia.Backend.Model.Shapes.Activity;
using KWRP.Avalonia.Backend.Model.Shapes.WorkArea;
using KWRP.Avalonia.Backend.Models.Roller;
using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Backend.Services.Activity;
using KWRP.Avalonia.Backend.Services.Extensions;
using KWRP.Avalonia.Frontend.Models.Stores;
using KWRP.Backend.Model.Modlules.WorkTimeEstimator;
using KWRP.Backend.Model.Shapes.Activity.Entity;
using NetTopologySuite.Algorithm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using Trdk.Geometry;

namespace KWRP.Avalonia.Frontend.Services.Activity
{
    public class ActivityService : IActivityService
    {
        private readonly ActivityStore _activityStore;
        private readonly ILogService _logService;
        private readonly INotificationService _notificationService;
        private readonly WorkAreaStore _workAreaStore;
        private readonly MachineStore _machineStore;
        private readonly IPathService _pathService;
        private readonly CaptureService _captureService;
        private readonly CadScriptService _cadScriptService;
        private readonly CanvasItemStore _canvasItemStore;
        private readonly ParameterStore _parameterStore;

        private RollerModel VR => _machineStore.CurrentRoller;

        public ActivityService(
            ActivityStore activityStore,
            ILogService logService,
            INotificationService notificationService,
            WorkAreaStore workAreaStore,
            MachineStore machineStore,
            IPathService pathService,
            CaptureService captureService,
            CadScriptService cadScriptService,
            CanvasItemStore canvasItemStore,
            ParameterStore parameterStore)
        {
            _activityStore = activityStore;
            _logService = logService;
            _notificationService = notificationService;
            _workAreaStore = workAreaStore;
            _machineStore = machineStore;
            _pathService = pathService;
            _captureService = captureService;
            _cadScriptService = cadScriptService;
            _canvasItemStore = canvasItemStore;
            _parameterStore = parameterStore;

            _logService.LogDebug("init");
        }

        public virtual void CreateActivityGroups()
        {
            _logService.LogInfo($"アクティビティを作成します。workArea: {_workAreaStore.WorkAreas.Count}");

            // init
            _activityStore.ActivityGroups.Clear();


            // create working sequences
            var workAreas = _workAreaStore.WorkAreas
                .Select(kv => kv.Key)
                .OrderBy(workArea => workArea.Xmin)
                .ToArray();
            int N = workAreas.Length;

            var adjacentyList = Enumerable.Range(0, N).Select(_ => new List<int>()).ToArray();
            var degreeIn = new int[N];
            var degreeOut = new int[N];

            for (int i = 0; i < N; ++i)
            {
                for (int j = i + 1; j < N; ++j)
                {
                    if (CanConnect(workAreas[i], workAreas[j]))
                    {
                        degreeOut[i]++;
                        degreeIn[j]++;
                        adjacentyList[i].Add(j);
                    }
                }
            }

            // generate workAreaSequence
            var visited = new bool[N];
            var stack = new Stack<int>();
            var starters = Enumerable
                .Range(0, N)
                .Where(i => degreeIn[i] == 0)
                .OrderByDescending(i => workAreas[i].Xmin);

            foreach (var start in starters)
            {
                stack.Push(~start);
                stack.Push(start);
            }


            _logService.LogInfo($"start dfs");
            var temporalSequence = new List<int>();
            var sequences = new List<int[]>();
            while (stack.Count > 0)
            {
                int now = stack.Pop();
                if (now >= 0)
                {
                    temporalSequence.Add(now);
                    foreach (var next in adjacentyList[now])
                    {
                        if (visited[next])
                            continue;
                        stack.Push(~next);
                        stack.Push(next);
                    }
                }
                else
                {
                    if (temporalSequence.Count == 0)
                        continue;
                    sequences.Add(temporalSequence.ToArray());
                    foreach (var id in temporalSequence)
                    {
                        visited[id] = true;
                    }
                    temporalSequence.Clear();
                }
            }

            foreach (var (seq, seqID) in sequences.Select((seq, id) => (seq, id)))
            {
                var workAreaSeq = seq.Select(i => workAreas[i]).ToArray();
                RegisterSequences(seqID, workAreaSeq);
            }
        }

        bool CanConnect(WorkAreaModel src, WorkAreaModel dst)
        {
            if (src == null || dst == null) return false;
            if (src.Attribute.IsDirectionSwap ^ dst.Attribute.IsDirectionSwap) return false;   // 進行方向が異なる場合不可

            double xLap = Math.Min(src.Xmax, dst.Xmax) - Math.Max(src.Xmin, dst.Xmin);
            if (!Utils.IsPositive(xLap)) return false;                                              // 進捗方向について領域が重複していない場合不可
            if (Utils.IsZero(xLap - src.Width) || Utils.IsZero(xLap - dst.Width)) return false;      // 上下方向には動かさない

            var sLane = src.OrthogonalLanes[^1];
            var tLane = dst.OrthogonalLanes[0];
            double yLap = Math.Min(sLane.AaBB.Ymax, tLane.AaBB.Ymax) - Math.Max(sLane.AaBB.Ymin, tLane.AaBB.Ymin);
            //if (!Utils.IsPositive(VR.Zone.LaneChangeLength - yLap)) return false;
            if (yLap < VR.Zone.LaneChangeLength) return false;

            return true;
        }


        /// <summary>
        /// 実際にアクティビティのインスタンス生成して登録するためのメソッド
        /// </summary>
        /// <param name="sequenceID">GroupIdに相当。0-based</param>
        /// <param name="workAreaSequence"></param>
        /// <returns></returns>
        protected ActivityModel[] RegisterSequences(int sequenceID, WorkAreaModel[] workAreaSequence)
        {
            _logService.LogDebug("register");

            var activities = new List<ActivityModel>();
            var representatives = new List<ActivityModel>();
            var workTimes = new List<TimeSpan>();

            var actBuilder = new ActivityBuilder()
                .SetRoller(VR)
                .SetWorkTimeEstimatorOption(ActivityWorkTimeEstimatorOption.BuildByRoller(VR));

            sequenceID++;   // 1-basedに変換
            foreach (var (workArea, i) in workAreaSequence.Select((wa, id) => (wa, id + 1)))    // 1-basedに変換
            {
                ActivityModel? represent = null;
                TimeSpan workTime = TimeSpan.Zero;
                actBuilder.SetCurrentWorkArea(workArea);

                if (_activityStore.EnableMove)
                {
                    var act = actBuilder.BuildMoveActivity(sequenceID, i);
                    activities.Add(act);
                    workTime += act.WorkTime;
                }

                if (_activityStore.EnableNonCompaction)
                {
                    var act = actBuilder.BuildNonCompactionActivity(sequenceID, i);
                    activities.Add(act);
                    workTime += act.WorkTime;
                    represent = act;
                }

                if (_activityStore.EnableCompaction)
                {
                    var act = actBuilder.BuildCompactionActivity(sequenceID, i);
                    activities.Add(act);
                    workTime += act.WorkTime;
                    represent = act;
                }

                workTimes.Add(workTime);
                if (represent != null)
                {
                    representatives.Add(represent);
                }
            }

            var acts = activities.ToArray();
            _activityStore.ActivityGroups.Add(acts);
            _activityStore.ActivityGroupWorkTimes.Add(workTimes.ToArray());
            _activityStore.ActivityCards.AddRange(representatives);
            return acts;
        }

        public async Task SetOutputFolderPathAsync()
        {
            try
            {
                var path = await _pathService.GetSaveFolderPathAsync(title: "アクティビティ出力先のフォルダを選択してください");
                if (path == null)
                {
                    _logService.LogInfo("アクティビティ出力先のフォルダ選択をキャンセルしました");
                    return;
                }

                _activityStore.OutputFolderPath.Value = path;
            }
            catch (Exception ex)
            {
                _activityStore.OutputFolderPath.Value = string.Empty;
                _logService.LogWarn($"アクティビティ出力先フォルダ選択時にエラーが発生しました: {ex.Message}");
                throw;
            }
        }

        public ActivityModel? GetActivityModel(int groupId, int index)
        {
            int n = _activityStore.ActivityGroups.Count;
            if (groupId < 0 || groupId >= n)
            {
                _activityStore.SelectedAcitivty.Value = null;
                return null;
            }

            int m = _activityStore.ActivityGroups[groupId].Length;
            if (index < 0 || index >= m)
            {
                _activityStore.SelectedAcitivty.Value = null;
                return null;
            }

            var act = _activityStore.ActivityGroups[groupId][index];
            _activityStore.SelectedAcitivty.Value = act;
            return act;
        }

        public async Task OutputActivitiesAsync()
        {
            if (_activityStore.OutputFolderPath.Value == null)
            {
                _logService.LogWarn("出力先フォルダが指定されていません。アクティビティ出力を中止します。");
                return;
            }

            var targetFolder = Path.Combine(_activityStore.OutputFolderPath.Value, _activityStore.OutputFolderPrefix.CurrentValue);
            if (Directory.Exists(targetFolder))
            {
                _logService.LogWarn(
                    $"フォルダ「{targetFolder}」がすでに存在します");
                throw new IOException(
                    $"フォルダ「{targetFolder}」がすでに存在します。\n" +
                    $"該当するフォルダを削除するか、保存するフォルダ名を変更してください。");
            }
            else
            {
                Directory.CreateDirectory(targetFolder);
            }

            if (!_pathService.TryValidatePath(targetFolder, out string? reason))
            {
                _logService.LogWarn($"{reason}。アクティビティ出力を中止します。");
                throw new IOException(reason);
            }

            // Output Activity & ActivityListJson
            var activityList = new List<RollerActivityEntity>();
            for (int groupId = 0; groupId < _activityStore.ActivityGroups.Count; ++groupId)
            {
                var outputFolder = $"Group{groupId + 1}";
                var outputDirectoryPath = Path.Combine(targetFolder, outputFolder);
                if (!Directory.Exists(outputDirectoryPath))
                {
                    Directory.CreateDirectory(outputDirectoryPath);
                }

                foreach (ActivityModel act in _activityStore.ActivityGroups[groupId].Where(act => act.GroupId == groupId + 1))
                {
                    //var fileName = $"{_activityStore.OutputFolderPrefix.CurrentValue}_{act.GroupId:d2}_{act.OutputFIlePrefix}.csv";
                    var fileName = $"{act.GroupId:d2}_{act.OutputFIlePrefix}.csv";
                    var filePath = Path.Combine(outputDirectoryPath, fileName);

                    try
                    {
                        await using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                        await using var writer = new StreamWriter(fs);
                        await writer.WriteAsync(act.ToString());
                    }
                    catch (Exception ex)
                    {
                        _logService.LogError($"ファイル{fileName}保存時にエラーが発生しました。{ex}");
                        throw;
                    }

                    try
                    {
                        var entity = act.ToEntity(_parameterStore.ProgresssDirectionRadian.Value);
                        if (entity is { })
                        {
                            activityList.Add(entity);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logService.LogError($"ActivityModelからActivityEntityへの変換に失敗しました。{ex}");
                        throw;
                    }
                }
            }

            try
            {
                var jsonName = $"{_activityStore.OutputFolderPrefix.CurrentValue}_ActivityList.json";
                var path = Path.Combine(targetFolder, jsonName);
                var json = System.Text.Json.JsonSerializer.Serialize(activityList, new System.Text.Json.JsonSerializerOptions()
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    WriteIndented = true,
                });
                await using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
                await using var writer = new StreamWriter(fs);
                await writer.WriteAsync(json);
            }
            catch (Exception e)
            {
                _logService.LogWarn("ActivityListJsonの保存に失敗しました" + e);
            }

            // Output WorkTime
            try
            {
                {
                    var fileName = $"{_activityStore.OutputFolderPrefix.CurrentValue}_作業時間.csv";
                    var path = Path.Combine(targetFolder, fileName);

                    var sb = new StringBuilder();
                    sb.AppendLine("エリア名,作業時間,残時間");
                    for (int groupId = 0; groupId < _activityStore.ActivityGroupWorkTimes.Count; ++groupId)
                    {
                        var remain = TimeSpan.Zero;
                        foreach (var t in _activityStore.ActivityGroupWorkTimes[groupId])
                            remain += t;

                        foreach (var (areaId, t) in _activityStore.ActivityGroupWorkTimes[groupId].Select((t, id) => (id, t)))
                        {
                            var tag = $"{groupId}_{areaId}";
                            sb.AppendLine($"{tag},{t:hh\\:mm},{remain:hh\\:mm}");
                            remain -= t;
                        }
                        sb.AppendLine();
                    }

                    // csvに出力
                    await using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
                    await using var writer = new StreamWriter(fs, new System.Text.UTF8Encoding(true));
                    await writer.WriteAsync(sb.ToString());
                }

                {
                    var fileName = $"{_activityStore.OutputFolderPrefix.CurrentValue}_作業時間.txt";
                    var path = Path.Combine(targetFolder, fileName);

                    var sb = new StringBuilder();
                    for (int groupId = 0; groupId < _activityStore.ActivityGroupWorkTimes.Count; ++groupId)
                    {
                        sb.AppendLine("エリア名    作業時間    残時間");
                        sb.AppendLine("----------------------------------------");
                        var remain = TimeSpan.Zero;
                        foreach (var t in _activityStore.ActivityGroupWorkTimes[groupId])
                            remain += t;

                        foreach (var (areaId, t) in _activityStore.ActivityGroupWorkTimes[groupId].Select((t, id) => (id, t)))
                        {
                            var tag = $"{groupId}_{areaId}";
                            sb.AppendLine($"{tag}        {t:hh\\:mm}        {remain:hh\\:mm}");
                            remain -= t;
                        }
                        sb.AppendLine();
                    }

                    // csvに出力
                    await using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
                    await using var writer = new StreamWriter(fs);
                    await writer.WriteAsync(sb.ToString());
                }
            }
            catch (Exception e)
            {
                _logService.LogWarn("作業時間の出力に失敗しました" + e);
            }



            // Output PlanView (png)
            try
            {
                var pngName = $"{_activityStore.OutputFolderPrefix.CurrentValue}_plan.png";
                var path = Path.Combine(targetFolder, pngName);
                await _captureService.SavePlanViewAsync(path);
            }
            catch (Exception e)
            {
                _logService.LogWarn("キャプチャの作成に失敗しました" + e);
            }

            // Output CadScript (scr)
            try
            {
                var scrName = $"{_activityStore.OutputFolderPrefix.CurrentValue}_plan.scr";
                var path = Path.Combine(targetFolder, scrName);
                await _cadScriptService.CreateCadScriptAsync(path);
            }
            catch (Exception e)
            {
                _logService.LogWarn("CadScriptの作成に失敗しました" + e);
            }

            // Output Perimeter (csv)
            try
            {
                var outputFolder = $"法肩ライン";
                var outputDirectoryPath = Path.Combine(targetFolder, outputFolder);
                if (!Directory.Exists(outputDirectoryPath))
                {
                    Directory.CreateDirectory(outputDirectoryPath);
                }

                var prefix = _activityStore.OutputFolderPrefix.CurrentValue;
                var outputAreasTask = ExportAreasAsync(_canvasItemStore.CompactionAreas, prefix, "法肩ライン", outputDirectoryPath);
                var outputObstacleTask = ExportAreasAsync(_canvasItemStore.Holes, prefix, "法肩ライン障害物", outputDirectoryPath);

                var occs = _activityStore
                    .ActivityGroups
                    .Where(g => g.Length > 0)
                    .Select(g => g.First())
                    .Where(act => act.ActivityType == Backend.Enums.RollerActivityType.Move)
                    .Select(act => act.OccArea.Shape);
                var outputStartOccAreaTask = ExportOccAreaAsync(occs, prefix, "初期移動占有エリア", outputDirectoryPath);

                await Task.WhenAll(outputAreasTask, outputObstacleTask, outputStartOccAreaTask);
            }
            catch (Exception e)
            {
                _logService.LogWarn("外形線の作成に失敗しました" + e);
            }
        }

        private async Task ExportAreasAsync(IEnumerable<Polygon> areas, string filePrefix, string fileSuffix, string outputDirectory)
        {
            foreach (var (area, index) in areas.Select((a, i) => (a, i)))
            {
                var fileName = $"{fileSuffix}_{index:D2}.csv";
                var path = Path.Combine(outputDirectory, fileName);
                try
                {
                    await using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
                    await using var writer = new StreamWriter(fs);
                    await writer.WriteAsync(area.ToSlopeLine());
                }
                catch (Exception ex)
                {
                    _logService.LogError($"ファイル {fileName} 保存時にエラーが発生しました。{ex}");
                    throw;
                }
            }
        }

        private async Task ExportOccAreaAsync(IEnumerable<Polygon> areas, string filePrefix, string fileSuffix, string outputDirectory)
        {
            foreach (var (area, index) in areas.Select((a, i) => (a, i)))
            {
                var fileName = $"{fileSuffix}_Group{index:D2}.csv";
                var path = Path.Combine(outputDirectory, fileName);
                try
                {
                    await using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
                    await using var writer = new StreamWriter(fs);
                    await writer.WriteAsync(area.ToSlopeLine());
                }
                catch (Exception ex)
                {
                    _logService.LogError($"ファイル {fileName} 保存時にエラーが発生しました。{ex}");
                    throw;
                }
            }
        }

        public void SetCurrentGroup(int groupId)
        {
            if (_activityStore.ActivityGroups.Count == 0) return;

            int n = _activityStore.ActivityGroups.Count;
            groupId = (groupId % n + n) % n;
            _activityStore.CurrentGroupItems.Clear();

            var acts = _activityStore.ActivityGroups[groupId].Where(area => area.ActivityType == Backend.Enums.RollerActivityType.Compaction);
            if (!acts.Any())
            {
                acts = _activityStore.ActivityGroups[groupId].Where(area => area.ActivityType == Backend.Enums.RollerActivityType.NonCompaction);
            }
            _activityStore.CurrentGroupItems.AddRange(acts);
        }
    }
}
