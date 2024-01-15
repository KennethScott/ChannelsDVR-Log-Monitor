namespace ChannelsDVR_Log_Monitor.Services
{
    public interface IChannelsLogService
    {
        Task<List<string>> GetLogsAsync();
    }
}
