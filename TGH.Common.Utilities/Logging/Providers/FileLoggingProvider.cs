using System;
using System.IO;
using TGH.Common.Patterns.PubSub;
using TGH.Common.Utilities.Logging.Enums;
using TGH.Common.Utilities.Logging.EventTypes;

namespace TGH.Common.Utilities.Logging.Providers
{
	public class FileLoggingProvider : LoggingProvider
	{
		#region File-Specific Constant(s)
		private const string LOG_FILE_HEADER_DIVIDER = "--------------------------------------------------------------";
		private const string LOG_FILE_SESSION_STAMP_TEMPLATE = "Logging Session Started ({0})";
		#endregion



		#region Fields
		private string _filePath = FileConstants.DEFAULT_LOG_FILEPATH;
		private StreamWriter _fileStream = null;
		#endregion



		#region Constructor
		public FileLoggingProvider
		(
			string logFileName, 
			string logFilePath = null
		) : base()
		{
			//If the provided path is valid, set it as the location to write log data.
			//Otherwise, write the log file data to the current executable's directory
			if (logFilePath != null && Directory.Exists(logFilePath))
			{
				_filePath = logFilePath;
			}

			//Open the file stream for writing
			_fileStream = File.AppendText($"{_filePath}\\{logFileName}");

			//Write a few lines of audit data to the log file
			_fileStream.WriteLine(LOG_FILE_HEADER_DIVIDER);
			_fileStream.WriteLine
			(
				string.Format
				(
					LOG_FILE_SESSION_STAMP_TEMPLATE,
					DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
				)
			);
			_fileStream.WriteLine(LOG_FILE_HEADER_DIVIDER);
		}
		#endregion



		#region Base Class Implementation / Override(s)
		public override void Dispose()
		{
			//Write one blank line to delimit the text, 
			//then close the file stream and unsubscribe from LogMessageEvent
			_fileStream.WriteLine("");
			_fileStream.Dispose();
		}
		

		protected override void HandleLogMessageEvent(LogMessageEvent logMessageEvent)
		{
			_fileStream.WriteLine(logMessageEvent.Message);
		}
		#endregion
	}
}
