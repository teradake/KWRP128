using KWRP.Avalonia.Backend.Enums;
using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Frontend.Models.Settings;
using KWRP.Infra;
using R3;
using System;
using Trdk.Geometry.NTS;

namespace KWRP.Avalonia.Frontend.Models.Stores
{
    public class ApplicationStore
    {
        private readonly ILogService _logService;
        private readonly ConfigPathInfo _configPathInfo;

        public ApplicationStore(ILogService logService, ConfigPathInfo configPathInfo)
        {
            _logService = logService;
            _configPathInfo = configPathInfo;

            IsBusy = new ReactiveProperty<bool>(false);
            SpatialDataPath = new ReactiveProperty<string>(string.Empty);

            try
            {
                var serializer = SerializerFactory.Create<LaneArrangementConfigs>(Backend.Enums.FileType.JSON);
                var item = serializer.Load(_configPathInfo.LaneArrangementConfigPath);
                LaneArrangementConfigs = item;
                _logService.LogInfo($"レーン割設定ファイル（{_configPathInfo.LaneArrangementConfigPath}）を読込みました");
            }
            catch (Exception e)
            {
                _logService.LogInfo($"レーン割設定ファイル（{_configPathInfo.LaneArrangementConfigPath}）が見つかりませんでした。デフォルト値を使用します\n" + e.Message);
            }
            finally
            {
                if (LaneArrangementConfigs == null)
                {
                    LaneArrangementConfigs = new LaneArrangementConfigs();
                }
            }

            _logService.LogDebug("init");
        }

        public ReactiveProperty<bool> IsBusy { get; }
        public ReactiveProperty<string> SpatialDataPath { get; }
        public ReactiveProperty<EditorMode> EditorMode { get; } = new(Backend.Enums.EditorMode.None);


        private LaneArrangementConfigs _laneArrangementConfigs;
        public LaneArrangementConfigs LaneArrangementConfigs
        {
            get => _laneArrangementConfigs;
            set
            {
                _laneArrangementConfigs = value;
                PolygonMergerOptions.VertexSimplificationDistance = _laneArrangementConfigs.VertexSimplificationDistance;
            }
        }
    }
}
