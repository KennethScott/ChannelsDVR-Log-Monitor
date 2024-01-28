using ChannelsDVR_Log_Monitor.Models.Config;
using ChannelsDVR_Log_Monitor.Services.ChannelsUrl;
using ChannelsDVR_Log_Monitor.Services.Persistence;
using Microsoft.Extensions.Options;
using Serilog;

namespace ChannelsDVR_Log_Monitor.Services.ChannelsLogs;

public class ChannelsLogHttpService(
    HttpClient httpClient,
    IOptions<AppConfig> appConfig,
    IPersistenceService persistenceService,
    IChannelsUrlService channelsUrlService
) : ChannelsLogServiceBase(appConfig, persistenceService)
{
#pragma warning disable IDE0052
    private Timer? _pollingTimer;
#pragma warning restore IDE0052

    public override Task InitializeAsync()
    {
        var baseUrl = channelsUrlService.GetApiUrl();
        var endUrl = appConfig.Value.Logs.ApiEndpoint;

        var url = $"{baseUrl.TrimEnd('/')}/{endUrl.TrimStart('/')}";

        var pollingInterval = TimeSpan.FromMinutes(appConfig.Value.Logs.ApiPollingIntervalMinutes);

        _pollingTimer = new Timer(_ => TimerCallback(url), null, TimeSpan.Zero, pollingInterval);

        Log.Information("Channels API Log Service initialized");

        return Task.CompletedTask;
    }

    private void TimerCallback(string url)
    {
        // Invoke the async method without await
        GetLogsAsync(url).ConfigureAwait(false);
    }

    public async Task<List<string>> GetLogsAsync(string url)
    {
        Log.Debug("Fetching logs from endpoint...");

        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        Log.Debug("Logs received");
        var logs = ParseLogs(content);

        if (logs.Count > 0)
        {
            RaiseOnNewLogs(logs);
        }

        Log.Debug("Finished processing logs");

        return logs;
    }
}
