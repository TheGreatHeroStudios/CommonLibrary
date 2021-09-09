using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TGH.Common.Extensions
{
	public static class LinqExtensions
	{
		public static bool In<TItem>(this TItem item, IEnumerable<TItem> collection)
		{
			return
				collection.Contains(item);
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
	}
}
