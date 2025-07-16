using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Frontend.Models.Settings;
using KWRP.Avalonia.Frontend.Models.Stores;
using R3;
using System;
using System.IO;

namespace KWRP.Avalonia.Frontend.ViewModels.StartUpPage
{
    public class DxfPageItemViewModel : ViewModelBase
    {
        private readonly ApplicationStore _applicationStore;
        private readonly ConfigPathInfo _configPathInfo;
        private readonly IPathService _pathService;
        private readonly ILogService _logService;

        public DxfPageItemViewModel(ApplicationStore applicationStore, ConfigPathInfo configPathInfo, IPathService pathService, ILogService logService)
        {
            _applicationStore = applicationStore;
            _configPathInfo = configPathInfo;
            _pathService = pathService;
            _logService = logService;


            CenterX = new BindableReactiveProperty<long?>().AddTo(Disposables);
            CenterY = new BindableReactiveProperty<long?>().AddTo(Disposables);
            MapWidth = new BindableReactiveProperty<long?>().AddTo(Disposables);
            MapHeight = new BindableReactiveProperty<long?>().AddTo(Disposables);
            Ratio = new BindableReactiveProperty<int>(5).AddTo(Disposables);
            DxfPath = new BindableReactiveProperty<string?>().AddTo(Disposables);
            PngPath = DxfPath
                .Where(s => !string.IsNullOrEmpty(s))
                .Select(s =>
                {
                    var fileName = Path.GetFileName(s);
                    return Path.ChangeExtension(fileName, "png");
                })
                .ToReadOnlyBindableReactiveProperty()
                .AddTo(Disposables);

            SetDxfPathCommand = new ReactiveCommand(async _ =>
            {
                try
                {
                    _applicationStore.IsBusy.Value = true;
                    DxfPath.Value = await _pathService.GetOpenFilePathAsync(Backend.Enums.FileType.DXF, "DXFファイルを選択してください");
                    if (DxfPath.Value != null)
                    {
                        _logService.LogInfo($"dxfファイルを選択しました : {DxfPath.Value}");
                    }
                }
                catch (Exception e)
                {
                    _logService.LogError("dxfファイル選択時エラー : " + e.Message, e);
                }
                finally
                {
                    _applicationStore.IsBusy.Value = false;
                }
            }).AddTo(Disposables);

            RenderCommand = Observable.CombineLatest(
                CenterX, CenterY, MapWidth, MapHeight, Ratio, DxfPath,
                (cx, cy, w, h, r, path) =>
                {
                    return cx != null && cy != null && w != null && h != null
                        && w > 0 && h > 0 && r > 0 && path != null;
                })
                .ToReactiveCommand(_ =>
                {
                })
                .AddTo(Disposables);
        }


        public BindableReactiveProperty<long?> CenterX { get; }
        public BindableReactiveProperty<long?> CenterY { get; }
        public BindableReactiveProperty<long?> MapWidth { get; }
        public BindableReactiveProperty<long?> MapHeight { get; }
        public BindableReactiveProperty<int> Ratio { get; }
        public BindableReactiveProperty<string?> DxfPath { get; }
        public IReadOnlyBindableReactiveProperty<string?> PngPath { get; }

        public ReactiveCommand SetDxfPathCommand { get; }
        public ReactiveCommand RenderCommand { get; }
    }
}
