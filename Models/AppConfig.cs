namespace ChannelsDVR_Log_Monitor.Models;

public class AppConfig
{
    public int PollingIntervalMinutes { get; set; } = 2;

    public string LogEndpointUrl { get; set; } = string.Empty;

    public IEnumerable<AlertRule> AlertRules { get; set; } = Enumerable.Empty<AlertRule>();

    public EmailSettings EmailSettings { get; set; } = new EmailSettings();

    public string DatabasePath { get; set; } = "data/app.db";
}
