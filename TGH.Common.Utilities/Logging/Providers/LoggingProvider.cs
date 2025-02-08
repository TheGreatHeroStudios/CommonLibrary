using System;
using System.Collections.Generic;
using System.Text;
using TGH.Common.Patterns.PubSub;
using TGH.Common.Utilities.Logging.EventTypes;

namespace TGH.Common.Utilities.Logging.Providers
{
	public abstract class LoggingProvider : IDisposable
	{
		#region Constructor(s)
		public LoggingProvider()
		{
			PubSubManager.Subscribe<LogMessageEvent>(HandleLogMessageEvent);
		}
		#endregion



		#region Abstract Method(s)
		protected abstract void HandleLogMessageEvent(LogMessageEvent logMessageEvent);
		#endregion



		#region 'IDisposable' Implementation
		public virtual void Dispose()
		{
			PubSubManager.Unsubscribe<LogMessageEvent>();
		}
		#endregion
	}
}
