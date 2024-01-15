
namespace ChannelsDVR_Log_Monitor.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string body);
    }
}