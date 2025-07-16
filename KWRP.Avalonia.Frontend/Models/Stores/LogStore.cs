using KWRP.Avalonia.Backend.Model;
using KWRP.Avalonia.Backend.Services;
using ObservableCollections;
using R3;
using System;

namespace KWRP.Avalonia.Frontend.Models.Stores
{
    public class LogStore
    {
        const int C_LOG_CAPACITY = 200;

        private readonly ILogService _logService;
        
        public LogStore(ILogService logService)
        {
            _logService = logService;

            // ログが発行された際の処理
            _logService.Logged += KWRPLogs.AddFirst;

            StatusMessageObservable = Observable.FromEvent<string>(
                h => _logService.StatusMessageUpdated += h,
                h => _logService.StatusMessageUpdated -= h)
                .Debounce(TimeSpan.FromMilliseconds(50));

            _logService.LogDebug("init");
        }


        public ObservableFixedSizeRingBuffer<KWRPLog> KWRPLogs { get; } = new(C_LOG_CAPACITY);
        public Observable<string> StatusMessageObservable { get; }
    }
}
