using ChannelsDVR_Log_Monitor.Services.ChannelsDevices;
using ChannelsDVR_Log_Monitor.Services.ChannelsLogs;
using ChannelsDVR_Log_Monitor.Services.Notifications;
using Serilog;

namespace ChannelsDVR_Log_Monitor;

public class App
{
    private readonly IChannelsLogService _channelsLogService;
    private readonly IChannelsDevicesService _channelsDevicesService;
    private readonly NotificationHandler _notificationHandler;

    public App(
        IChannelsLogService channelsLogService,
        IChannelsDevicesService channelsDevicesService,
        NotificationHandler notificationHandler
    )
    {
        _notificationHandler = notificationHandler;

        _channelsLogService = channelsLogService;

        // Subscribe the notification handler to the log service's event
        _channelsLogService.OnNewLogs += async (_, notifications) =>
        {
            try
            {
                await _notificationHandler.HandleNewMessagesAsync(notifications);
            }
            catch (Exception ex)
            {
                Log.Error($"Error handling new logs: {ex}");
            }
        };

        _channelsDevicesService = channelsDevicesService;

        // Subscribe the notification handler to the log service's event
        _channelsDevicesService.OnNewDeviceChanges += async (_, notifications) =>
        {
            try
            {
                await _notificationHandler.HandleNewMessagesAsync(notifications);
            }
            catch (Exception ex)
            {
                Log.Error($"Error handling new logs: {ex}");
            }
        };
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            Log.Information("Initializing log service...");
            await _channelsLogService.InitializeAsync();

            Log.Information("Initializing devices service...");
            await _channelsDevicesService.InitializeAsync();

            Log.Information("Application started.");

            // Keep the application running until a cancellation is requested
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
            Log.Information("Application stopping...");
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
