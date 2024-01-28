namespace ChannelsDVR_Log_Monitor.Models.Config;

public class Devices
{
    public string ApiEndpoint { get; set; } = "/devices";

    public int ApiPollingIntervalMinutes { get; set; } = 60;

    public List<string>? PropertiesToIgnore { get; set; } = null;
}
