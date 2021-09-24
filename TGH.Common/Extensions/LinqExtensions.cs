using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TGH.Common.Extensions
{
	public static class LinqExtensions
	{
		public static void Add<TKey, TValue>
		(
			this Dictionary<TKey, TValue> dictionary,
			KeyValuePair<TKey, TValue> newEntry
		)
		{
			if(!dictionary.ContainsKey(newEntry.Key))
			{
				dictionary.Add(newEntry.Key, newEntry.Value);
			}
		}


		public static string Delimit<TItem>
		(
			this IEnumerable<TItem> collection,
			string delimiter
		)
		{
			return
				collection
					.Aggregate
					(
						new StringBuilder(),
						(builder, item) =>
						{
							builder.Append(item.ToString());
							builder.Append(delimiter);
							return builder;
						},
						builder =>
						{
							string result = builder.ToString();
							return result.Substring(0, result.LastIndexOf(delimiter));
						}
					);
		}


		public static bool In<TItem>(this TItem item, IEnumerable<TItem> collection)
		{
			return
				collection.Contains(item);
		}


		public static IEnumerable<TResult> LeftAntiJoin<TLeft, TRight, TKey, TResult>
		(
			this IEnumerable<TLeft> left,
			IEnumerable<TRight> right,
			Func<TLeft, TKey> leftKeySelector,
			Func<TRight, TKey> rightKeySelector,
			Func<TLeft, TResult> resultSelector
		)
			where TResult : class
		{
			return
				left
					.LeftJoin
					(
						right,
						leftKeySelector,
						rightKeySelector,
						(left, right) =>
							right == null ?
								resultSelector(left) :
								null
					)
					.Where
					(
						result =>
							result != null
					);
		}


		public static IEnumerable<TResult> LeftJoin<TLeft, TRight, TKey, TResult>
		(
			this IEnumerable<TLeft> left,
			IEnumerable<TRight> right,
			Func<TLeft, TKey> leftKeySelector,
			Func<TRight, TKey> rightKeySelector,
			Func<TLeft, TRight, TResult> resultSelector
		)
		{
			return
				left
					.GroupJoin
					(
						right,
						leftKeySelector,
						rightKeySelector,
						(leftItem, rightGroup) =>
							new { leftItem, rightGroup }
					)
					.SelectMany
					(
						joinedResult => joinedResult.rightGroup.DefaultIfEmpty(),
						(joinedResult, rightItem) =>
							resultSelector(joinedResult.leftItem, rightItem)
					);
		}


		public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>
		(
			this IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs
		)
		{
			return
				keyValuePairs
					.ToDictionary
					(
						kvp => kvp.Key,
						kvp => kvp.Value
					);
		}
	}
}
