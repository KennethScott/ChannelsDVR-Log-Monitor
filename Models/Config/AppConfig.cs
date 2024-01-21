namespace ChannelsDVR_Log_Monitor.Models.Config;

public class AppConfig
{
    public LogMonitoringType LogMonitoringType { get; set; }

    public LogApiPoller LogApiPoller { get; set; } = new();

    public LogFileWatcher LogFileWatcher { get; set; } = new();

    public IEnumerable<AlertRule> AlertRules { get; set; } = Enumerable.Empty<AlertRule>();

    public EmailSettings EmailSettings { get; set; } = new EmailSettings();

    public string DatabasePath { get; set; } = "data/app.db";
}
