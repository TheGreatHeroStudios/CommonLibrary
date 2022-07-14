﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TGH.Common.Patterns.IoC;
using TGH.Common.Repository.Interfaces;
using TGH.Common.Utilities.DataLoader.Extensions;
using TGH.Common.Utilities.DataLoader.Interfaces;

namespace TGH.Common.Utilities.DataLoader.Implementations
{
	public abstract class DataLoader<TDataType> : IDataLoader<TDataType>
		where TDataType : class
	{
		#region File-Specific Constant(s)
		private const string ADD_REQUEST_HEADER_FAILURE = "Failed to add request header.";
		private const string REMOTE_CONTENT_ACCESS_FAILURE = "Failed to get remote content.";
		private const string HTTP_CLIENT_NOT_SPECIFIED = "No HTTP client was specified for the target data loader.";
		#endregion



		#region Non-Public Member(s)
		private HttpClient _dataLoaderClient;
		private int? _actualRecordCount;

		protected IGenericRepository _repository;
		#endregion



		#region Constructor(s)
		public DataLoader()
		{
			_repository =
				DependencyManager.ResolveService<IGenericRepository>();
		}


		public DataLoader(string remoteContentURL = null, int requestTimeoutSeconds = 100, IEnumerable<(string Key, string Value)> defaultRequestHeaders = null)
		{
			if (remoteContentURL != null)
			{
				_dataLoaderClient = new HttpClient();
				_dataLoaderClient.BaseAddress = new Uri(remoteContentURL);
				_dataLoaderClient.Timeout = requestTimeoutSeconds > 0 ? TimeSpan.FromSeconds(requestTimeoutSeconds) : Timeout.InfiniteTimeSpan;
				defaultRequestHeaders?.ToList().ForEach(header => AddRequestHeader(header.Key, header.Value));
			}

			_repository =
				DependencyManager.ResolveService<IGenericRepository>();
		}


		public DataLoader(HttpClient client)
		{
			_dataLoaderClient = client;

			_repository =
				DependencyManager.ResolveService<IGenericRepository>();
		}
		#endregion



		#region Interface Implementations
		public int ActualRecordCount
		{
			get
			{
				if (!_actualRecordCount.HasValue)
				{
					//If the record count from the database has not
					//yet been checked, check it and cache the result
					_actualRecordCount =
						_repository.GetRecordCount(RecordRetrievalPredicate);
				}

				return _actualRecordCount.Value;
			}
		}

		public virtual int ExpectedRecordCount => 0;

		public abstract Func<TDataType, int> KeySelector { get; }

		public virtual Func<TDataType, bool> RecordRetrievalPredicate => entity => true;


		public abstract IEnumerable<TDataType> ReadDataIntoMemory();


		public IEnumerable<TDataType> LoadDataIntoDatabase(IEnumerable<TDataType> payload)
		{
			if (ActualRecordCount == 0)
			{
				//If no entities have been loaded, load them into the database
				_repository
					.PersistEntities
					(
						payload,
						KeySelector
					);
			}
			else if (ActualRecordCount != ExpectedRecordCount)
			{
				//If the number of entities does not match the expected value 
				//for the data loader, truncate the table before reloading
				_repository.DeleteEntities<TDataType>(entity => true);

				_repository
					.PersistEntities
					(
						payload,
						KeySelector
					);
			}

			//After persisting entities (or not) retrieve and 
			//return them from the underlying database context
			return
				_repository
					.RetrieveEntities(RecordRetrievalPredicate);
		}
		#endregion



		#region Non-Public Method(s)
		protected void AddRequestHeader(string key, string value)
		{
			if (_dataLoaderClient != null)
			{
				_dataLoaderClient.DefaultRequestHeaders.Add(key, value);
			}
			else
			{
				throw new NotSupportedException($"{ADD_REQUEST_HEADER_FAILURE}  {HTTP_CLIENT_NOT_SPECIFIED}");
			}
		}


		protected async Task<HttpResponseMessage> GetRemoteContentAsync(string relativePath)
		{
			if (_dataLoaderClient == null)
			{
				throw new NotSupportedException($"{REMOTE_CONTENT_ACCESS_FAILURE}  {HTTP_CLIENT_NOT_SPECIFIED}");
			}

			try
			{
				HttpResponseMessage response = await _dataLoaderClient.GetAsyncWithAutoRetry(relativePath, 10);
				response.EnsureSuccessStatusCode();

				return response;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}


		protected IEnumerable<string> LoadDataFile(string filePath)
		{
			return File.ReadAllLines(filePath);
		}
		#endregion
	}
}
