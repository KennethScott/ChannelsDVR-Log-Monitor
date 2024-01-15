using System.Text.RegularExpressions;
using ChannelsDVR_Log_Monitor.Models;
using Microsoft.Extensions.Options;
using Serilog;

namespace ChannelsDVR_Log_Monitor.Services;

public class ChannelsLogService(
    HttpClient httpClient,
    IOptions<AppConfig> appConfig,
    IPersistenceService persistenceService
) : IChannelsLogService
{
    public async Task<List<string>> GetLogsAsync()
    {
        Log.Debug("Fetching logs from endpoint...");

        var response = await httpClient.GetAsync(appConfig.Value.LogEndpointUrl);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        Log.Debug("Logs received");
        var logs = ParseLogs(content);

        return logs;
    }

    private List<string> ParseLogs(string logRecords)
    {
        Log.Debug("Parsing log records");
        string? lastProcessedLogId =
            persistenceService.GetValue<string>(nameof(lastProcessedLogId)) ?? string.Empty;

        var newLastProcessedLogId = lastProcessedLogId;
        var alerts = new List<string>();

        foreach (var logRecord in logRecords.Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(logRecord))
                continue;

            var log = ParseLogRecord(logRecord);
            if (log == null || string.Compare(log.Id, lastProcessedLogId) <= 0)
                continue;

            newLastProcessedLogId = log.Id;

            foreach (var rule in appConfig.Value.AlertRules)
            {
                if (CheckConditions(log, rule))
                {
                    Log.Debug($"Found log record matching rule {rule}: \n {log}");
                    alerts.Add(logRecord);
                    break; // one rule match is enough
                }
            }
        }

        // Update the last processed log ID
        if (newLastProcessedLogId != lastProcessedLogId)
        {
            Log.Debug($"Saving last processed log id: {newLastProcessedLogId}");
            persistenceService.SaveValue(nameof(lastProcessedLogId), newLastProcessedLogId);
        }

        return alerts;
    }

    private static ChannelsLogRecord? ParseLogRecord(string logRecord)
    {
        var parts = logRecord.Split(' ');

        var dateTimeStr = $"{parts[0]} {parts[1]}";
        if (!DateTime.TryParse(dateTimeStr, out var dateTime))
            return null;

        var type = parts[2];
        var description = string.Join(" ", parts.Skip(3));

        return new ChannelsLogRecord
        {
            Id = dateTimeStr,
            DateTime = dateTime,
            Type = type,
            Description = description
        };
    }

    private static bool CheckConditions(ChannelsLogRecord log, AlertRule rule)
    {
        if (log.Type != rule.LogType)
            return false;

        if (rule.StartsWith != null && !log.Description.StartsWith(rule.StartsWith))
            return false;

        if (rule.Contains != null && !log.Description.Contains(rule.Contains))
            return false;

        if (rule.NotContains != null && log.Description.Contains(rule.NotContains))
            return false;

        if (rule.Regex != null && !Regex.IsMatch(log.Description, rule.Regex))
            return false;

        return true;
    }
}
