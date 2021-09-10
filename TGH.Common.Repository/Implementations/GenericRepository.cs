using System;
using System.Collections.Generic;
using System.Text;
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
		public int RecordCount<TEntityType>()
			where TEntityType : class
		{
			return
				_context.RecordCount<TEntityType>();
		}


		public int RecordCount<TEntityType>(Func<TEntityType, bool> predicate)
			where TEntityType : class
		{
			return
				_context.RecordCount(predicate);
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
				.DividePayload
				(
					initialPayload,
					keySelector,
					out addPayload,
					out updatePayload
				);

			//Add the new entities to the underlying context
			int persistedEntityCount = _context.Add(addPayload, true);

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
