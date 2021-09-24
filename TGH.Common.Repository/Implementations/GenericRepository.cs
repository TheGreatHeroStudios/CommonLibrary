using System;
using System.Collections.Generic;
using TGH.Common.Persistence.Interfaces;
using TGH.Common.Repository.Interfaces;

namespace TGH.Common.Repository.Implementations
{
	public class GenericRepository : IGenericRepository
	{
		#region Non-Public Member(s)
		private IDatabaseContext _context;
		#endregion



		#region Constructor(s)
		public GenericRepository(IDatabaseContext context)
		{
			_context = context;
		}
		#endregion



		#region 'IGenericRepository' Implementation
		public int GetRecordCount<TEntityType>()
			where TEntityType : class
		{
			return
				_context.Count<TEntityType>();
		}


		public int GetRecordCount<TEntityType>(Func<TEntityType, bool> predicate)
			where TEntityType : class
		{
			return
				_context.Count(predicate);
		}


		public IEnumerable<TEntityType> RetrieveEntities<TEntityType>
		(
			Func<TEntityType, bool> predicate
		)
			where TEntityType : class
		{
			return
				_context.Read(predicate);
		}


		public int PersistEntities<TEntityType, TKeyType>
		(
			IEnumerable<TEntityType> initialPayload,
			Func<TEntityType, TKeyType> keySelector

		)
			where TEntityType : class
			where TKeyType : struct
		{
			IEnumerable<TEntityType> addPayload;
			IEnumerable<TEntityType> updatePayload;

			//Divide the initial payload into those
			//entities to be added and those to be updated
			_context
				.DerivePersistPayloads
				(
					initialPayload,
					keySelector,
					out addPayload,
					out updatePayload
				);

			//Add the new entities to the underlying context
			int persistedEntityCount = _context.Create(addPayload, true);

			//Update existing entities on the underlying context
			persistedEntityCount += _context.Update(updatePayload, keySelector, true);

			_context.CommitChanges();

			return persistedEntityCount;
		}


		public int DeleteEntities<TEntityType>(Func<TEntityType, bool> predicate)
			where TEntityType : class
		{
			return
				_context.Delete(predicate, false);
		}
		#endregion
	}
}
