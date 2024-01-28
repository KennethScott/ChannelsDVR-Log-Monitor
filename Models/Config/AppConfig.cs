namespace ChannelsDVR_Log_Monitor.Models.Config;

public class AppConfig
{
    public string ChannelsDVRServerBaseUrl { get; set; } = string.Empty;

    public string BonjourServiceName { get; set; } = "_channels_dvr._tcp";

    public Logs Logs { get; set; } = new();

    public Devices Devices { get; set; } = new();

    public EmailSettings EmailSettings { get; set; } = new();

    public string DatabasePath { get; set; } = "data/app.db";
}
