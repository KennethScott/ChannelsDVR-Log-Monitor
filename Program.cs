using ChannelsDVR_Log_Monitor.Models;
using ChannelsDVR_Log_Monitor.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

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

        services.Configure<AppConfig>(config);

        services.AddHttpClient<IChannelsLogService, ChannelsLogService>();

        services.AddSingleton<IPersistenceService, LiteDbService>();
        services.AddSingleton<IEmailService, EmailService>();
        services.AddSingleton<App>();
    }
}
