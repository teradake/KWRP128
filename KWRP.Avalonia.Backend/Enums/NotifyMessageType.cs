namespace KWRP.Avalonia.Backend.Enums
{
    public enum NotifyMessageType
    {
        Info = 0,
        Warn = 1,
    }

    public static class NotifyMessageTypeExtensions
    {
        public static string ToBadge(this NotifyMessageType type)
        {
            return type switch
            {
                NotifyMessageType.Info => "Info",
                NotifyMessageType.Warn => "Warn",
                _ => throw new ArgumentException()
            };
        }

        public static string ToAccent(this NotifyMessageType type)
        {
            return type switch
            {
                NotifyMessageType.Info => "#1751C3",
                NotifyMessageType.Warn => "#C31751",
                _ => throw new ArgumentException()
            };
        }
    }
}
