using System;
using System.Collections.Generic;
using System.Text;

namespace TGH.Common.Repository.Interfaces
{
	public interface IGenericRepository
	{
		/// <summary>
		///		Saves a collection of <typeparamref name="TEntityType"/>
		///		to the underlying database context.
		/// </summary>
		/// <typeparam name="TEntityType">
		///		The type of entity being persisted.
		/// </typeparam>
		/// <typeparam name="TKeyType">
		///		The type of the entity's primary key
		/// </typeparam>
		/// <param name="initialPayload">
		///		The collection of entities being persisted.
		/// </param>
		/// <param name="keySelector">
		///		A function applied to each <typeparamref name="TEntityType"/>
		///		in the initial input payload to select its primary key value.
		/// </param>
		/// <returns>
		///		A count of the number of rows in the database affected.
		/// </returns>
		int PersistEntities<TEntityType, TKeyType>
		(
			IEnumerable<TEntityType> initialPayload,
			Func<TEntityType, TKeyType> keySelector

		)
			where TEntityType : class
			where TKeyType : struct;
	}
}
