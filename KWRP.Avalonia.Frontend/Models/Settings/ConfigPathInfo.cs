using KWRP.Avalonia.Backend.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KWRP.Avalonia.Frontend.Models.Settings
{
    public class ConfigPathInfo
    {
        private readonly string _basePath;
        private readonly string _settingDirName = "settings";
        private readonly string _assetsDirName = "assets";

        public ConfigPathInfo(string? basePath = null)
        {
            if (string.IsNullOrEmpty(basePath))
            {
                basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (string.IsNullOrEmpty(basePath))
                {
                    string exeFullPath = Path.GetFullPath(Environment.GetCommandLineArgs()[0]);
                    basePath = Path.GetDirectoryName(exeFullPath);
                }
            }
            _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath), "実行ファイルのフォルダパスが取得できませんでした");

            CreateDirectoryIfNotExists(SettingDirectoryPath);
            CreateDirectoryIfNotExists(AssetsDirectoryPath);
        }

        public string MachineConfigPath => Path.Combine(SettingDirectoryPath, "MachineInfo.json");
        public string LaneArrangementConfigPath => Path.Combine(SettingDirectoryPath, "LaneArrangementInfo.json");
        public string DxfHistoryPath => Path.Combine(AssetsDirectoryPath, "dxf_converter_history.json");

        public string AssetsDirectoryPath => Path.Combine(_basePath, _assetsDirName);
        public string SettingDirectoryPath => Path.Combine(_basePath, _settingDirName);

        private void CreateDirectoryIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
