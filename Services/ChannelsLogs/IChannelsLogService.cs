using ChannelsDVR_Log_Monitor.Models;

namespace ChannelsDVR_Log_Monitor.Services.ChannelsLogs;

public interface IChannelsLogService
{
    event EventHandler<NotificationEventArgs> OnNewLogs;

    Task InitializeAsync();
}
