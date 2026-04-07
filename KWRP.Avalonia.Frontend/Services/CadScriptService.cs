using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Frontend.Models.Stores;
using KWRP.Backend.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWRP.Avalonia.Frontend.Services
{
    public class CadScriptService
    {
        private readonly ILogService _logService;
        private readonly IPathService _pathService;
        private readonly ActivityStore _activityStore;
        private readonly ILanguageService _languageService;

        public CadScriptService(
            ILogService logService,
            ActivityStore activityStore,
            IPathService pathService,
            ILanguageService languageService)
        {
            _logService = logService;
            _activityStore = activityStore;
            _pathService = pathService;
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
                sb.AppendLine(act.ToCadScript());
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
    }
}
