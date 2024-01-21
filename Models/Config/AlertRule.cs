namespace ChannelsDVR_Log_Monitor.Models.Config;

public class AlertRule
{
    public string LogType { get; set; } = "[ERR]";

    public string? StartsWith { get; set; }

    public string? Contains { get; set; }

    public string? NotContains { get; set; }

    public string? Regex { get; set; }
}
