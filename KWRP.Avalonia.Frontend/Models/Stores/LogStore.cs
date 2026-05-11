using KWRP.Avalonia.Backend.Model;
using KWRP.Avalonia.Backend.Services;
using KWRP.Backend.Services;
using ObservableCollections;
using R3;
using System;

namespace KWRP.Avalonia.Frontend.Models.Stores
{
    public class LogStore
    {
        const int C_LOG_CAPACITY = 200;

        private readonly ILogService _logService;
        private readonly ILanguageService _languageService;

        public LogStore(
            ILogService logService, 
            ILanguageService languageService)
        {
            _logService = logService;
            _languageService = languageService;

            // ログが発行された際の処理
            _logService.Logged += KWRPLogs.AddFirst;

            StatusMessageObservable = Observable.FromEvent<string>(
                h => _logService.StatusMessageUpdated += h,
                h => _logService.StatusMessageUpdated -= h)
                .Debounce(TimeSpan.FromMilliseconds(50))
                .Do(key =>
                {
                    currentStatusMessageKey = key;
                });

            Observable.FromEvent(
                h => _languageService.LanguageChanged += h,
                h => _languageService.LanguageChanged -= h)
                .Subscribe(_ =>
                {
                    // ステータスメッセージの再発行
                    _logService.SetStatusMessage(currentStatusMessageKey);
                });

            _logService.LogDebug("init");
        }


        private string currentStatusMessageKey = "";
        public ObservableFixedSizeRingBuffer<KWRPLog> KWRPLogs { get; } = new(C_LOG_CAPACITY);
        public Observable<string> StatusMessageObservable { get; }
    }
}
