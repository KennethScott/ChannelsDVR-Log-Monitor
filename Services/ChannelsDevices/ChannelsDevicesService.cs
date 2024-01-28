using ChannelsDVR_Log_Monitor.Models;
using ChannelsDVR_Log_Monitor.Models.Config;
using ChannelsDVR_Log_Monitor.Services.ChannelsUrl;
using ChannelsDVR_Log_Monitor.Services.ObjectComparer;
using ChannelsDVR_Log_Monitor.Services.Persistence;
using KellermanSoftware.CompareNetObjects;
using Microsoft.Extensions.Options;
using Serilog;
using System.Text.Json;

namespace ChannelsDVR_Log_Monitor.Services.ChannelsDevices;

public class ChannelsDevicesService(
    HttpClient httpClient,
    IOptions<AppConfig> appConfig,
    IPersistenceService persistenceService,
    IChannelsUrlService channelsUrlService
) : IChannelsDevicesService
{
    private Timer? _pollingTimer;

    public event EventHandler<NotificationEventArgs>? OnNewDeviceChanges;

    protected void RaiseOnNewDeviceChanges(List<string> deviceChanges)
    {
        OnNewDeviceChanges?.Invoke(
            this,
            new NotificationEventArgs(deviceChanges, "ChannelsDVR Device Changes")
        );
    }

    public Task InitializeAsync()
    {
        var baseUrl = channelsUrlService.GetApiUrl();
        var endUrl = appConfig.Value.Devices.ApiEndpoint;

        if (string.IsNullOrEmpty(endUrl))
        {
            Log.Information("No Devices endpoint defined. Exiting Devices Service.");
            return Task.CompletedTask;
        }

        var url = $"{baseUrl.TrimEnd('/')}/{endUrl.TrimStart('/')}";

        var pollingInterval = TimeSpan.FromMinutes(
            appConfig.Value.Devices.ApiPollingIntervalMinutes
        );

        _pollingTimer = new Timer(_ => TimerCallback(url), null, TimeSpan.Zero, pollingInterval);

        Log.Information("Channels Devices Service initialized");

        return Task.CompletedTask;
    }

    private void TimerCallback(string url)
    {
        // Invoke the async method without await
        GetDevicesAsync(url).ConfigureAwait(false);
    }

    public async Task<List<string>> GetDevicesAsync(string url)
    {
        Log.Debug("Loading previous devices from database");

        ChannelsDevicesResponse? previousDevices =
            persistenceService.GetValue<ChannelsDevicesResponse?>(nameof(previousDevices));

        Log.Debug("Fetching devices from endpoint...");

        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        Log.Debug("Devices received");

        var currentDevices = DeserializeResponse(content);

        var deviceChanges = CompareDevices(previousDevices, currentDevices);

        if (deviceChanges.Count > 0)
        {
            RaiseOnNewDeviceChanges(deviceChanges);
        }

        Log.Debug("Finished processing devices");

        return deviceChanges;
    }

    private static ChannelsDevicesResponse? DeserializeResponse(string response)
    {
        Log.Debug("Deserializing devices received from endpoint");

        ChannelsDevicesResponse currentDevices = new();

        using (JsonDocument document = JsonDocument.Parse(response))
        {
            JsonElement root = document.RootElement;
            foreach (JsonElement element in root.EnumerateArray())
            {
                ChannelsDevice? device = JsonSerializer.Deserialize<ChannelsDevice>(
                    element.GetRawText()
                );

                if (device is not null)
                {
                    currentDevices.Devices.Add(device);
                }
            }
        }

        return currentDevices;
    }

    protected List<string> CompareDevices(
        ChannelsDevicesResponse? previousDevices,
        ChannelsDevicesResponse? currentDevices
    )
    {
        Log.Debug("Comparing previous and current devices");

        var deviceChanges = new List<string>();

        if (previousDevices?.Devices.Count > 0)
        {
            CompareLogic comparer =
                new()
                {
                    Config = new()
                    {
                        MaxDifferences = Int32.MaxValue,
                        IgnoreCollectionOrder = true,
                        MembersToIgnore = [.. appConfig.Value.Devices.PropertiesToIgnore],
                        CollectionMatchingSpec = new()
                        {
                            { typeof(ChannelsDevice), ["FriendlyName"] },
                            { typeof(Channel), ["GuideKey"] }
                        },
                    }
                };

            var result = comparer.Compare(previousDevices, currentDevices);

            EmailHtmlReport htmlReport =
                new()
                {
                    Config = new()
                    {
                        GenerateFullHtml = true,
                        HtmlTitle = "Channels Comparison Report",
                        BreadCrumbColumName = "Change Detected",
                        ExpectedColumnName = "Previous",
                        ActualColumnName = "Current",
                    }
                };

            if (result.Differences.Count > 0)
            {
                var report = htmlReport.OutputString(result.Differences);
                deviceChanges.Add(report);
            }
        }

        // Update the last sources
        if (previousDevices?.Devices.Count == 0 || deviceChanges.Count > 0)
        {
            Log.Debug("Saving latest sources data");
            persistenceService.SaveValue(nameof(previousDevices), currentDevices);
        }

        return deviceChanges;
    }
}
