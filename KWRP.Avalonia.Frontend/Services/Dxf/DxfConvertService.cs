using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Frontend.Models.Settings;
using KWRP.Avalonia.Frontend.Models.Stores;
using KWRP.Avalonia.Frontend.ViewModels.StartUpPage;
using KWRP.Avalonia.NetDxf;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Trdk.Geometry;

namespace KWRP.Avalonia.Frontend.Services.Dxf
{
    public class DxfConvertService
    {
        private readonly ConfigPathInfo _configPathInfo;
        private readonly ILogService _logService;
        private readonly DxfStore _dxfStore;

        public DxfConvertService(
            ConfigPathInfo configPathInfo,
            ILogService logService,
            DxfStore dxfStore)
        {
            _configPathInfo = configPathInfo;
            _logService = logService;
            _dxfStore = dxfStore;
        }

        public void AddOption()
        {
            var titles = _dxfStore
                    .Options
                    .Where(op => op.Title != null)
                    .Select(op => op.Title)
                    .Where(title => title!.StartsWith("背景"))
                    .ToHashSet();

            string title = $"背景";
            for (int i = 0; i <= titles.Count; ++i)
            {
                title = $"背景{i + 1:D2}";
                if (!titles.Contains(title)) break;
            }

            _dxfStore.SelectedOption.Value = new DxfConverterOptionViewModel
            {
                Title = title,
                CenterX = 0,
                CenterY = 0,
                MapWidth = 100,
                MapHeight = 100,
                PixelsPerMeter = 5,
            };
        }

        public void DeleteOption(DxfConverterOptionViewModel option)
        {
            _dxfStore.Options.Remove(option);
            _dxfStore.SelectedOption.Value = _dxfStore.Options.Count > 0 ? _dxfStore.Options[0] : null;
        }

        public string GetSavePngPath(DxfConverterOption option)
        {
            if (option.DxfFilePath == null)
                throw new NullReferenceException("dxfファイルが未登録");

            var fileName = Path.GetFileName(option.DxfFilePath);
            var pngFileName = option.Title + "_" + Path.ChangeExtension(fileName, "png");
            return Path.Combine(_configPathInfo.AssetsDirectoryPath, pngFileName);
        }

        public async Task<string?> ExportPngAsync(DxfConverterOption option)
        {
            try
            {
                var pngPath = GetSavePngPath(option);
                
                await new DxfConverter()
                    .SetOption(option)
                    .SetLogger(_logService)
                    .Load(option.DxfFilePath!)
                    .ConvertTo(pngPath)
                    .ConvertAsync();


                return pngPath;
            }
            catch(Exception e)
            {
                _logService.LogError($"背景図作成時にエラーが発生しました: {e.Message}", e);
                throw;
            }
        }

    }
}
