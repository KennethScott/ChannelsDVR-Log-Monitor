# ChannelsDVR-Log-Monitor

### Simple log monitor with alerting for ChannelsDVR

This project is a .NET8 cross-platform console application that periodically polls
a ChannelsDVR log endpoint.  Log records are parsed and compared to user-defined
alert rules.  If a match is found, the log records in question are collected and 
the user is notified via email.

The application is [containerized](https://hub.docker.com/r/kman0/channelsdvr-log-monitor) and expects the user to mount an **appsettings.json** file
for configuration.

Optionally, you may also provide a mount for the logs (with appropriate write permissions) 
via `/app/logs`.

Example mount command:

`docker run -d -v /path/to/appsettings.json:/app/appsettings.json channelsdvr-log-monitor:latest`

All application configuration is handled through the **appsettings.json** file.
A default copy is available in the root of this repository.  Simply copy it to your desired
location, make the necessary changes, and mount it when you run the container. 


#### Appsettings.json has the following settings

**LogMonitoringType**: Specifies the method to use for monitoring the log.  Default is "API".
  * "API" - Monitor the ChannelsDVR log endpoint url via polling
  * "FILE" - Monitor the ChannelsDVR log file via docker file mount

**LogApiPoller** - Used to configure poller when Monitoring Type = API <br/>
   If running in docker Host mode, the ChannelsDVR endpoint will try to be determined by listening via Bonjour. <br/>
   If running in docker Bridge mode (or Bonjour doesn't work), the Log Endpoint Url may be specified.
    
  1. **LogEndpointUrl**: The ChannelsDVR log endpoint URL to monitor. <br/>
     Example: `http://XXX.XXX.XXX.XXX:8089/log`
  1. **PollingIntervalMinutes**: The interval in minutes at which the log monitor polls for new log entries. <br/>
     Defaults to `2`
  1. **BonjourServiceName**: The service name being broadcast by the ChannelsDVR server via Bonjour. <br/>
     Defaults to `_channels_dvr._tcp`

**LogFileWatcher** - Used to configure file watcher when Monitoring Type = FILE <br/>
  1. **LogFilePath**: Specifies the mounted path to the raw ChannelsDVR log file.  This would normally be found
in your ChannelsDVR server installation folder at /data/channels-dvr.log.


**EmailSettings** <br/>
    Configure the SMTP server details and email addresses for sending notifications.
  1. **SmtpServer**: The SMTP server to use for sending emails (i.e. `smtp.gmail.com`)
  1. **SmtpPort**: The port to use for the SMTP server (i.e. `587`)
  1. **FromAddress**: The email address from which the alerts are sent.
  1. **ToAddress**: The email address to which the alerts are sent.
  1. **UserId**: The user ID for the SMTP server authentication.
  1. **Password**: The password for the SMTP server authentication.

**AlertRules**: array of rules to use for matching logs for alerting.  A single rule
  may contain a combination of StartsWith/Contains/NotContains/Regex.
  1. **LogType**: The log record type (i.e. [ERR], [DVR], etc.)
  1. **StartsWith**: desired description starts with text
  1. **Contains**: desired description contains text
  1. **NotContains**: desired description does not contain text
  1. **Regex**: desired description matches regex pattern


