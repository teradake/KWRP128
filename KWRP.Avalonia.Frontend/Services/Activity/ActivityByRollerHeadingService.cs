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
    public class ActivityByRollerHeadingService : ActivityService
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

        public ActivityByRollerHeadingService(
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
            : base(activityStore, logService, notificationService, workAreaStore, machineStore, pathService, captureService, cadScriptService, canvasItemStore, parameterStore)
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

        public override void CreateActivityGroups()
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
    }
}
