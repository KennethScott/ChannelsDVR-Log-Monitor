namespace ChannelsDVR_Log_Monitor.Models;

public class NotificationEventArgs(List<string> messages, string? subject = null) : EventArgs
{
    public List<string> Messages { get; set; } = messages;

    public string? Subject { get; set; } = subject;
}
