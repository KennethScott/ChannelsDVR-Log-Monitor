using ChannelsDVR_Log_Monitor.Models.Config;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Serilog;

namespace ChannelsDVR_Log_Monitor.Services.Notifications;

public class EmailNotificationService(IOptions<AppConfig> appConfig) : INotificationService
{
    public async Task SendNotificationAsync(string body, string? subject = null)
    {
        try
        {
            var settings = appConfig.Value.EmailSettings;

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(settings.FromAddress));
            email.To.Add(MailboxAddress.Parse(settings.ToAddress));
            email.Subject = subject ?? settings.Subject;
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };

            using var smtpClient = new SmtpClient();
            await smtpClient.ConnectAsync(
                settings.SmtpServer,
                settings.SmtpPort,
                SecureSocketOptions.StartTls
            );
            await smtpClient.AuthenticateAsync(settings.UserId, settings.Password);
            await smtpClient.SendAsync(email);
            await smtpClient.DisconnectAsync(true);

            Log.Information("Email sent successfully");
        }
        catch (Exception ex)
        {
            Log.Error($"Error sending email: {ex}");
        }
    }
}
