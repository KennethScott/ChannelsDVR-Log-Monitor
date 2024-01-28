namespace ChannelsDVR_Log_Monitor.Models.Config;

public class Devices
{
    public string ApiEndpoint { get; set; } = "/devices";

    public int ApiPollingIntervalMinutes { get; set; } = 60;

    public string[] PropertiesToIgnore { get; set; } =
        ["ChannelsDevice.DeviceAuth", "Channel.Favorite", "Channel.Hidden", "Channel.Enabled"];
}
