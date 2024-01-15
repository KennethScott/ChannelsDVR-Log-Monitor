namespace ChannelsDVR_Log_Monitor.Models;

public record ChannelsLogRecord
{
    public string Id { get; set; } = string.Empty;

    public DateTime DateTime { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}
