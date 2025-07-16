using KWRP.Avalonia.Backend.Models;

namespace KWRP.Avalonia.Backend.Services
{
    public interface INotificationService
    {
        event Action<KWRPNotification> Notified;

        void Notify(KWRPNotification notification);
    }
}
