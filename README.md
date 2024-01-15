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
A default copy is available in the root of this repository.  Copy it to your desired
location, make the necessary changes, and mount it when you run the container. 


The **appsettings.json** has the following settings

- `PollingIntervalMinutes`: The interval in minutes at which the log monitor polls for new log entries.

- `LogEndpointUrl`: The ChannelsDVR log endpoint URL to monitor.
  - Example: `"LogEndpointUrl": "http://XXX.XXX.XXX.XXX:8089/log"`

- `Email Settings`
Configure the SMTP server details and email addresses for sending notifications.
  - `SmtpServer`: The SMTP server to use for sending emails (i.e. `smtp.gmail.com`)
  - `SmtpPort`: The port to use for the SMTP server (i.e. `587`)
  - `FromAddress`: The email address from which the alerts are sent.
  - `ToAddress`: The email address to which the alerts are sent.
  - `UserId`: The user ID for the SMTP server authentication.
  - `Password`: The password for the SMTP server authentication.

- `Alert Rules`: array of rules to use for matching logs for alerting.  A single rule
  may contain a combination of StartsWith/Contains/NotContains/Regex.
  - `LogType`: The log record type (i.e. [ERR], [DVR], etc.)
  - `StartsWith`: desired description starts with text
  - `Contains`: desired description contains text
  - `NotContains`: desired description does not contain text
  - `Regex`: desired description matches regex pattern
