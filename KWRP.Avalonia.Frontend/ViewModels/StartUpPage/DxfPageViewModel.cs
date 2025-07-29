using Avalonia.Media.Imaging;
using KWRP.Avalonia.Backend.Model;
using KWRP.Avalonia.Backend.Models;
using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Frontend.Models.Stores;
using KWRP.Avalonia.Frontend.Services.Dxf;
using KWRP.Avalonia.NetDxf;
using MsBox.Avalonia;
using ObservableCollections;
using R3;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KWRP.Avalonia.Frontend.ViewModels.StartUpPage
{
    public class DxfPageViewModel : ViewModelBase
    {
        private readonly IPathService _pathService;
        private readonly ILogService _logService;
        private readonly INotificationService _notificationService;
        private readonly ApplicationStore _applicationStore;
        private readonly DxfConvertService _convertService;
        private readonly DxfStore _dxfStore;

        public DxfPageViewModel(
            ApplicationStore applicationStore,
            IPathService pathService,
            ILogService logService,
            INotificationService notificationService,
            DxfConvertService convertService,
            DxfStore dxfStore)
        {
            _applicationStore = applicationStore;
            _pathService = pathService;
            _logService = logService;
            _notificationService = notificationService;
            _convertService = convertService;
            _dxfStore = dxfStore;

            Options = _dxfStore
                .Options
                .CreateView(op => op)
                .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current)
                .AddTo(Disposables);

            SelectedOption = _dxfStore
                .SelectedOption
                .ToBindableReactiveProperty(_dxfStore.SelectedOption.Value)
                .AddTo(Disposables);

            SelectedOption
                .Subscribe(v => _dxfStore.SelectedOption.Value = v)
                .AddTo(Disposables);

            AddOptionCommand = new ReactiveCommand(_ => _convertService.AddOption())
                .AddTo(Disposables);

            RemoveOptionCommand = SelectedOption
                .Select(op => op != null)
                .ToReactiveCommand(_ => _convertService.DeleteOption(SelectedOption.Value!))
                .AddTo(Disposables);

            PngPath = SelectedOption
                .Select(op =>
                {
                    if (op == null || op.DxfFilePath == null) return null;
                    return _convertService.GetSavePngPath(op.ToModel());
                })
                .ToBindableReactiveProperty()
                .AddTo(Disposables);

            Thumnail = PngPath
                .ThrottleLast(TimeSpan.FromMilliseconds(100))
                .SelectAwait(async (png, ct) =>
                {
                    if (string.IsNullOrEmpty(png) || !File.Exists(png))
                        return null;

                    await using var imageStream = File.OpenRead(png);
                    return await Task.Run(() => Bitmap.DecodeToWidth(imageStream, 400));
                })
                .ToBindableReactiveProperty(null) 
                .AddTo(Disposables);

            RunConversionCommand = SelectedOption
                .Select(op => op != null)
                .ToReactiveCommand(async _ =>
                {
                    try
                    {
                        _applicationStore.IsBusy.Value = true;
                        var option = SelectedOption.Value!.ToModel();

                        string? savedPngPath = await _convertService.ExportPngAsync(option);

                        if (!_dxfStore.Options.Contains(SelectedOption.Value))
                        {
                            _dxfStore.Options.Add(SelectedOption.Value);
                            _dxfStore.SelectedOption.Value = null;
                            _dxfStore.SelectedOption.Value = _dxfStore.Options[_dxfStore.Options.Count - 1];
                        }
                        else
                        {
                            var item = _dxfStore.SelectedOption.Value;
                            _dxfStore.SelectedOption.Value = null;
                            _dxfStore.SelectedOption.Value = item;
                        }

                        _notificationService.Notify(KWRPNotification.Create("背景図を作成しました", Backend.Enums.NotifyMessageType.Info, 5));
                        _logService.LogInfo($"背景図を作成しました:{savedPngPath}");
                    }
                    catch (Exception ex)
                    {
                        _notificationService.Notify(KWRPNotification.Create($"背景図作成に失敗しました\n{ex.Message}", Backend.Enums.NotifyMessageType.Warn));
                        _logService.LogError($"背景図作成に失敗しました", ex);
                    }
                    finally
                    {
                        _applicationStore.IsBusy.Value = false;
                    }
                })
                .AddTo(Disposables);

            SetDxfPathCommand = SelectedOption
                .Select(op => op != null)
                .ToReactiveCommand(async _ =>
                {
                    try
                    {
                        _applicationStore.IsBusy.Value = true;
                        var path = await _pathService.GetOpenFilePathAsync(Backend.Enums.FileType.DXF, "DXFファイルを選択してください");
                        if (path != null && SelectedOption.Value != null)
                        {
                            SelectedOption.Value.DxfFilePath = path;
                            _logService.LogInfo($"dxfファイルを選択 : {path}");
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
                })
                .AddTo(Disposables);

            _logService.SetStatusMessage("背景設定UI : 背景画像を登録・選択することができます");
            _logService.LogDebug("init");
        }

        public override void Dispose()
        {
            if (SelectedOption.Value != null && !_dxfStore.Options.Contains(SelectedOption.Value))
            {
                _dxfStore.SelectedOption.Value = null;
            }
            base.Dispose();
        }


        public INotifyCollectionChangedSynchronizedViewList<DxfConverterOptionViewModel> Options { get; }

        public BindableReactiveProperty<DxfConverterOptionViewModel?> SelectedOption { get; }
        public BindableReactiveProperty<string?> PngPath { get; }
        public BindableReactiveProperty<Bitmap?> Thumnail { get; }
        
        public ReactiveCommand SetDxfPathCommand { get; }
        public ReactiveCommand RunConversionCommand { get; }
        public ReactiveCommand AddOptionCommand { get; }
        public ReactiveCommand RemoveOptionCommand { get; }
    }
}
