namespace ChannelsDVR_Log_Monitor.Services.Notifications;

public interface INotificationService
{
    Task SendNotificationAsync(string message);
}
