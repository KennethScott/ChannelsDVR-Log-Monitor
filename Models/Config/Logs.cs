namespace ChannelsDVR_Log_Monitor.Models.Config;

public class Logs
{
    public enum LogMonitoringType
    {
        API,
        File
    }

    public LogMonitoringType MonitoringType { get; set; } = LogMonitoringType.API;

    public string ApiEndpoint { get; set; } = "/log";

    public int ApiPollingIntervalMinutes { get; set; } = 2;

    public string FilePath { get; set; } = string.Empty;

    public IEnumerable<AlertRule> AlertRules { get; set; } =
        new List<AlertRule>()
        {
            new() { LogType = "[ERR]" },
            new() { LogType = "[DVR]", StartsWith = "Error running job" }
        };
}
