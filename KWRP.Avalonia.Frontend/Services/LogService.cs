using KWRP.Avalonia.Backend.Model;
using KWRP.Avalonia.Backend.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace KWRP.Avalonia.Frontend.Services
{
    public class LogService : ILogService
    {
        private readonly ILogger<LogService> _logger;


        public LogService(ILogger<LogService> logger)
        {
            _logger = logger;
        }

        public event Action<KWRPLog>? Logged;
        public event Action<string>? StatusMessageUpdated;

        #region インターフェイス実装
        public void SetStatusMessage(string message) => StatusMessageUpdated?.Invoke(message);

        public void LogDebug(string message,
            [CallerMemberName] string? memberName = "",
            [CallerFilePath] string? filePath = "",
            [CallerLineNumber] int lineNumber = -1)
        {
#if DEBUG
            Log(LogType.Debug, message, memberName, filePath, lineNumber);
#endif
        }

        public void LogInfo(string message,
            [CallerMemberName] string? memberName = "",
            [CallerFilePath] string? filePath = "",
            [CallerLineNumber] int lineNumber = -1)
        {
            Log(LogType.Info, message, memberName, filePath, lineNumber);
        }

        public void LogWarn(string message,
            [CallerMemberName] string? memberName = "",
            [CallerFilePath] string? filePath = "",
            [CallerLineNumber] int lineNumber = -1)
        {
            Log(LogType.Warn, message, memberName, filePath, lineNumber);
        }

        public void LogError(string message,
            Exception? exception = null,
            [CallerMemberName] string? memberName = "",
            [CallerFilePath] string? filePath = "",
            [CallerLineNumber] int lineNumber = -1)
        {
            if (exception != null)
            {
#if DEBUG
                message = $"{message}\n\nexception.trace:\n{exception.StackTrace}\n\nexception.message:\n {exception.Message}";
#else
                message = $"{message}\n{exception.Message}";
#endif
            }
            Log(LogType.Error, message, memberName, filePath, lineNumber);
        }

        public void LogFatal(string message,
            Exception? exception = null,
            [CallerMemberName] string? memberName = "",
            [CallerFilePath] string? filePath = "",
            [CallerLineNumber] int lineNumber = -1)
        {
            if (exception != null)
            {
#if DEBUG
                message = $"{message}\n\nexception.trace:\n{exception.StackTrace}\n\nexception.message:\n {exception.Message}";
#else
                message = $"{message}\n{exception.Message}";
#endif
            }
            Log(LogType.Fatal, message, memberName, filePath, lineNumber);
        }
        #endregion



        private void Log(LogType type, string message, string? memberName, string? filePath, int lineNumber)
        {
            var log = new KWRPLog(type, message, memberName, filePath, lineNumber);

            Logged?.Invoke(log);

            switch (log.Type)
            {
                case (LogType.Debug): _logger.LogDebug(log.ToString()); break;
                case (LogType.Info): _logger.LogInformation(log.ToString()); break;
                case (LogType.Warn): _logger.LogWarning(log.ToString()); break;
                case (LogType.Error): _logger.LogError(log.ToString()); break;
                case (LogType.Fatal): _logger.LogCritical(log.ToString()); break;
                default: break;
            }
        }
    }
}
