using System;
using System.Collections.Generic;
using System.Linq;
using TGH.Common.Patterns.PubSub;
using TGH.Common.Utilities.Logging.Enums;
using TGH.Common.Utilities.Logging.EventTypes;
using TGH.Common.Utilities.Logging.Providers;

namespace TGH.Common.Utilities.Logging
{
	public static class Logger
	{
		#region Non-Public Member(s)
		private static List<LoggingProvider> _loggingProviders;
		#endregion



		#region Public Propert(ies)
		public static LogLevel MinimumLoggingLevel { get; set; } = LogLevel.INFO;
		#endregion



		#region Public Method(s)
		public static void RegisterLoggingProviders(IEnumerable<LoggingProvider> providers)
		{
			if(_loggingProviders == null)
			{
				_loggingProviders = new List<LoggingProvider>(providers);
			}
			else
			{
				_loggingProviders.AddRange(providers);
			}
		}


		public static void LogDebug(string message, bool showLogLevel = true, bool showTimestamp = true)
		{
			PublishLogMessage(LogLevel.DEBUG, message, showLogLevel, showTimestamp);
		}


		public static void LogVerbose(string message, bool showLogLevel = true, bool showTimestamp = true)
		{
			PublishLogMessage(LogLevel.VERBOSE, message, showLogLevel, showTimestamp);
		}


		public static void LogInfo(string message, bool showLogLevel = true, bool showTimestamp = true)
		{
			PublishLogMessage(LogLevel.INFO, message, showLogLevel, showTimestamp);
		}


		public static void LogWarning(string message, bool showLogLevel = true, bool showTimestamp = true)
		{
			PublishLogMessage(LogLevel.WARN, message, showLogLevel, showTimestamp);
		}


		public static void LogError(string message, bool showLogLevel = true, bool showTimestamp = true)
		{
			PublishLogMessage(LogLevel.ERROR, message, showLogLevel, showTimestamp);
		}


		public static void WriteLine(string message)
		{
			PublishLogMessage(LogLevel.INFO, message, false, false);
		}


		public static void UnregisterLoggingProviders()
		{
			if (_loggingProviders?.Any() ?? false)
			{
				foreach(LoggingProvider provider in _loggingProviders)
				{
					provider.Dispose();
				}
			}
		}
		#endregion



		#region Non-Public Method(s)
		private static void PublishLogMessage(LogLevel level, string message, bool showLogLevel, bool showTimestamp)
		{
			if (level >= MinimumLoggingLevel)
			{
				LogMessageEvent messageEvent = new LogMessageEvent
				{
					Level = level,
					Message = 
						$"{(showTimestamp ? DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss.fff]") : "")}" +
						$"{(showLogLevel ? $"[{level}]" : "")}" +
						message
				};

				PubSubManager.Publish(messageEvent);
			}
		}
		#endregion
	}
}
