namespace ChannelsDVR_Log_Monitor.Services.Notifications;

public class NotificationHandler(IEnumerable<INotificationService> notificationServices)
{
    public async Task HandleNewLogsAsync(List<string> logs)
    {
        if (logs.Count > 0)
        {
            var message = string.Join(Environment.NewLine, logs);
            foreach (var service in notificationServices)
            {
                await service.SendNotificationAsync(message);
            }
        }
    }
}
