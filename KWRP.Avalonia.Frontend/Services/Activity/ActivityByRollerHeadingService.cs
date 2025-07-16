using KWRP.Avalonia.Backend.Model.Shapes.Activity;
using KWRP.Avalonia.Backend.Model.Shapes.WorkArea;
using KWRP.Avalonia.Backend.Models.Roller;
using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Backend.Services.Activity;
using KWRP.Avalonia.Backend.Services.Extensions;
using KWRP.Avalonia.Frontend.Models.Stores;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Trdk.Geometry;

namespace KWRP.Avalonia.Frontend.Services.Activity
{
    public class ActivityByRollerHeadingService : IActivityService
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

        private RollerModel VR => _machineStore.CurrentRoller;

        public ActivityByRollerHeadingService(
            ActivityStore activityStore,
            ILogService logService,
            INotificationService notificationService,
            WorkAreaStore workAreaStore,
            MachineStore machineStore,
            IPathService pathService,
            CaptureService captureService,
            CadScriptService cadScriptService,
            CanvasItemStore canvasItemStore)
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

            _logService.LogDebug("init");
        }

        public void CreateActivityGroups()
        {
            _logService.LogInfo($"アクティビティを作成します。workArea: {_workAreaStore.WorkAreas.Count}");

            // init
            _activityStore.ActivityGroups.Clear();


            // create working sequences
            var workAreas = _workAreaStore.WorkAreas
                .Select(kv => kv.Key)
                .OrderBy(workArea => new Vec2(workArea.Xmin, workArea.Ymin))
                .ToArray();
            int N = workAreas.Length;

            var adjacentyList = Enumerable.Range(0, N).Select(_ => new List<int>()).ToArray();
            var degreeIn = new int[N];
            var degreeOut = new int[N];

            // ローラの向きについて有向グラフをつくる
            for (int i = 0; i < N; ++i)
            {
                for (int j = 0; j < N; ++j)
                {
                    if (i == j)
                        continue;
                    if (CanConnectRollerHeadingDirection(workAreas[i], workAreas[j]))
                    {
                        degreeOut[i]++;
                        degreeIn[j]++;
                        adjacentyList[i].Add(j);
                    }
                }
            }

            // 余ったやつでは、作業進捗方向について有向グラフを作る
            var restAreas = Enumerable.Range(0, N).Where(i => degreeIn[i] + degreeOut[i] == 0).ToArray();
            for (int i = 0; i < restAreas.Length; ++i)
            {
                for (int j = i + 1; j < restAreas.Length; ++j)
                {
                    int p = restAreas[i];
                    int q = restAreas[j];
                    if (CanConnectProgressDirection(workAreas[p], workAreas[q]))
                    {
                        degreeOut[p]++; 
                        degreeIn[q]++;
                        adjacentyList[p].Add(q);
                    }
                }
            }

            for (int i = 0; i < N; ++i)
            {
                if (workAreas[i].Orientation == Backend.Enums.WorkAreaOrientation.Up)
                {
                    adjacentyList[i].Sort((p, q) => workAreas[p].Shape.Centroid.CompareTo(workAreas[q].Shape.Centroid));
                }
                else
                {
                    adjacentyList[i].Sort((p, q) => workAreas[q].Shape.Centroid.CompareTo(workAreas[p].Shape.Centroid));
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

        bool CanConnectRollerHeadingDirection(WorkAreaModel src, WorkAreaModel dst)
        {
            // ローラの向いている方向につなぐ

            if (src == null || dst == null) return false;
            if (src.Attribute.IsDirectionSwap ^ dst.Attribute.IsDirectionSwap) return false;   // 進行方向が異なる場合不可

            if (src.Orientation == Backend.Enums.WorkAreaOrientation.Up)
            {
                if (src.OrthogonalLanes[0].Centroid.Y > dst.OrthogonalLanes[0].Centroid.Y)
                    return false;
            }
            else
            {
                if (src.OrthogonalLanes[0].Centroid.Y < dst.OrthogonalLanes[0].Centroid.Y)
                    return false;
            }

            double xLap = Math.Min(src.Xmax, dst.Xmax) - Math.Max(src.Xmin, dst.Xmin);
            if (!Utils.IsPositive(xLap)) return false;                                              // 進捗方向について領域が重複していない場合不可

            double yLap = Math.Min(src.Ymax, dst.Ymax) - Math.Max(src.Ymin, dst.Ymin);
            if (!Utils.IsPositive(yLap)) return false;                                              // ローラ向きについて領域が重複していない場合不可

            if (Utils.IsZero(xLap - src.Width) || Utils.IsZero(xLap - dst.Width))
                return true;

            return false;
        }

        bool CanConnectProgressDirection(WorkAreaModel src, WorkAreaModel dst)
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


        ActivityModel[] RegisterSequences(int sequenceID, WorkAreaModel[] workAreaSequence)
        {
            _logService.LogDebug("register");

            var activities = new List<ActivityModel>();
            var representatives = new List<ActivityModel>();
            var actBuilder = new ActivityBuilder().SetRoller(VR);
            for (int i = 0; i < workAreaSequence.Length; ++i)
            {
                ActivityModel? represent = null;
                actBuilder.SetCurrentWorkArea(workAreaSequence[i]);

                if (_activityStore.EnableMove)
                {
                    activities.Add(actBuilder.BuildMoveActivity(sequenceID, i));
                }

                if (_activityStore.EnableNonCompaction)
                {
                    var act = actBuilder.BuildNonCompactionActivity(sequenceID, i);
                    activities.Add(act);
                    represent = act;
                }

                if (_activityStore.EnableCompaction)
                {
                    var act = actBuilder.BuildCompactionActivity(sequenceID, i);
                    activities.Add(act);
                    represent = act;
                }

                if (represent != null)
                {
                    representatives.Add(represent);
                }
            }

            var acts = activities.ToArray();
            _activityStore.ActivityGroups.Add(acts);
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

            // Output Activity
            for (int groupId = 0; groupId < _activityStore.ActivityGroups.Count; ++groupId)
            {
                var outputFolder = $"Group{groupId}";
                var outputDirectoryPath = Path.Combine(targetFolder, outputFolder);
                if (!Directory.Exists(outputDirectoryPath))
                {
                    Directory.CreateDirectory(outputDirectoryPath);
                }

                foreach (ActivityModel act in _activityStore.ActivityGroups[groupId].Where(act => act.GroupId == groupId))
                {
                    var fileName = $"{_activityStore.OutputFolderPrefix.CurrentValue}_{act.GroupId:d2}_{act.OutputFIlePrefix}.csv";
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
                }
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
                await ExportAreasAsync(_canvasItemStore.CompactionAreas, prefix, "法肩ライン", outputDirectoryPath);
                await ExportAreasAsync(_canvasItemStore.Holes, prefix, "法肩ライン障害物", outputDirectoryPath);
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
                var fileName = $"{filePrefix}_{fileSuffix}_{index:D2}.csv";
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
