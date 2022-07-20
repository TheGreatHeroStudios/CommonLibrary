using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace TGH.Common.Utilities.CodeGen
{
	public static class EnumToCodeTableSqlGenerator
	{
		#region File-Specific Constant(s)
		private const string CREATE_TABLE_BOILERPLATE =
			"CREATE TABLE IF NOT EXISTS [{0}{1}] (" +
			"[{1}Id] INTEGER NOT NULL CONSTRAINT [PK_{0}{1}] PRIMARY KEY, " +
			"[{1}Description] NVARCHAR({2}) NOT NULL);";

		private const string INSERT_BOILERPLATE =
			"INSERT INTO [{0}{1}] ([{1}Id], [{1}Description]) VALUES ({2}, '{3}');";
		#endregion



		#region Public Method(s)
		public static void GenerateCodeTableSql
		(
			string enumNamespace, 
			string sqlOutputFileName, 
			string matchPattern = null, 
			string tablePrefix = ""
		)
		{
			IEnumerable<Type> enumTypes =
				AppDomain
					.CurrentDomain
					.GetAssemblies()
					.SelectMany(assembly => assembly.GetTypes())
					.Where
					(
						type =>
							type.IsEnum &&
							type.Namespace == enumNamespace &&
							(
								matchPattern == null ||
								Regex.IsMatch(type.FullName, matchPattern)
							)
					);

			if(!sqlOutputFileName.EndsWith(".sql", StringComparison.InvariantCultureIgnoreCase))
			{
				sqlOutputFileName += ".sql";
			}

			StringBuilder sqlFileContentBuilder = new StringBuilder();

			foreach(Type enumType in enumTypes)
			{
				string[] enumValues = Enum.GetNames(enumType);

				int maxFieldSize = 
					enumValues.OrderByDescending(enumValue => enumValue.Length).First().Length;

				sqlFileContentBuilder
					.AppendLine
					(
						string.Format
						(
							CREATE_TABLE_BOILERPLATE,
							tablePrefix,
							enumType.Name,
							maxFieldSize
						)
					);

				sqlFileContentBuilder.AppendLine();

				foreach(string enumValue in enumValues)
				{
					int codeId = (int)Enum.Parse(enumType, enumValue);

					sqlFileContentBuilder
						.AppendLine
						(
							string.Format
							(
								INSERT_BOILERPLATE,
								tablePrefix,
								enumType.Name,
								codeId,
								enumValue
							)
						);
				}

				sqlFileContentBuilder.AppendLine();
			}

			File.WriteAllText(sqlOutputFileName, sqlFileContentBuilder.ToString());
		}
		#endregion
	}
}
