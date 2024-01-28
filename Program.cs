using ChannelsDVR_Log_Monitor.Models.Config;
using ChannelsDVR_Log_Monitor.Services.Bonjour;
using ChannelsDVR_Log_Monitor.Services.ChannelsDevices;
using ChannelsDVR_Log_Monitor.Services.ChannelsLogs;
using ChannelsDVR_Log_Monitor.Services.ChannelsUrl;
using ChannelsDVR_Log_Monitor.Services.Notifications;
using ChannelsDVR_Log_Monitor.Services.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using static ChannelsDVR_Log_Monitor.Models.Config.Logs;

namespace ChannelsDVR_Log_Monitor;

static class Program
{
    public static async Task Main()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        await using var serviceProvider = services.BuildServiceProvider();

        Log.Information("Starting application");

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true; // Prevent the application from terminating immediately.
            cts.Cancel();
        };

        var app = serviceProvider.GetRequiredService<App>();
        await app.RunAsync(cts.Token);

        Log.Information("Ending application");
        Log.CloseAndFlush();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(config).CreateLogger();

        services.AddMemoryCache();

        var appConfig = new AppConfig();
        config.Bind(appConfig);
        services.Configure<AppConfig>(config);

        services.AddSingleton<IPersistenceService, LiteDbService>();
        services.AddSingleton<INotificationService, EmailNotificationService>();
        services.AddSingleton<NotificationHandler>();
        services.AddSingleton<IBonjourService, ZeroconfService>();
        services.AddSingleton<IChannelsUrlService, ChannelsUrlService>();
        services.AddSingleton<IChannelsDevicesService, ChannelsDevicesService>();

        switch (appConfig.Logs.MonitoringType)
        {
            case LogMonitoringType.File:
                services.AddSingleton<IChannelsLogService, ChannelsLogFileService>();
                break;
            case LogMonitoringType.API:
                services.AddHttpClient<IChannelsLogService, ChannelsLogHttpService>();
                break;
            default:
                throw new InvalidOperationException("Invalid log monitoring type specified.");
        }

        services.AddSingleton<App>();
    }
}
