using Avalonia.Logging;
using KWRP.Avalonia.Backend.Services;
using KWRP.Backend.Services;
using netDxf;

namespace KWRP.Avalonia.NetDxf
{
    public class DxfConverter
    {
        private DxfDocument? _dxfdoc;
        private DxfConverterOption? _option;
        private string? _dxfFilePath;
        private string? _pngFilePath;
        private ILogService? _logger;
        private ILanguageService? _languageService;

        public DxfConverter SetLogger(ILogService logger)
        {
            _logger = logger;
            return this;
        }

        public DxfConverter SetLang(ILanguageService lang)
        {
            _languageService = lang;
            return this;
        }

        public DxfConverter Load(string dxfFilePath)
        {
            _dxfFilePath = dxfFilePath;
            return this;
        }

        public DxfConverter ConvertTo(string pngFilePath)
        {
            _pngFilePath = pngFilePath;
            return this;
        }

        public DxfConverter SetOption(DxfConverterOption option)
        {
            _option = option;
            return this;
        }

        public async Task ConvertAsync()
        {
            if (!TryValidate(out string? reason))
            {
                throw new InvalidOperationException(reason);
            }

            _dxfdoc = await Task.Run(() => DxfDocument.Load(_dxfFilePath));
            if (_dxfdoc == null)
            {
                throw new Exception(_languageService?.GetString("Domain.Dxf.LoadFailed") ?? "dxfファイルの読込みに失敗しました");
            }
            _option!.DxfFilePath = _dxfFilePath;

            try
            {
                var bitmap = await AvaloniaDxfRenderer.RenderAsync(_dxfdoc, _option!, _logger, _languageService);
                await using var fs = new FileStream(_pngFilePath!, FileMode.Create);
                bitmap.Save(fs);

                _logger?.LogInfo(_languageService?.GetString("Domain.Dxf.BackgroundGenerated") ?? $"背景図を生成しました。");
            }
            catch (Exception e)
            {
                throw new Exception(string.Format(
                    (_languageService?.GetString("Domain.Dxf.PngCreationFailed") ?? "Pngファイル生成に失敗しました {0}"), e));
            }
            finally
            {
                _dxfdoc = null;
            }
        }

        private bool TryValidate(out string? reason)
        {
            reason = null;
            if (string.IsNullOrEmpty(_dxfFilePath))
            {
                reason = _languageService?.GetString("Domain.Dxf.NoDxfFilePath") ?? "DXFファイルパスが未入力です";
                return false;
            }

            if (string.IsNullOrEmpty(_pngFilePath))
            {
                reason = _languageService?.GetString("Domain.Dxf.NoPngFilePath") ?? "Pngファイルパスが未入力です";
                return false;
            }

            if (_option == null)
            {
                reason = _languageService?.GetString("Domain.Dxf.NoSettingsInfo") ?? "設定情報が未入力です";
                return false;
            }

            if (_option.MapWidth <= 0 || _option.MapHeight <= 0)
            {
                reason = _languageService?.GetString("Domain.Dxf.CropRangeAboveOne") ?? "切り取り範囲は1以上の値を指定してください";
                return false;
            }

            if (_option.PixelsPerMeter <= 0)
            {
                reason = _languageService?.GetString("Domain.Dxf.ResolutionAboveOne") ?? "解像度は1以上の値を指定してください";
                return false;
            }

            if (string.IsNullOrEmpty(_option.Title))
            {
                reason = _languageService?.GetString("Domain.Dxf.NoTitle") ?? "タイトルが未入力です";
                return false;
            }

            return true;
        }
    }
}