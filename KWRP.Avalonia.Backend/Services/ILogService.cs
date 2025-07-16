using KWRP.Avalonia.Backend.Model;
using System.Runtime.CompilerServices;

namespace KWRP.Avalonia.Backend.Services
{
    public interface ILogService
    {
        event Action<KWRPLog> Logged;
        event Action<string> StatusMessageUpdated;

        void SetStatusMessage(string message);

        void LogDebug(
            string message,
            [CallerMemberName] string? memberName = "",
            [CallerFilePath] string? filePath = "",
            [CallerLineNumber] int lineNumber = -1);

        void LogInfo(
            string message,
            [CallerMemberName] string? memberName = "",
            [CallerFilePath] string? filePath = "",
            [CallerLineNumber] int lineNumber = -1);

        void LogWarn(
            string message,
            [CallerMemberName] string? memberName = "",
            [CallerFilePath] string? filePath = "",
            [CallerLineNumber] int lineNumber = -1);

        void LogError(
            string message,
            Exception? exception = null,
            [CallerMemberName] string? memberName = "",
            [CallerFilePath] string? filePath = "",
            [CallerLineNumber] int lineNumber = -1);

        void LogFatal(
            string message,
            Exception? exception = null,
            [CallerMemberName] string? memberName = "",
            [CallerFilePath] string? filePath = "",
            [CallerLineNumber] int lineNumber = -1);
    }
}
