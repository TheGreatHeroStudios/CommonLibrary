using System;
using System.Collections.Generic;
using System.Text;

namespace TGH.Common.Persistence.Interfaces
{
	public interface IDatabaseContext
	{
		/// <summary>
		///		Divides an <paramref name="initialPayload"/> of
		///		entities to be persisted into two sets--one to be
		///		newly added to the database, the other to be updated.
		/// </summary>
		/// <typeparam name="TEntityType">
		///		The type of entity being persisted
		/// </typeparam>
		/// <typeparam name="TKeyType">
		///		The type of the entity's primary key
		/// </typeparam>
		/// <param name="initialPayload">
		///		The initial (unsorted) payload of entities to be persisted.
		/// </param>
		/// <param name="keySelector">
		///		A function applied to each <typeparamref name="TEntityType"/>
		///		in the initial input payload to select its primary key value.
		/// </param>
		/// <param name="addPayload">
		///		The output collection of entities to be added to the database.
		/// </param>
		/// <param name="updatePayload">
		///		The output collection of entities to be updated in the database.
		/// </param>
		void DividePayload<TEntityType, TKeyType>
		(
			IEnumerable<TEntityType> initialPayload,
			Func<TEntityType, TKeyType> keySelector,
			out IEnumerable<TEntityType> addPayload,
			out IEnumerable<TEntityType> updatePayload
		)
			where TEntityType : class
			where TKeyType : struct;


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
		int Count<TEntityType>()
			where TEntityType : class;


		/// <summary>
		///		Adds a set of newly created 
		///		<typeparamref name="TEntityType"/>s
		///		to the underlying database context
		/// </summary>
		/// <typeparam name="TEntityType">
		///		The entity type being added.
		/// </typeparam>
		/// <param name="payload">
		///		The collection of entities to add.
		/// </param>
		/// <param name="deferCommit">
		///		When set to 'true', changes made to the underlying
		///		context are not automatically persisted to the database
		///		when the method completes.  By default, any entities 
		///		added to the context are automatically persisted
		///		to the database when the method finishes executing.
		/// </param>
		/// <returns>
		///		The number of entities added.
		/// </returns>
		int Add<TEntityType>
		(
			IEnumerable<TEntityType> payload,
			bool deferCommit = false
		)
			where TEntityType : class;


		/// <summary>
		///		Updates a set of pre-existing 
		///		<typeparamref name="TEntityType"/>s
		///		on the underlying database context
		/// </summary>
		/// <typeparam name="TEntityType">
		///		The entity type being updated.
		/// </typeparam>
		/// <param name="initialPayload">
		///		The collection of entities to update.
		/// </param>
		/// <param name="deferCommit">
		///		When set to 'true', changes made to the underlying
		///		context are not automatically persisted to the database
		///		when the method completes.  By default, any changes made 
		///		to entities on the context are automatically persisted
		///		to the database when the method finishes executing.
		/// </param>
		/// <returns>
		///		The number of entities updated.
		/// </returns>
		int Update<TEntityType, TKeyType>
		(
			IEnumerable<TEntityType> initialPayload,
			Func<TEntityType, TKeyType> keySelector,
			bool deferCommit = false
		)
			where TEntityType : class
			where TKeyType : struct;


		/// <summary>
		///		Commits any outstanding changes made to the context
		///		whaich have not yet been persisted to the database.
		/// </summary>
		void CommitChanges();
	}
}
