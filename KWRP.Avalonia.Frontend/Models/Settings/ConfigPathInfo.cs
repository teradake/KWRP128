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
        private readonly string _settingDirName;
        private readonly string _assetsDirName;

        public ConfigPathInfo(string? basePath = null)
        {
            _basePath = basePath ?? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            if (_basePath == null)
            {
                string exeFullPath = Path.GetFullPath(Environment.GetCommandLineArgs()[0]);
                _basePath = Path.GetDirectoryName(exeFullPath);
            }
        
            _settingDirName = "settings";
            _assetsDirName = "assets";

            if (!Directory.Exists(SettingDirectoryPath)) Directory.CreateDirectory(SettingDirectoryPath);
            if (!Directory.Exists(AssetsDirectoryPath)) Directory.CreateDirectory(AssetsDirectoryPath);
        }

        public string MachineConfigPath => Path.Combine(SettingDirectoryPath, "MachineInfo.json");
        public string LaneArrangementConfigPath => Path.Combine(SettingDirectoryPath, "LaneArrangementInfo.json");
        public string DxfHistoryPath => Path.Combine(AssetsDirectoryPath, "dxf_converter_history.json");

        public string AssetsDirectoryPath => Path.Combine(_basePath, _assetsDirName);
        public string SettingDirectoryPath => Path.Combine(_basePath, _settingDirName);
    }
}
