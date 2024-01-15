using ChannelsDVR_Log_Monitor.Models;
using ChannelsDVR_Log_Monitor.Services;
using Microsoft.Extensions.Options;
using Serilog;

namespace ChannelsDVR_Log_Monitor;

public class App(
    IOptionsSnapshot<AppConfig> appConfig,
    IChannelsLogService logService,
    IEmailService emailService
)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            Log.Information("Begin polling...");

            while (!cancellationToken.IsCancellationRequested)
            {
                var pollingInterval = TimeSpan.FromMinutes(appConfig.Value.PollingIntervalMinutes);
                var logRecs = await logService.GetLogsAsync();

                if (logRecs.Count > 0)
                {
                    Log.Information($"Alerting: {logRecs.Count} log records found!");
                    var body = string.Join(Environment.NewLine, logRecs);
                    await emailService.SendEmailAsync(body);
                }

                try
                {
                    await Task.Delay(pollingInterval, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    Log.Information("Polling canceled.");
                    break; // Exit the loop if the operation is cancelled
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Unexpected Error: {ex}");
        }
        finally
        {
            Log.Information("Shutting down application gracefully.");
        }
    }
}
