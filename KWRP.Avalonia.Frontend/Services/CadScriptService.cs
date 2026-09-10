using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Backend.Model.Shapes.Activity;
using KWRP.Avalonia.Frontend.Models.Stores;
using KWRP.Backend.Enums;
using KWRP.Backend.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trdk.Geometry;

namespace KWRP.Avalonia.Frontend.Services
{
    public class CadScriptService
    {
        private readonly ILogService _logService;
        private readonly IPathService _pathService;
        private readonly ActivityStore _activityStore;
        private readonly ParameterStore _parameterStore;
        private readonly ILanguageService _languageService;

        public CadScriptService(
            ILogService logService,
            ActivityStore activityStore,
            IPathService pathService,
            ParameterStore parameterStore,
            ILanguageService languageService)
        {
            _logService = logService;
            _activityStore = activityStore;
            _pathService = pathService;
            _parameterStore = parameterStore;
            _languageService = languageService;
        }

        public async Task CreateCadScriptAsync(string path)
        {
            if (!_pathService.TryValidatePath(path, out string? reason))
            {
                throw new IOException(reason);
            }

            var sb = new StringBuilder();

            // 「#区割り結果」という名前のレイヤを作成。現在のレイヤを「#区割り結果」にする
            sb.AppendLine(
                $"-LAYER\r\n" +
                $"n {_languageService.GetString("Domain.Front.DivisionResults")}\r\n" +
                $"s  {_languageService.GetString("Domain.Front.DivisionResults")} \r\n\n");

            var acts = _activityStore.ActivityGroups.SelectMany(group => group).Where(act => act.ActivityType == Backend.Enums.RollerActivityType.Compaction);
            if (!acts.Any())
            {
                acts = _activityStore.ActivityGroups.SelectMany(group => group).Where(act => act.ActivityType == Backend.Enums.RollerActivityType.NonCompaction);
            }

            foreach (var act in acts)
            {
                sb.AppendLine(CreateActivityCadScript(act, GetLengthScale()));
            }


            try
            {
                await using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
                await using var writer = new StreamWriter(fs, Encoding.UTF8);
                await writer.WriteAsync(sb.ToString());
            }
            catch (Exception ex)
            {
                _logService.LogError(_languageService.GetString("Domain.Front.CadScriptSaveFailed"), ex);
                throw;
            }
        }

        private double GetLengthScale()
            => _parameterStore.CurrentLengthUnitType.Value == LengthUnitType.Millimeter ? 1000.0 : 1.0;

        private static string CreateActivityCadScript(ActivityModel activity, double lengthScale)
        {
            if (activity.OccArea == null || activity.GoalArea == null || activity.WorkArea == null)
                throw new NullReferenceException("エリア設定ができていません");

            var sb = new StringBuilder();

            double lineWidth = 0.2;
            sb.AppendLine("PLINE");
            for (int i = 0; i < activity.WorkArea.Shape.Points.Count; i++)
            {
                var point = activity.WorkArea.Shape.Points[i];
                sb.AppendLine($"{point.X * lengthScale:F5},{point.Y * lengthScale:F5}");
                if (i == 0)
                    sb.AppendLine($"w {lineWidth} {lineWidth}");
            }
            sb.AppendLine("c");

            var c = activity.WorkArea.Shape.Centroid;

            var seg = Seg2.Create(c, 3.0, activity.Dir);
            var from = seg.Src;
            var to = from + new Vec2(activity.Dir) * 3;
            sb.AppendLine("PLINE");
            sb.AppendLine($"{from.X * lengthScale:F6},{from.Y * lengthScale:F6}");
            sb.AppendLine($"w {1.85 * lengthScale} 0");
            sb.AppendLine($"{to.X * lengthScale:F6},{to.Y * lengthScale:F6}");
            sb.AppendLine();

            const double width = 3.0;
            var mTextCorner = Seg2.Create(c, 3.1, activity.Dir).Src + new Vec2(activity.Dir + Math.PI * 0.5) * (width * 0.5);
            sb.AppendLine("MTEXT");
            sb.AppendLine($"{mTextCorner.X * lengthScale:F6},{mTextCorner.Y * lengthScale:F6}");
            sb.AppendLine($"r {activity.Dir * 180 / Math.PI - 90:0.00}");
            sb.AppendLine($"h {0.71 * lengthScale}");
            sb.AppendLine($"w {width * lengthScale:0.0}");
            sb.AppendLine($"{activity.GroupId}-{activity.AreaID}");
            sb.AppendLine($"L{activity.Length * lengthScale:0.0}");
            sb.AppendLine($"W{activity.Width * lengthScale:0.0}");

            return sb.ToString();
        }
    }
}
