using Avalonia.Logging;
using KWRP.Avalonia.Backend.Services;
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

        public DxfConverter SetLogger(ILogService logger)
        {
            _logger = logger;
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

            //_dxfdoc = DxfDocument.Load(_dxfFilePath);
            _dxfdoc = await Task.Run(() => DxfDocument.Load(_dxfFilePath));
            if (_dxfdoc == null)
            {
                throw new Exception("dxfファイルの読込みに失敗しました");
            }

            try
            {
                var bitmap = await AvaloniaDxfRenderer.RenderAsync(_dxfdoc, _option!, _logger);
                await using var fs = new FileStream(_pngFilePath!, FileMode.Create);
                bitmap.Save(fs);

                _logger?.LogInfo($"背景図を生成しました。");
            }
            catch (Exception e)
            {
                throw new Exception($"Pngファイル生成に失敗しました + {e.Message}", e);
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
                reason = "DXFファイルパスが未入力です";
                return false;
            }

            if (string.IsNullOrEmpty(_pngFilePath))
            {
                reason = "Pngファイルパスが未入力です";
                return false;
            }

            if (_option == null)
            {
                reason = "設定情報が未入力です";
                return false;
            }

            if (_option.MapWidth <= 0 || _option.MapHeight <= 0)
            {
                reason = "切り取り範囲は1以上の値を指定してください";
                return false;
            }

            if (_option.PixelsPerMeter <= 0)
            {
                reason = "解像度は1以上の値を指定してください";
                return false;
            }

            if (string.IsNullOrEmpty(_option.Title))
            {
                reason = "タイトルが未入力です";
                return false;
            }

            return true;
        }
    }
}
