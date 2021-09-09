using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace TGH.Common.Persistence.Contexts
{
	public class SqliteDbContext : DbContext
	{
		#region Non-Public Member(s)
		private string _targetDatabaseRootedFilePath;
		private string _sourceDatabaseTemplateFilePath;
		#endregion



		#region Constructor(s)
		/// <summary>
		///		Constructs an instance of a <seealso cref="DbContext"/> which uses
		///		an underlying Sqlite database as its storage mechanism.
		/// </summary>
		/// <remarks>
		///		Using this constructor overload assumes that the Sqlite database
		///		already exists at the rooted path specified by the parameter 
		///		<paramref name="targetDatabaseRootedPath"/>.  If the file does
		///		not exist at this path, an <seealso cref="ApplicationLayerException"/> 
		///		for the persistence layer is thrown when the context is configured.
		///		<para/>
		///		To avoid this when creating a new Sqlite database, construct the
		///		context using the other constructor overload which allows a
		///		source database file path to be specified, allowing it to
		///		be copied to the target directory if it does not yet exist.
		/// </remarks>
		/// <param name="targetDatabaseRootedPath">
		///		The rooted path (including file name) where the Sqlite
		///		database targeted by this instance of the context exists.
		/// </param>
		public SqliteDbContext(string targetDatabaseRootedPath)
		{
			_targetDatabaseRootedFilePath = targetDatabaseRootedPath;
		}


		/// <summary>
		///		Constructs an instance of a <seealso cref="DbContext"/> which uses
		///		an underlying Sqlite database as its storage mechanism.
		/// </summary>
		/// <remarks>
		///		If the file path supplied for the <paramref name="targetDatabaseRootedPath"/>
		///		parameter does not exist, the context will copy the file from the path
		///		supplied for the <paramref name="sourceDatabaseTemplatePath"/> parameter.
		///		<para/>
		///		If neither of these paths resolve to a valid file location,
		///		an <seealso cref="ApplicationLayerException"/> for the 
		///		persistence layer is thrown when the context is configured.
		/// </remarks>
		/// <param name="targetDatabaseRootedPath">
		///		The rooted path (including file name) where the Sqlite
		///		database targeted by this instance of the context exists.
		/// </param>
		/// <param name="sourceDatabaseTemplatePath"></param>
		public SqliteDbContext
		(
			string targetDatabaseRootedPath,
			string sourceDatabaseTemplatePath
		)
		{
			_targetDatabaseRootedFilePath = targetDatabaseRootedPath;
			_sourceDatabaseTemplateFilePath = sourceDatabaseTemplatePath;
		}
		#endregion



		#region Public Propertie(s)
		public virtual string TargetDatabaseRootedFilePath => _targetDatabaseRootedFilePath;
		public virtual string SourceDatabaseTemplateFilePath => _sourceDatabaseTemplateFilePath;
		#endregion



		#region Override(s)
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!File.Exists(TargetDatabaseRootedFilePath))
			{
				//If the underlying SQLite database file doesn't exist, initialize 
				//it on the file system by copying from the specified source directory.
				InitializeSQLiteDatabase();
			}

			//Configure the context to use the configured local SQLite database 
			optionsBuilder
				.UseSqlite
				(
					string.Format
					(
						PersistenceLayerConstants.TEMPLATE_SQLITE_CONNECTION_STRING,
						TargetDatabaseRootedFilePath
					)
				);

			base.OnConfiguring(optionsBuilder);
		}


		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			//Find and apply all 'IEntityTypeConfiguration's defined in the persistence assembly
			modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

			base.OnModelCreating(modelBuilder);
		}
		#endregion



		#region Non-Public Method(s)
		private void InitializeSQLiteDatabase()
		{
			//Before doing anything, check to make sure that a valid
			//template database file path was specified from which to copy.
			if(_sourceDatabaseTemplateFilePath == null)
			{
				//If no template file was specified, throw an 'ApplicationLayerException'
				throw new ApplicationLayerException
				(
					PersistenceLayerConstants.LAYER_NAME,
					PersistenceLayerConstants.ERROR_SQLITE_SOURCE_NOT_SPECIFIED
				);
			}
			else if (!File.Exists(_sourceDatabaseTemplateFilePath))
			{
				//Likewise, if the specified template file could
				//not be found, throw an 'ApplicationLayerException'
				throw new ApplicationLayerException
				(
					PersistenceLayerConstants.LAYER_NAME,
					PersistenceLayerConstants.ERROR_SQLITE_SOURCE_NOT_FOUND
				);
			}

			//Strip the file name from the target database
			//file path to determine the target directory
			string targetDirectory =
				_targetDatabaseRootedFilePath
					.Substring
					(
						0,
						_targetDatabaseRootedFilePath
							.LastIndexOf(FileConstants.PATH_SEPARATOR)
					);

			//Create the target directory for the Sqlite
			//database file (if it doesn't already exist)
			Directory.CreateDirectory(targetDirectory);

			//Copy the database template file from the working directory
			//to the app data folder configured for the application. 
			File.Copy
			(
				_sourceDatabaseTemplateFilePath,
				_targetDatabaseRootedFilePath
			);
		}
		#endregion
	}
}
