namespace ChannelsDVR_Log_Monitor.Services.ChannelsLogs;

public interface IChannelsLogService
{
    event EventHandler<List<string>> OnNewLogs;

    Task InitializeAsync();
}
