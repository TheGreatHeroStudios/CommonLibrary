using System;
using System.Collections.Generic;
using System.Text;
using TGH.Common.Utilities.Logging.Enums;
using TGH.Common.Utilities.Logging.EventTypes;

namespace TGH.Common.Utilities.Logging.Providers
{
	public class ConsoleLoggingProvider : LoggingProvider
	{
		#region Abstract Implementation
		protected override void HandleLogMessageEvent(LogMessageEvent logMessageEvent)
		{
			ConsoleColor initialConsoleColor = Console.ForegroundColor;
			ConsoleColor messageColor = ConsoleColor.White;

			switch (logMessageEvent.Level)
			{
				case LogLevel.VERBOSE:
				case LogLevel.DEBUG:
				{
					messageColor = ConsoleColor.Cyan;
					break;
				}
				case LogLevel.WARN:
				{
					messageColor = ConsoleColor.Yellow;
					break;
				}
				case LogLevel.ERROR:
				{
					messageColor = ConsoleColor.Red;
					break;
				}
				default:
				{
					break;
				}
			}

			Console.ForegroundColor = messageColor;
			Console.WriteLine(logMessageEvent.Message);
			Console.ForegroundColor = initialConsoleColor;
		}
		#endregion
	}
}
