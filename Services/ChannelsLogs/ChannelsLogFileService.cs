using ChannelsDVR_Log_Monitor.Models.Config;
using ChannelsDVR_Log_Monitor.Services.Persistence;
using Microsoft.Extensions.Options;
using Serilog;

namespace ChannelsDVR_Log_Monitor.Services.ChannelsLogs;

public class ChannelsLogFileService(
    IOptions<AppConfig> appConfig,
    IPersistenceService persistenceService
) : ChannelsLogServiceBase(appConfig, persistenceService)
{
    private FileSystemWatcher? _fileWatcher;
    private long _lastMaxOffset;
    private readonly TimeSpan _debounceTime = TimeSpan.FromSeconds(1);
    private DateTime _lastReadTime = DateTime.MinValue;

    public override Task InitializeAsync()
    {
        _fileWatcher = new FileSystemWatcher
        {
            Path = Path.GetDirectoryName(appConfig.Value.Logs.FilePath)!,
            Filter = Path.GetFileName(appConfig.Value.Logs.FilePath),
            NotifyFilter = NotifyFilters.LastWrite
        };

        _fileWatcher.Changed += OnChangedAsync;
        _fileWatcher.EnableRaisingEvents = true;

        SetInitialFileOffsetToEnd();

        Log.Information("Channels File Log Service initialized");

        return Task.CompletedTask;
    }

    private void SetInitialFileOffsetToEnd()
    {
        try
        {
            using var fs = new FileStream(
                appConfig.Value.Logs.FilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite
            );
            _lastMaxOffset = fs.Length; // Set offset to end of the file
        }
        catch (Exception ex)
        {
            Log.Error($"Error setting initial file offset: {ex}");
        }
    }

    private async void OnChangedAsync(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType != WatcherChangeTypes.Changed)
            return;

        if (DateTime.Now - _lastReadTime < _debounceTime)
            return; // Skip this event, still within debounce time

        _lastReadTime = DateTime.Now;
        await Task.Delay(_debounceTime); // Wait to see if more changes are coming

        var newLogs = await ReadNewLogEntriesAsync();
        RaiseOnNewLogs(newLogs);
    }

    private async Task<List<string>> ReadNewLogEntriesAsync()
    {
        var newLogs = new List<string>();

        try
        {
            await using var fs = new FileStream(
                appConfig.Value.Logs.FilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite
            );
            using var sr = new StreamReader(fs);

            if (fs.Length < _lastMaxOffset)
            {
                // Handle the case where the file was truncated or rolled over
                _lastMaxOffset = 0;
            }

            fs.Seek(_lastMaxOffset, SeekOrigin.Begin);
            string newContent = await sr.ReadToEndAsync();
            _lastMaxOffset = fs.Position;

            var logs = ParseLogs(newContent);
            newLogs.AddRange(logs);
        }
        catch (Exception ex)
        {
            Log.Error($"Error reading log file: {ex}");
        }

        return newLogs;
    }
}
