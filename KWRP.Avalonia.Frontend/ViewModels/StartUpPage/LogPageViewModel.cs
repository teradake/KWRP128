using KWRP.Avalonia.Backend.Model;
using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Frontend.Models.Stores;
using MsBox.Avalonia;
using ObservableCollections;
using R3;
using System;
using System.Threading.Tasks;

namespace KWRP.Avalonia.Frontend.ViewModels.StartUpPage
{
    public class LogPageViewModel : ViewModelBase
    {
        private readonly LogStore _logStore;
        private readonly ILogService _logService;

        private readonly ISynchronizedView<KWRPLog, KWRPLog> _synchronizedView;
        public NotifyCollectionChangedSynchronizedViewList<KWRPLog> Logs { get; }

        public LogPageViewModel(LogStore logStore, ILogService logService)
        {
            _logStore = logStore;
            _logService = logService;

            _synchronizedView = _logStore.KWRPLogs.CreateView(log => log);

            Logs = _synchronizedView
                .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current)
                .AddTo(Disposables);

            LogDebugCommand = new ReactiveCommand(_ => _logService.LogDebug("debug")).AddTo(Disposables);
            LogInfoCommand = new ReactiveCommand(_ => _logService.LogInfo("info")).AddTo(Disposables);
            LogWarnCommand = new ReactiveCommand(_ => _logService.LogWarn("warn")).AddTo(Disposables);
            LogErrorCommand = new ReactiveCommand(async _ =>
            {
                var messageBox = MessageBoxManager
                .GetMessageBoxStandard(
                    $"captionのはず",
                    "アプリケーションを起動します",
                    MsBox.Avalonia.Enums.ButtonEnum.YesNo);

                var result = await messageBox.ShowWindowAsync();
                System.Diagnostics.Debug.WriteLine(result);
                if (result == MsBox.Avalonia.Enums.ButtonResult.No)
                    return;


                try
                {
                    throw new NotImplementedException("エラーが発生しました（LogErrorCommandに仮で実装しています）");
                }
                catch (Exception e)
                {
                    _logService.LogError("error", e);
                }
            }).AddTo(Disposables);
            LogFatalCommand = new ReactiveCommand(_ => _logService.LogFatal("fatal")).AddTo(Disposables);
            LogDebugParalellCommand = new ReactiveCommand(_ =>
            {
                // メインスレッドからのログ
                _logService.LogDebug("Log from main thread");

                // 異なるスレッドからログを記録
                Task[] tasks = new Task[5];
                for (int i = 0; i < tasks.Length; i++)
                {
                    int taskId = i; // ローカルコピーを作成してクロージャを回避
                    tasks[i] = Task.Run(() =>
                    {
                        _logService.LogDebug($"Log from task {taskId}");
                    });
                }

                // 全てのタスクが完了するまで待機
                Task.WaitAll(tasks);

            }).AddTo(Disposables);
        }


        public ReactiveCommand LogDebugCommand { get; }
        public ReactiveCommand LogInfoCommand { get; }
        public ReactiveCommand LogWarnCommand { get; }
        public ReactiveCommand LogErrorCommand { get; }
        public ReactiveCommand LogFatalCommand { get; }
        public ReactiveCommand LogDebugParalellCommand { get; }
    }
}
