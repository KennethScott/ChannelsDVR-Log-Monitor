namespace ChannelsDVR_Log_Monitor.Models;

public record ChannelsDevicesResponse
{
    public List<ChannelsDevice> Devices { get; set; } = [];
}

public record ChannelsDevice
{
    public string? DeviceID { get; set; }

    public string? DeviceAuth { get; set; }

    public string? IPAddress { get; set; }

    public int TunerCount { get; set; }

    public bool IsLegacy { get; set; }

    public string? FriendlyName { get; set; }

    public string? FirmwareName { get; set; }

    public string? FirmwareVersion { get; set; }

    public string? ModelNumber { get; set; }

    public string? Lineup { get; set; }

    public List<Channel> Channels { get; set; } = [];
}

public record Channel
{
    public string? ID { get; set; }

    public string? GuideKey => $"{GuideName} ({GuideNumber})";

    public string? GuideNumber { get; set; }

    public string? GuideName { get; set; }

    public string? Modulation { get; set; }

    public int Frequency { get; set; }

    public int ProgramNumber { get; set; }

    public int TransportStreamID { get; set; }

    public int HD { get; set; }

    public int Favorite { get; set; }

    public int Hidden { get; set; }

    public int Enabled { get; set; }

    public string? VideoCodec { get; set; }

    public string? AudioCodec { get; set; }

    public string? Station { get; set; }

    public string? Logo { get; set; }
}
