namespace ChannelsDVR_Log_Monitor.Models;

public class AlertRule
{
    public string LogType { get; set; } = "[ERR]";

    public string? StartsWith { get; set; } = string.Empty;

    public string? Contains { get; set; } = string.Empty;

    public string? NotContains { get; set; } = string.Empty;

    public string? Regex { get; set; } = string.Empty;
}
