using ChannelsDVR_Log_Monitor.Models;

namespace ChannelsDVR_Log_Monitor.Services.ChannelsDevices
{
    public interface IChannelsDevicesService
    {
        event EventHandler<NotificationEventArgs>? OnNewDeviceChanges;

        Task<List<string>> GetDevicesAsync(string url);

        Task InitializeAsync();
    }
}
