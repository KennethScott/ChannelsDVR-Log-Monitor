using ChannelsDVR_Log_Monitor.Models.Config;
using ChannelsDVR_Log_Monitor.Services.Bonjour;
using ChannelsDVR_Log_Monitor.Services.Persistence;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Serilog;

namespace ChannelsDVR_Log_Monitor.Services.ChannelsLogs;

public class ChannelsLogHttpService(
    HttpClient httpClient,
    IOptions<AppConfig> appConfig,
    IPersistenceService persistenceService,
    IBonjourService bonjourService,
    IMemoryCache cache
) : ChannelsLogServiceBase(appConfig, persistenceService)
{
#pragma warning disable IDE0052
    private Timer? _pollingTimer;
#pragma warning restore IDE0052

    public override Task InitializeAsync()
    {
        var url = GetApiUrl();

        var pollingInterval = TimeSpan.FromMinutes(
            appConfig.Value.LogApiPoller.PollingIntervalMinutes
        );

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

        return logs;
    }

    private string GetApiUrl()
    {
        // Define a unique key for caching the URL
        const string urlCacheKey = "ApiUrl";

        // Try to get the URL from the cache
        var cachedUrl = GetUrlFromCache(urlCacheKey);
        if (!string.IsNullOrEmpty(cachedUrl))
        {
            return cachedUrl;
        }

        // If not in cache, discover the URL synchronously
        var url = appConfig.Value.LogApiPoller.LogApiUrl;
        if (string.IsNullOrEmpty(url))
        {
            url = Task.Run(
                    () =>
                        bonjourService.DiscoverServiceUrlAsync(
                            appConfig.Value.LogApiPoller.BonjourServiceName
                        )
                )
                .GetAwaiter()
                .GetResult();
        }

        if (string.IsNullOrEmpty(url))
        {
            Log.Error("Unable to discover ChannelsDVR Url and no LogApiUrl specified.");
            ArgumentNullException.ThrowIfNull(url);
        }
        else
        {
            // Cache the discovered URL
            SetUrlInCache(urlCacheKey, url);
        }

        return url;
    }

    private string? GetUrlFromCache(string urlKey)
    {
        return cache.Get<string>(urlKey);
    }

    private void SetUrlInCache(string urlKey, string url)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions
        {
            Priority = CacheItemPriority.NeverRemove
        };

        cache.Set(urlKey, url, cacheEntryOptions);
    }
}
