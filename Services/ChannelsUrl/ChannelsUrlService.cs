using ChannelsDVR_Log_Monitor.Models.Config;
using ChannelsDVR_Log_Monitor.Services.Bonjour;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Serilog;

namespace ChannelsDVR_Log_Monitor.Services.ChannelsUrl;

public class ChannelsUrlService(
    IOptions<AppConfig> appConfig,
    IBonjourService bonjourService,
    IMemoryCache cache
) : IChannelsUrlService
{
    public string GetApiUrl()
    {
        // Define a unique key for caching the URL
        const string urlCacheKey = nameof(appConfig.Value.ChannelsDVRServerBaseUrl);

        // Try to get the URL from the cache
        var cachedUrl = GetUrlFromCache(urlCacheKey);
        if (!string.IsNullOrEmpty(cachedUrl))
        {
            return cachedUrl;
        }

        // If not in cache, and base url not specified, discover the URL synchronously
        var url = appConfig.Value.ChannelsDVRServerBaseUrl;
        if (string.IsNullOrEmpty(url))
        {
            url = Task.Run(
                    () => bonjourService.DiscoverServiceUrlAsync(appConfig.Value.BonjourServiceName)
                )
                .GetAwaiter()
                .GetResult();
        }

        if (string.IsNullOrEmpty(url))
        {
            Log.Error(
                $"Unable to discover ChannelsDVR Url and no {nameof(appConfig.Value.ChannelsDVRServerBaseUrl)} specified."
            );
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
