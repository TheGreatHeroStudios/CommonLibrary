using System;
using System.Collections.Generic;
using System.Text;

namespace TGH.Common.Repository.Interfaces
{
	public interface IGenericRepository
	{
		/// <summary>
		///		Returns the number of entities of a given
		///		<typeparamref name="TEntityType"/> currently 
		///		stored in the underlying database context.
		/// </summary>
		/// <typeparam name="TEntityType">
		///		The type of entity for which to retrieve a count.
		/// </typeparam>
		/// <returns>
		///		The number of <typeparamref name="TEntityType"/>s
		///		currently stored in the underlying database context.
		/// </returns>
		int GetRecordCount<TEntityType>()
			where TEntityType : class;


		/// <summary>
		///		Returns the number of entities of a given
		///		<typeparamref name="TEntityType"/> currently 
		///		stored in the underlying database context
		///		matching the supplied <paramref name="predicate"/>.
		/// </summary>
		/// <typeparam name="TEntityType">
		///		The type of entity for which to retrieve a count.
		/// </typeparam>
		/// <param name="predicate">
		///		A function to be applied to all items in the
		///		underlying database set to determine if they 
		///		should be included in the final count.
		/// </param>
		/// <returns>
		///		The number of <typeparamref name="TEntityType"/>s
		///		currently stored in the underlying database context.
		/// </returns>
		int GetRecordCount<TEntityType>(Func<TEntityType, bool> predicate)
			where TEntityType : class;


		/// <summary>
		///		Retrieves a set of entities from the database
		///		matching the supplied <paramref name="predicate"/>
		/// </summary>
		/// <typeparam name="TEntityType">
		///		The type of entity to retrieve
		/// </typeparam>
		/// <param name="predicate">
		///		A function to be applied to each item in the database
		///		to determine whether or not it should be retrieved.
		/// </param>
		/// <returns>
		///		A collection of <typeparamref name="TEntityType"/>
		///		whose items match the specified <paramref name="predicate"/>
		/// </returns>
		IEnumerable<TEntityType> RetrieveEntities<TEntityType>
		(
			Func<TEntityType, bool> predicate
		)
			where TEntityType : class;


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


		/// <summary>
		///		Deletes entities from the underlying database context
		///		matching a given <paramref name="predicate"/>.
		/// </summary>
		/// <typeparam name="TEntityType">
		///		The type of entity being deleted.
		/// </typeparam>
		/// <param name="predicate">
		///		The function which will be applied to each item
		///		in the underlying database context to determine
		///		whether or not it should be deleted.
		/// </param>
		/// <param name="deferCommit">
		///		When set to 'true', changes made to the underlying
		///		context are not automatically persisted to the database
		///		when the method completes.  By default, any deletions  
		///		of entities on the context are automatically persisted
		///		to the database when the method finishes executing.
		/// </param>
		/// <returns>
		///		The number of entities that were deleted.
		/// </returns>
		int DeleteEntities<TEntityType>(Func<TEntityType, bool> predicate)
			where TEntityType : class;
	}
}
