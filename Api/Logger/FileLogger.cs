using System.Text;

namespace Api.Logger
{
	public class FileLogger : ILogger
	{

		private readonly string logName;
		private readonly FileLoggerProvider LoggerPrv;

		public FileLogger(string logName, FileLoggerProvider loggerPrv)
		{
			this.logName = logName;
			this.LoggerPrv = loggerPrv;
		}
		public IDisposable BeginScope<TState>(TState state)
		{
			return null;
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return logLevel >= LoggerPrv.MinLevel;
		}

		string GetShortLogLevel(LogLevel logLevel)
		{
			switch (logLevel)
			{
				case LogLevel.Trace:
					return "TRCE";
				case LogLevel.Debug:
					return "DBUG";
				case LogLevel.Information:
					return "INFO";
				case LogLevel.Warning:
					return "WARN";
				case LogLevel.Error:
					return "FAIL";
				case LogLevel.Critical:
					return "CRIT";
			}
			return logLevel.ToString().ToUpper();
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
			Exception exception, Func<TState, Exception, string> formatter)
		{
			if (!IsEnabled(logLevel))
			{
				return;
			}

			if (formatter == null)
			{
				throw new ArgumentNullException(nameof(formatter));
			}

			string message = null;

			if (null != formatter)
			{
				message = formatter(state, exception);
			}

			if (LoggerPrv.Options.FilterLogEntry != null)
				if (!LoggerPrv.Options.FilterLogEntry(new LogMessage(logName, logLevel, eventId, message, exception)))
					return;

			if (LoggerPrv.FormatLogEntry != null)
			{
				LoggerPrv.WriteEntry(LoggerPrv.FormatLogEntry(
					new LogMessage(logName, logLevel, eventId, message, exception)));
			}
			else
			{
				var logBuilder = new StringBuilder();
				if (!string.IsNullOrEmpty(message))
				{
					DateTime timeStamp = LoggerPrv.UseUtcTimestamp ? DateTime.UtcNow : DateTime.Now;
					logBuilder.Append(timeStamp.ToString("[HH:mm:ss dd.MM.yy]"));
					logBuilder.Append($"\t{GetShortLogLevel(logLevel)} [{eventId}]\t[{logName, -30}] {message}");
				}

				if (exception != null)
				{
					logBuilder.AppendLine(exception.ToString());
				}

				LoggerPrv.WriteEntry(logBuilder.ToString());
			}
		}
	}
}
