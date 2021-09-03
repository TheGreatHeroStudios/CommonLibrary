using System.IO;
using static System.Environment;

namespace TGH.Common
{
	public static class FileConstants
	{
		public static readonly char PATH_SEPARATOR = Path.DirectorySeparatorChar;

		public static readonly string APP_DATA_DIRECTORY =
			$"{GetFolderPath(SpecialFolder.LocalApplicationData)}" +
			$"{PATH_SEPARATOR}TGHStudios{PATH_SEPARATOR}";

		public static readonly string EXECUTABLE_DIRECTORY =
			$"{CurrentDirectory}{PATH_SEPARATOR}";
	}
}
