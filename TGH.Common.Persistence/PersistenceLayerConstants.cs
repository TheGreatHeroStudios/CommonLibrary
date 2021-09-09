namespace TGH.Common.Persistence
{
	public static class PersistenceLayerConstants
	{
		public const string LAYER_NAME = "Persistence";



		#region Template(s)
		public const string TEMPLATE_SQLITE_CONNECTION_STRING = "Data Source = {0}";
		#endregion



		#region Error Message(s)
		public const string ERROR_SQLITE_SOURCE_NOT_FOUND =
			"Failed to initialize Sqlite database.  The target database file does not " +
			"exist and the source file specified to generate it could not be found.";

		public const string ERROR_SQLITE_SOURCE_NOT_SPECIFIED =
			"Failed to initialize Sqlite database.  The target database file " +
			"does not exist and no source file was specified to generate it.";
		#endregion
	}
}
