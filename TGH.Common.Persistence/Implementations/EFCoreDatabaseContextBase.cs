using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using TGH.Common.Extensions;
using TGH.Common.Persistence.Interfaces;

namespace TGH.Common.Persistence.Implementations
{
	public abstract class EFCoreDatabaseContextBase : DbContext, IDatabaseContext
	{
		#region 'IDatabaseContext' Implementation
		public void DividePayload<TEntityType, TKeyType>
		(
			IEnumerable<TEntityType> initialPayload, 
			Func<TEntityType, TKeyType> keySelector, 
			out IEnumerable<TEntityType> addPayload, 
			out IEnumerable<TEntityType> updatePayload
		)
			where TEntityType : class
			where TKeyType : struct
		{
			//Extract a set of keys from the input collection to serve as the filter query
			IEnumerable<TKeyType> payloadKeys =
				initialPayload.Select(payloadItem => keySelector(payloadItem));

			//Join the extracted keys with the queryable to create a filtering context
			IEnumerable<TEntityType> trackedEntities =
				Set<TEntityType>()
					.Join
					(
						payloadKeys,
						keySelector,
						payloadKey => payloadKey,
						(trackedEntity, payloadKey) => trackedEntity
					)
					.AsEnumerable();

			//Set the 'add' payload to all those items from the initial payload
			//EXCEPT those which were retrieved from the underlying context.
			addPayload =
				initialPayload
					.LeftAntiJoin
					(
						trackedEntities,
						initialEntity => keySelector(initialEntity),
						trackedEntity => keySelector(trackedEntity),
						initialEntity => initialEntity
					);

			//Set the 'update' payload to all those items from the initial 
			//payload which WERE retrieved from the underlying context.
			updatePayload =
				initialPayload
					.Join
					(
						trackedEntities,
						initialEntity => keySelector(initialEntity),
						trackedEntity => keySelector(trackedEntity),
						(initialEntity, trackedEntity) => initialEntity
					);
		}


		public int Count<TEntityType>()
			where TEntityType : class
		{
			return
				Set<TEntityType>().Count();
		}


		public int Add<TEntityType>
		(
			IEnumerable<TEntityType> initialPayload, 
			bool deferCommit = false
		) 
			where TEntityType : class
		{
			AddRange(initialPayload);

			if(!deferCommit)
			{
				CommitChanges();
			}

			return initialPayload.Count();
		}


		public int Update<TEntityType, TKeyType>
		(
			IEnumerable<TEntityType> initialPayload,
			Func<TEntityType, TKeyType> keySelector,
			bool deferCommit = false
		) 
			where TEntityType : class
			where TKeyType : struct
		{
			//Split the update payload into entities already tracked by 
			//the context (that need to be memberwise copied), and those
			//not currently tracked (which must be attached explicitly).
			IEnumerable<TEntityType> trackedEntities = Set<TEntityType>().Local;

			IEnumerable<(TEntityType untracked, TEntityType tracked)> copyPayload =
				initialPayload
					.Join
					(
						trackedEntities,
						payloadEntity => keySelector(payloadEntity),
						trackedEntity => keySelector(trackedEntity),
						(payloadEntity, trackedEntity) => (payloadEntity, trackedEntity)
					);

			//Iterate over the entities requiring a memberwise copy, and copy its 
			//properties from the untracked payload entity to the tracked entity
			foreach(var (untracked, tracked) in copyPayload)
			{
				CopyEntityProperties(untracked, tracked);
			}

			IEnumerable<TEntityType> updatePayload =
				initialPayload
					.LeftAntiJoin
					(
						trackedEntities,
						payloadEntity => keySelector(payloadEntity),
						trackedEntity => keySelector(trackedEntity),
						payloadEntity => payloadEntity
					);

			//For those entities requiring an update that are not currently tracked 
			//locally, simply attach them to the context with an 'Updated' state.
			UpdateRange(updatePayload);

			if (!deferCommit)
			{
				CommitChanges();
			}

			return initialPayload.Count();
		}


		public void CommitChanges()
		{
			SaveChanges();
		}
		#endregion



		#region Non-Public Method(s)
		private void CopyEntityProperties<TEntityType>
		(
			TEntityType untrackedEntity, 
			TEntityType trackedEntity
		)
		{
			//Determine the properties of the entity type which should 
			//be copied by extracting the public instance properties 
			//for value types with an exposed public setter and getter.
			IEnumerable<PropertyInfo> copyProperties =
				typeof(TEntityType)
					.GetProperties(BindingFlags.Public | BindingFlags.Instance)
					.Where
					(
						prop =>
							prop.PropertyType.IsPrimitiveOrNullablePrimitive() &&
							prop.GetSetMethod() != null &&
							prop.GetGetMethod() != null
					);

			//Copy the value of each 'copyable' property
			//from the untracked entity to the tracked entity
			foreach(PropertyInfo property in copyProperties)
			{
				property.SetValue(trackedEntity, property.GetValue(untrackedEntity));
			}
		}
		#endregion
	}
}
