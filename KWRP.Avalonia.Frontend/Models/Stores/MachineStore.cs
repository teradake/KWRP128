using KWRP.Avalonia.Backend.Models.Roller;
using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Frontend.Models.Settings;
using KWRP.Infra;
using System;

namespace KWRP.Avalonia.Frontend.Models.Stores
{
    public class MachineStore
    {
        private readonly ILogService _logService;
        private readonly ConfigPathInfo _configPathInfo;

        public MachineStore(
            ConfigPathInfo configPathInfo, 
            ILogService logService)
        {
            _configPathInfo = configPathInfo;
            _logService = logService;

            try
            {
                var serializer = SerializerFactory.Create<RollerModel>(Backend.Enums.FileType.JSON);
                _current = serializer.Load(_configPathInfo.MachineConfigPath);
                _logService.LogInfo($"重機設定ファイル（{_configPathInfo.MachineConfigPath}）を読込みました");
            }
            catch
            {
                _logService.LogInfo($"重機設定ファイル（{_configPathInfo.MachineConfigPath}）が見つかりませんでした。デフォルト値を使用します");
            }
            finally
            {
                if (_current == null)
                {
                    _current = RollerModel.CreateDefault();
                    _original = null;
                }
                else
                {
                    _original = _current.Clone();
                }
            }
        }

        private RollerModel? _original;
        private RollerModel? _current;
        public RollerModel CurrentRoller => _current ??= RollerModel.CreateDefault();
        public string MachineConfigPath => _configPathInfo.MachineConfigPath;

        public bool IsPropertyChanged => _current != null && !_current.Equals(_original);
    }
}
