namespace TGH.Common.Persistence
{
	public static class PersistenceLayerConstants
	{
		public const string LAYER_NAME = "Persistence";



		#region Template(s)
		public const string TEMPLATE_SQLITE_CONNECTION_STRING = "Data Source = {0}";
		#endregion



		#region Error Message(s)
		public const string ERROR_SQLITE_DB_INITIALIZATION_FAILED =
			"Failed to initialize SQLite database '{0}'.  The target database file " +
			"does not exist and the source file used to generate it could not be found.";
		#endregion
	}
}
