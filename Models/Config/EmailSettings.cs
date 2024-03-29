﻿namespace ChannelsDVR_Log_Monitor.Models.Config;

public class EmailSettings
{
    public string Subject { get; set; } = "ChannelsDVR Alert";

    public string SmtpServer { get; set; } = string.Empty;

    public int SmtpPort { get; set; } = 587;

    public string FromAddress { get; set; } = string.Empty;

    public string ToAddress { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
