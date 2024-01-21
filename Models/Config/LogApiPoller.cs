namespace ChannelsDVR_Log_Monitor.Models.Config;

public class LogApiPoller
{
    public string LogApiUrl { get; set; } = string.Empty;

    public int PollingIntervalMinutes { get; set; } = 2;

    public string BonjourServiceName { get; set; } = "_channels_dvr._tcp";
}
