using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Frontend.Models.Settings;
using KWRP.Avalonia.Frontend.Models.Stores;
using KWRP.Avalonia.Frontend.ViewModels.StartUpPage;
using KWRP.Avalonia.NetDxf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace KWRP.Avalonia.Frontend.Services.Dxf
{
    public class DxfHistoryStorageService
    {
        private readonly ConfigPathInfo _configPathInfo;
        private readonly ILogService _logService;
        private readonly DxfStore _dxfStore;

        public DxfHistoryStorageService(
            ConfigPathInfo configPathInfo,
            ILogService logService,
            DxfStore dxfStore)
        {
            _configPathInfo = configPathInfo;
            _logService = logService;
            _dxfStore = dxfStore;
        }

        public async Task SaveAsync()
        {
            var viewModels = _dxfStore.Options.ToList();
            var selectedOption = _dxfStore.SelectedOption.Value;

            if (viewModels.Count == 0) 
                return;

            try
            {
                var models = viewModels.Select(vm => vm.ToModel()).ToList();
                var history = new DxfHistoryFile
                {
                    SelectedId = selectedOption?.Id ?? Guid.NewGuid(),
                    Items = models,
                };

                var option = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    WriteIndented = true,
                };
                var json = JsonSerializer.Serialize(history, option);
                File.WriteAllText(_configPathInfo.DxfHistoryPath, json);
                _logService.LogInfo("dxf履歴を保存しました");
            }
            catch (Exception e)
            {
                _logService.LogError("dxf履歴の保存に失敗しました", e);
            }
        }

        public async Task<(List<DxfConverterOptionViewModel>, Guid?)> LoadAsync()
        {
            if (!File.Exists(_configPathInfo.DxfHistoryPath))
            {
                return (new List<DxfConverterOptionViewModel>(), null);
            }

            try
            {
                var json = await File.ReadAllTextAsync(_configPathInfo.DxfHistoryPath);
                var history = JsonSerializer.Deserialize<DxfHistoryFile>(json);

                var viewModels = history?
                    .Items?
                    .Select(DxfConverterOptionViewModel.FromModel)
                    .ToList() ?? [];
                var selectedId = history?.SelectedId ?? null;

                _dxfStore.Options.Clear();
                _dxfStore.Options.AddRange(viewModels);
                _dxfStore.SelectedOption.Value = _dxfStore.Options.FirstOrDefault(x => x.Id == selectedId) ?? null;

                _logService.LogInfo("dxf履歴をロードしました");
                return (viewModels, selectedId);
            }
            catch (Exception e)
            {
                _logService.LogError("dxf履歴のロードに失敗しました", e);
                return (new List<DxfConverterOptionViewModel>(), null);
            }
        }
    }
}
