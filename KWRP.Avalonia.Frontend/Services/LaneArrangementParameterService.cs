using KWRP.Avalonia.Backend;
using KWRP.Avalonia.Backend.Model.Paramter;
using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Frontend.Models.Stores;
using KWRP.Infra.JSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWRP.Avalonia.Frontend.Services
{
    public class LaneArrangementParameterService : ILaneArrangementParameterService
    {
        private readonly IPathService _pathService;
        private readonly ILogService _logService;
        private readonly ParameterStore _parameterStore;
        private readonly MachineStore _machineStore;

        public LaneArrangementParameterService(IPathService pathService, ILogService logService, ParameterStore parameterStore, MachineStore machineStore)
        {
            _pathService = pathService;
            _logService = logService;
            _parameterStore = parameterStore;
            _machineStore = machineStore;

            _logService.LogDebug("init");
        }

        public async Task<bool> LoadParamsAsync()
        {
            var filePath = await _pathService.GetOpenFilePathAsync(Backend.Enums.FileType.JSON);

            if (string.IsNullOrEmpty(filePath))
            {
                _logService.LogInfo("パラメータ読み込み:ファイル選択を中止しました");
                return false;
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"指定されたファイルが存在しません: {filePath}");
            }

            try
            {
                _parameterStore.CanArrangeLane.Value = false;

                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                { }

                _logService.LogDebug($"ファイル「{filePath}」を選択しました");

                {
                    var param = await new JsonSerializerService<LaneArrangementParameter>().LoadAsync(filePath);
                    if (param == null || param.VrParams == null || param.PairingParams == null) 
                        throw new ArgumentNullException($"ファイルのフォーマットが正しくありません");


                    _parameterStore.Reload();

                    _parameterStore.PairCountMax = int.MaxValue;
                    _parameterStore.PairCountMin = 0;
                    _parameterStore.PairCountMin = param.PairingParams.CountMin;
                    _parameterStore.PairCountMax = param.PairingParams.CountMax;

                    //_parameterStore.LaneWidth = param.VrParams.LaneWidth;
                    //_parameterStore.FrontAllowance = param.VrParams.FrontAllowance;
                    //_parameterStore.RearAllowance = param.VrParams.RearAllowance;
                    //_parameterStore.SidePrevAllowance = param.VrParams.Offset;
                    //_parameterStore.SideNextAllowance = param.VrParams.SideAllowance;
                    //_parameterStore.PerimeterAllowance = param.VrParams.SideAllowance;
                    //_parameterStore.LaneChangeLength = param.VrParams.LaneChangeLength;       // ローラに依存するパラメータは更新しない
                    
                    _parameterStore.RollerHeadType = param.VrParams.IsRollerHeadingRight ? Backend.Enums.RollerHeadingType.ToRight : Backend.Enums.RollerHeadingType.ToLeft;
                    _parameterStore.FrontOffset = param.VrParams.FrontAllowance - _machineStore.CurrentRoller.FrontAllowance;
                    _parameterStore.RearOffset = param.VrParams.RearAllowance - _machineStore.CurrentRoller.RearAllowance;
                    _parameterStore.SidePrevOffset.Value = param.VrParams.Offset;
                    _parameterStore.SideNextOffset.Value = param.VrParams.SideAllowance;
                    _parameterStore.ProgresssDirectionRadian.Value = param.VrParams.LaneProgressDirection * Math.PI / 180.0;
                    _parameterStore.LapWidth = param.VrParams.LapWidth;

                    // 新しいパラメータで再度レーン割計算
                    _parameterStore.CanArrangeLane.Value = true;
                    _parameterStore.PublishParameterFileLoaded();
                }
                
                return true;
            }
            catch (IOException ex)
            {
                throw new Exception($"ファイルが開かれています\n{ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"ファイル{filePath}読み込みに失敗しました\n{ex.Message}", ex);
            }
            finally
            {
                _parameterStore.CanArrangeLane.Value = true;
            }
        }

        public async Task<string?> SaveParamsAsync()
        {
            var filePath = await _pathService.GetSaveFilePathAsync(Backend.Enums.FileType.JSON);

            if (string.IsNullOrEmpty(filePath))
            {
                _logService.LogInfo("パラメータ読み込み:ファイル選択を中止しました");
                return null;
            }

            if (!_pathService.TryValidatePath(filePath, out string? reason))
            {
                throw new IOException(reason);
            }

            var vrparam = new VRParameterModel
            {
                LaneWidth = _parameterStore.LaneWidth,
                FrontAllowance = _parameterStore.FrontAllowance + _parameterStore.FrontOffset,
                RearAllowance = _parameterStore.RearAllowance + _parameterStore.RearOffset,
                SideAllowance = _parameterStore.SideNextOffset.Value,
                Offset = _parameterStore.SidePrevOffset.Value,
                LapWidth = _parameterStore.LapWidth,
                LaneChangeLength = _parameterStore.LaneChangeLength,
                LaneProgressDirection = _parameterStore.ProgresssDirectionRadian.Value * 180 / Math.PI,
                IsRollerHeadingRight = _parameterStore.RollerHeadType == Backend.Enums.RollerHeadingType.ToRight,
            };

            var pair = new PairingParameterModel
            {
                CountMax = _parameterStore.PairCountMax,
                CountMin = _parameterStore.PairCountMin,
            };

            var param = new LaneArrangementParameter { PairingParams = pair, VrParams = vrparam, Description = $"{DateTime.Now:yyyyMMdd}作成"};

            try
            {
                await new JsonSerializerService<LaneArrangementParameter>().SaveAsync(param, filePath);
                _logService.LogInfo($"パラメータを{filePath}に保存しました");
                return filePath;
            }
            catch (Exception ex)
            {
                throw new Exception("パラメータの保存に失敗しました" + ex.Message, ex);
            }
        }
    }
}
