using TGH.Common.Patterns.PubSub;
using TGH.Common.Utilities.Logging.Enums;

namespace TGH.Common.Utilities.Logging.EventTypes
{
	public class LogMessageEvent : IPubSubEvent
	{
		public string Message { get; set; }
		public LogLevel Level { get; set; }
	}
}
