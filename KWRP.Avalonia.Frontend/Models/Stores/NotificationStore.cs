using Avalonia.Notification;
using KWRP.Avalonia.Backend.Enums;
using KWRP.Avalonia.Backend.Models;
using KWRP.Avalonia.Backend.Services;
using System;

namespace KWRP.Avalonia.Frontend.Models.Stores
{
    public class NotificationStore
    {
        object _lock = new();
        private readonly INotificationService _notificationService;

        public INotificationMessageManager Manager { get; } = new NotificationMessageManager();

        public NotificationStore(INotificationService notificationService)
        {
            _notificationService = notificationService;
            _notificationService.Notified += OnNotified;
        }

        private void OnNotified(KWRPNotification message)
        {
            lock (_lock)
            {
                // toast 見た目定義
                var builder = Manager
                    .CreateMessage()
                    .Accent(message.MessageType.ToAccent())
                    .Background("#333")
                    .HasBadge(message.MessageType.ToBadge())
                    .HasMessage(message.Message ?? "");

                // toast コマンド定義
                if (message.CommandHeader != null && message.Command != null)
                {
                    builder.WithButton(message.CommandHeader, button => message.Command?.Invoke());
                }
                builder.Dismiss().WithButton("×", button => { });

                // toast 自動で消えるまでの時間定義
                if (message.Duration > 0)
                {
                    builder.Dismiss().WithDelay(TimeSpan.FromSeconds(message.Duration));
                }

                // マネージャに投げる
                builder.Queue();
            }
        }
    }
}
