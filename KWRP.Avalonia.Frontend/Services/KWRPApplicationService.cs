
using KWRP.Avalonia.Backend.Models.Roller;
using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Frontend.Models.Settings;
using KWRP.Avalonia.Frontend.Models.Stores;
using KWRP.Infra.JSON;
using System;
using System.IO;
using System.Threading.Tasks;

namespace KWRP.Avalonia.Frontend.Services
{
    public class KWRPApplicationService : IKWRPApplicationService
    {
        private readonly MachineStore _machineStore;
        private readonly ILogService _logService;
        private readonly ConfigPathInfo _configPathInfo;
        private readonly ApplicationStore _applicationStore;

        public KWRPApplicationService(
            MachineStore machineStore,
            ILogService logService,
            ConfigPathInfo configPathInfo,
            ApplicationStore applicationStore)
        {
            _machineStore = machineStore;
            _logService = logService;
            _configPathInfo = configPathInfo;

            _logService.LogDebug("init");
            _applicationStore = applicationStore;
        }

        public async Task SaveMachineConfigAsync()
        {
            try
            {
                var savePath = _configPathInfo.MachineConfigPath;
                var dir = Path.GetDirectoryName(savePath) ?? throw new Exception("ディレクトリパスの取得に失敗しました");
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var serializer = new JsonSerializerService<RollerModel>();
                await serializer.SaveAsync(_machineStore.CurrentRoller, savePath);
                _logService.LogInfo($"重機情報を保存しました: {savePath}");
            }
            catch (Exception e)
            {
                _logService.LogError("重機情報の保存に失敗しました", e);
            }
        }

        public async Task SaveLaneArrangementConfigAsync()
        {
            try
            {
                var savePath = _configPathInfo.LaneArrangementConfigPath;
                var dir = Path.GetDirectoryName(savePath) ?? throw new Exception("ディレクトリパスの取得に失敗しました");
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var serializer = new JsonSerializerService<LaneArrangementConfigs>();
                await serializer.SaveAsync(_applicationStore.LaneArrangementConfigs, savePath);
                _logService.LogInfo($"レーン割設定を保存しました: {savePath}");
            }
            catch (Exception e)
            {
                _logService.LogError("レーン割設定ファイルの保存に失敗しました", e);
            }
        }
    }
}
