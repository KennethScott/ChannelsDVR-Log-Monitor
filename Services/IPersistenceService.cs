namespace ChannelsDVR_Log_Monitor.Services
{
    public interface IPersistenceService
    {
        void SaveValue<T>(string key, T value);

        T? GetValue<T>(string key);
    }
}
