using System;
using System.Runtime.CompilerServices;

namespace TGH.Common
{
	/// <summary>
	///		Extends the <seealso cref="ApplicationException"/> to add more granular error information.
	/// </summary>
	/// <remarks>
	///		The properties and methods exposed by this class should be used for
	///		secure logging purposes only and should not be exposed to end users.
	/// </remarks>
	public class ApplicationLayerException : ApplicationException
	{
		#region Class-Specific Constant(s)
		protected const string LOGGING_TEMPLATE =
			"{0} Layer Exception \n" +
			"    File: {1} \n" +
			"    Member: {2} (Line {3})\n" +
			"    Message: {4}\n";
		#endregion



		#region Propert(ies)
		/// <summary>
		///		The layer of the application in which the exception was encountered.
		/// </summary>
		public string ApplicationLayer { get; protected set; }

		/// <summary>
		///		The path to the source file containing the member that caused the error.
		/// </summary>
		public string FilePath { get; protected set; }

		/// <summary>
		///		The name of the method, property, event, etc. in which the error was encountered
		/// </summary>
		public string MemberName { get; protected set; }

		/// <summary>
		///		The line number within the source file where the error was encountered.
		/// </summary>
		public int LineNumber { get; protected set; }
		#endregion



		#region Constructor(s)
		public ApplicationLayerException
		(
			string applicationLayer,
			string message,
			Exception innerException = null,
			[CallerFilePath] string filePath = "",
			[CallerMemberName] string memberName = "",
			[CallerLineNumber] int lineNumber = 0
		)
			: base(message, innerException)
		{
			ApplicationLayer = applicationLayer;
			FilePath = filePath;
			MemberName = memberName;
			LineNumber = lineNumber;
		}
		#endregion



		#region Override(s)
		/// <summary>
		///		Creates a detailed, formatted error message containing details
		///		about where in overall application the exception was encountered.
		/// </summary>
		public override string ToString()
		{
			return
				string.Format
				(
					LOGGING_TEMPLATE,
					ApplicationLayer,
					FilePath,
					MemberName,
					LineNumber,
					Message
				);
		}
		#endregion
	}
}
