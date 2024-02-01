# ChannelsDVR Log Monitor

### Simple log monitor with alerting for ChannelsDVR

This project is a containerized .NET8 cross-platform console application used to monitor ChannelsDVR logs. 
The logs can be polled from the **log** endpoint, or read realtime from the filesystem (if accessible).
Log records are parsed and compared to user-defined alert rules.  If a match is found, the log 
records in question are collected and the user is notified via email.

Please note the difference between monitoring the log via API vs filesystem is that the log API
only returns the most recent 5000 log records.  It's therefore possible if you have the polling
interval set long and you are seeing lots of log records written that you could theoretically 
miss events.  For the vast majority, this is of no concern and therefore the default polling is set
at 2 minutes.  If you want truly realtime, then simply mount the log file.

The app is also now capable of monitoring the **devices** endpoint to alert on device and channel changes. 

The application is [containerized](https://hub.docker.com/r/kman0/channelsdvr-log-monitor) and expects the user to mount an **appsettings.json** file
for configuration, but settings may also be passed via environment variables.

Optionally, you may also provide a mount for the logs (with appropriate write permissions) via `/app/logs`.

Example mount command:

`docker run -d -v /path/to/appsettings.json:/app/appsettings.json channelsdvr-log-monitor:latest`

All application configuration is handled through the **appsettings.json** file, but all settings may also be specified using environment variables.
A default copy appsettings.json is available in the root of this repository.  Simply copy it to your desired
location, make the necessary changes, and mount it when you run the container, or run the container passing settings as environment variables. 

There are really only 2 required settings as everything else will default:
1.  Either specify the ChannelsBaseUrl manually, OR run docker in Host mode so it can automatically find the server via Bonjour.
1.  Your EmailSettings for notifications


#### Appsettings.json

**ChannelsDVRServerBaseUrl**: The ChannelsDVR server base URL <br/>
     Example: `http://XXX.XXX.XXX.XXX:8089

**BonjourServiceName**: The service name being broadcast by the ChannelsDVR server via Bonjour. <br/>
     Defaults to `_channels_dvr._tcp`
 
**Logs.MonitoringType**: Specifies the method to use for monitoring the log.  Default is 0/API. 
  * 0 = API - Monitor the ChannelsDVR log endpoint url via polling (default) 
  * 1 = FILE - Monitor the ChannelsDVR log file realtime via docker file mount

**Logs.ApiPollingIntervalMinutes**: The interval in minutes at which the log monitor polls for new log entries. <br/>
     Defaults to `2`
    
**Logs.ApiEndpoint**: The ChannelsDVR server log endpoint to monitor. <br/>
     Defaults to `/log`

**Logs.FilePath**: Specifies the mounted path to the raw ChannelsDVR log file.  This would normally be found
in your ChannelsDVR server installation folder at /data/channels-dvr.log.

**Logs.AlertRules**: array of rules to use for matching logs for alerting.  A single rule
  may contain a combination of StartsWith/Contains/NotContains/Regex.
  Defaults to all [ERR] log entries and [DVR] log entries that start with "Error running job"
  1. **LogType**: The log record type (i.e. [ERR], [DVR], etc.)
  1. **StartsWith**: desired description starts with text
  1. **Contains**: desired description contains text
  1. **NotContains**: desired description does not contain text
  1. **Regex**: desired description matches regex pattern

**Devices.ApiPollingIntervalMinutes**: The interval in minutes at which the log monitor polls for new log entries. <br/>
     Defaults to `60`    

**Devices.ApiEndpoint**: The ChannelsDVR server log endpoint monitor. <br/>
     Defaults to `/devices`

**Devices.PropertiesToIgnore**: Optional list of Channel property names to ignore as to not be alerted if their values change.
     Defaults to `[ "ChannelsDevice.DeviceAuth", "Channel.Favorite", "Channel.Hidden", "Channel.Enabled" ]`


**EmailSettings** <br/>
    Configure the SMTP server details and email addresses for sending notifications.
  1. **SmtpServer**: The SMTP server to use for sending emails (i.e. `smtp.gmail.com`)
  1. **SmtpPort**: The port to use for the SMTP server. Defaults to `587`
  1. **FromAddress**: The email address from which the alerts are sent.
  1. **ToAddress**: The email address to which the alerts are sent.
  1. **UserId**: The user ID for the SMTP server authentication.
  1. **Password**: The password for the SMTP server authentication.



