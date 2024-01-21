namespace ChannelsDVR_Log_Monitor.Services.Bonjour;

public interface IBonjourService
{
    Task<string?> DiscoverServiceUrlAsync(string serviceName);
}
