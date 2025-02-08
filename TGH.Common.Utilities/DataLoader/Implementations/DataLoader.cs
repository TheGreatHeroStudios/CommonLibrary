using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TGH.Common.Patterns.IoC;
using TGH.Common.Persistence.Interfaces;
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

		protected IDatabaseContext _context;
		#endregion



		#region Constructor(s)
		public DataLoader()
		{
			_context =
				DependencyManager.ResolveService<IDatabaseContext>();
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

			_context = DependencyManager.ResolveService<IDatabaseContext>();
		}


		public DataLoader(HttpClient client)
		{
			_dataLoaderClient = client;

			_context = DependencyManager.ResolveService<IDatabaseContext>();
		}
		#endregion



		#region Interface Implementations
		public int ActuaRecordCount
		{
			get
			{
				if (!_actualRecordCount.HasValue)
				{
					//If the record count from the database has not
					//yet been checked, check it and cache the result
					_actualRecordCount =
						_context.Count(RecordRetrievalPredicate);
				}

				return _actualRecordCount.Value;
			}
		}

		public virtual int ExpectedRecordCount => 0;

		public abstract Func<TDataType, int> KeySelector { get; }

		public virtual Func<TDataType, bool> RecordRetrievalPredicate => entity => true;


		public abstract IEnumerable<TDataType> ReadDataIntoMemory();


		public void StageDataForInsert(IEnumerable<TDataType> payload, bool deferCommit = false)
		{
			if (ActuaRecordCount == 0)
			{
				//If no entities have been loaded, load them into the database
				_context
					.Create
					(
						payload,
						deferCommit
					);
			}
			else if (ActuaRecordCount != ExpectedRecordCount)
			{
				//If the number of entities does not match the expected value 
				//for the data loader, truncate the table before reloading
				_context.Delete<TDataType>(entity => true, deferCommit);

				_context
					.Create
					(
						payload,
						deferCommit
					);
			}
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
			catch (Exception)
			{
				throw;
			}
		}


		protected IEnumerable<string> LoadDataFile(string filePath)
		{
			return File.ReadAllLines(filePath);
		}
		#endregion
	}
}
