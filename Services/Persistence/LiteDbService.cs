using ChannelsDVR_Log_Monitor.Models.Config;
using LiteDB;
using Microsoft.Extensions.Options;

namespace ChannelsDVR_Log_Monitor.Services.Persistence;

public class LiteDbService : IPersistenceService
{
    private readonly string _databasePath;

    public LiteDbService(IOptions<AppConfig> appConfig)
    {
        _databasePath = appConfig.Value.DatabasePath;
        EnsureDatabaseDirectoryExists();
    }

    public void SaveValue<T>(string key, T value)
    {
        using var db = new LiteDatabase(_databasePath);
        var col = db.GetCollection<StoredObject<T>>("data");
        col.Upsert(new StoredObject<T> { Key = key, Value = value });
    }

    public T? GetValue<T>(string key)
    {
        using var db = new LiteDatabase(_databasePath);
        var col = db.GetCollection<StoredObject<T>>("data");
        var result = col.FindById(key);
        return result != null ? result.Value : default;
    }

    private void EnsureDatabaseDirectoryExists()
    {
        var directory = Path.GetDirectoryName(_databasePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public class StoredObject<T>
    {
        [BsonId]
        public string? Key { get; set; }
        public T? Value { get; set; }
    }
}
