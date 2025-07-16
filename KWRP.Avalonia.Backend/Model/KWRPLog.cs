namespace KWRP.Avalonia.Backend.Model
{
    public class KWRPLog
    {
        public DateTime Timestamp { get; }
        public string Message { get; }
        public LogType Type { get; }
        public string? CalledMemberName { get; }
        public string? CalledFilePath { get; }
        public string? CalledFileName => Path.GetFileName(CalledFilePath);
        public int CalledLineNumber { get; }
        public int? ThreadId { get; }

        public KWRPLog(LogType type, string message, string? calledMemberName, string? calledFilePath, int calledLineNumber)
        {
            Timestamp = DateTime.Now;
            Message = message;
            Type = type;
            CalledMemberName = calledMemberName;
            CalledFilePath = calledFilePath;
            CalledLineNumber = calledLineNumber;
            ThreadId = Environment.CurrentManagedThreadId;
        }

        public override string ToString()
        {
            return $"{Timestamp:yyyy-MM-dd HH:mm:ss.ff}|{Type}|{CalledFileName}({CalledMemberName}-{CalledLineNumber}[{ThreadId}])|{Message}";
        }
    }

    public enum LogType
    {
        Debug = 0,
        Info = 1,
        Warn = 2,
        Error = 3,
        Fatal = 4,
    }
}
