using KWRP.Avalonia.Backend.Models;
using KWRP.Avalonia.Backend.Services;
using System;

namespace KWRP.Avalonia.Frontend.Services
{
    public class NotificationService : INotificationService
    {
        public event Action<KWRPNotification> Notified;

        public void Notify(KWRPNotification notification)
        {
            Notified?.Invoke(notification);
        }
    }
}
