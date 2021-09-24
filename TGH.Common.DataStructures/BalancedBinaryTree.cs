using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TGH.Common.DataStructures.Enums;

namespace TGH.Common.DataStructures
{
	public class BalancedBinaryTree<TItemType> : IEnumerable<TItemType>
		where TItemType : class
	{
		#region Non-Public Member(s)
		private BinaryTreeNode<TItemType> _rootNode;
		private SearchMethod _searchMethod;
		#endregion



		#region Constructor(s)
		public BalancedBinaryTree
		(
			SearchMethod searchMethod = SearchMethod.BreadthFirst
		)
		{
			//Initialize an empty root node
			_rootNode = new BinaryTreeNode<TItemType>();

			_searchMethod = searchMethod;
		}


		public BalancedBinaryTree
		(
			IEnumerable<TItemType> items,
			SearchMethod searchMethod = SearchMethod.BreadthFirst
		)
		{
			//Iterate over the collection and build the tree for it
			_rootNode = new BinaryTreeNode<TItemType>(items);

			_searchMethod = searchMethod;
		}
		#endregion



		#region 'IEnumerable' Implementation
		public IEnumerator<TItemType> GetEnumerator()
		{
			return GetTreeEnumerator();
		}


		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetTreeEnumerator();
		}
		#endregion



		#region Public Method(s)
		public void Add(TItemType item)
		{
			_rootNode.Add(item);
		}


		public TItemType Find(TItemType item)
		{
			if(item == null)
			{
				throw new ArgumentNullException(nameof(item));
			}


			//Iterate over the tree (based on preferred search order) until the 
			//specified item has been found or the collection has been exhausted.
			foreach (TItemType iteratedItem in this)
			{
				if(iteratedItem.Equals(item))
				{
					return iteratedItem;
				}
			}

			return null;
		}
		#endregion



		#region Non-Public Method(s)
		private IEnumerator<TItemType> GetTreeEnumerator()
		{
			if (_rootNode.Data != null)
			{
				//Yield the data of the current node first
				yield return _rootNode.Data;
			}

			//Then yield the data for each child node
			IEnumerator<TItemType> nodeEnumerator = 
				_rootNode.GetNodeEnumerator(_searchMethod);

			while(nodeEnumerator.MoveNext())
			{
				yield return nodeEnumerator.Current;
			}
		}
		#endregion
	}
}
