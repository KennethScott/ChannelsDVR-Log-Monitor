using Serilog;
using Zeroconf;

namespace ChannelsDVR_Log_Monitor.Services.Bonjour;

public class ZeroconfService : IBonjourService
{
    public async Task<string?> DiscoverServiceUrlAsync(string serviceName)
    {
        try
        {
            Log.Information("Browsing for services...");
            var domains = await ZeroconfResolver.BrowseDomainsAsync();

            var service = domains.FirstOrDefault(d => d.Key.StartsWith(serviceName));

            if (service is not null)
            {
                var hosts = await ZeroconfResolver.ResolveAsync(service.Key);

                foreach (var host in hosts)
                {
                    // Safely attempt to get the port
                    if (
                        host.Services.FirstOrDefault(s => s.Key.EndsWith(service.Key))
                            is KeyValuePair<string, IService> v
                        && v.Value != null
                    )
                    {
                        var url = $"http://{host.IPAddress}:{v.Value.Port}/log";
                        Log.Information($"ChannelsDVR found: {url}");
                        return url;
                    }
                    else
                    {
                        Log.Warning(
                            $"Service found at {host.IPAddress}, but no port information available."
                        );
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Error during service discovery: {ex.Message}");
        }

        return null;
    }
}
