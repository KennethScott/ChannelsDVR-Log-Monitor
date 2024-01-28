using ChannelsDVR_Log_Monitor.Models;

namespace ChannelsDVR_Log_Monitor.Services.Notifications;

public class NotificationHandler(IEnumerable<INotificationService> notificationServices)
{
    public async Task HandleNewMessagesAsync(NotificationEventArgs eventArgs)
    {
        if (eventArgs.Messages.Count > 0)
        {
            var message = string.Join(Environment.NewLine, eventArgs.Messages);
            foreach (var service in notificationServices)
            {
                await service.SendNotificationAsync(message, eventArgs.Subject);
            }
        }
    }
}
