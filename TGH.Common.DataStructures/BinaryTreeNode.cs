using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TGH.Common.DataStructures.Enums;

namespace TGH.Common.DataStructures
{
	public class BinaryTreeNode<TItemType>
		where TItemType : class
	{
		#region Constructor(s)
		public BinaryTreeNode()
		{
		}


		public BinaryTreeNode(IEnumerable<TItemType> items)
		{
			BuildChildNodes(items);
		}
		#endregion



		#region Public Propertie(s)
		public TItemType Data { get; set; }
		public BinaryTreeNode<TItemType> LeftChildNode { get; set; }
		public BinaryTreeNode<TItemType> RightChildNode { get; set; }
		public bool IsLeafNode => 
			LeftChildNode == null && RightChildNode == null;
		#endregion



		#region Public Method(s)
		public void Add(TItemType item)
		{
			if (Data == null)
			{
				//If data for the current node has not yet 
				//been set, assign the current item to it.
				Data = item;
			}
			else if (LeftChildNode == null)
			{
				//If data has been set for the current node,
				//but not for the left child, initialize the left
				//child node and set the data to the current item.
				LeftChildNode = new BinaryTreeNode<TItemType>();
				LeftChildNode.Data = item;
			}
			else if (RightChildNode == null)
			{
				//If data has been set for the current node,
				//but not for the right child, initialize the right
				//child node and set the data to the current item.
				RightChildNode = new BinaryTreeNode<TItemType>();
				RightChildNode.Data = item;
			}
			else if (LeftChildNode.GetChildNodeCount() <= RightChildNode.GetChildNodeCount())
			{
				//If both child nodes are set, but the left node  
				//has fewer child nodes (or the tree is currently 
				//balanced), add the item to the left child node.
				LeftChildNode.Add(item);
			}
			else
			{
				//If both child nodes are set and the left child 
				//node has more items than the right child node, 
				//add the item to the right child node.
				RightChildNode.Add(item);
			}
		}


		public IEnumerator<TItemType> GetNodeEnumerator(SearchMethod searchMethod)
		{
			//Get enumerators for both child nodes (if they are initialized)
			IEnumerator<TItemType> leftNodeEnumerator =
					LeftChildNode?.GetNodeEnumerator(searchMethod);

			IEnumerator<TItemType> rightNodeEnumerator =
					RightChildNode?.GetNodeEnumerator(searchMethod);

			if (searchMethod == SearchMethod.BreadthFirst)
			{
				//When enumerating in a "breadth first" fashion, yield data for the
				//left node and right node (respectively) before enumerating child nodes
				if(LeftChildNode?.Data != null)
				{
					yield return LeftChildNode.Data;
				}

				if (RightChildNode?.Data != null)
				{
					yield return RightChildNode.Data;
				}

				if(leftNodeEnumerator != null)
				{
					while (leftNodeEnumerator.MoveNext())
					{
						yield return leftNodeEnumerator.Current;
					}
				}

				if (rightNodeEnumerator != null)
				{
					while (rightNodeEnumerator.MoveNext())
					{
						yield return rightNodeEnumerator.Current;
					}
				}
			}
			else
			{
				//When enumerating in a "depth first" fashion, yield data 
				//for all left nodes first, before enumerating right nodes
				if (LeftChildNode?.Data != null)
				{
					yield return LeftChildNode.Data;
				}

				if (leftNodeEnumerator != null)
				{
					while (leftNodeEnumerator.MoveNext())
					{
						yield return leftNodeEnumerator.Current;
					}
				}

				if (RightChildNode?.Data != null)
				{
					yield return RightChildNode.Data;
				}

				if (rightNodeEnumerator != null)
				{
					while (rightNodeEnumerator.MoveNext())
					{
						yield return rightNodeEnumerator.Current;
					}
				}
			}
		}


		public TItemType Find(TItemType item)
		{
			TItemType foundItem = null;

			if (Data.Equals(item))
			{
				//If the node's direct data is the item
				//being searched for, return it directly.
				foundItem = Data;
			}
			else
			{
				//Otherwise, search for the item across child nodes
				if(LeftChildNode?.Data?.Equals(item) ?? false)
				{
					foundItem = LeftChildNode.Data;
				}
				else if(RightChildNode?.Data?.Equals(item) ?? false)
				{
					foundItem = RightChildNode.Data;
				}
				else if(LeftChildNode != null)
				{
					//If neither child node contains the target item,
					//begin searching the grandchildren of the current node
					foundItem = LeftChildNode.Find(item);

					if(foundItem == null && RightChildNode != null)
					{
						foundItem = RightChildNode.Find(item);
					}
				}
			}

			return foundItem;
		}


		public int GetChildNodeCount()
		{
			int currentCount = 0;

			if (LeftChildNode != null)
			{
				currentCount++;
				currentCount += LeftChildNode.GetChildNodeCount();
			}

			if (RightChildNode != null)
			{
				currentCount++;
				currentCount += RightChildNode.GetChildNodeCount();
			}

			return currentCount;
		}
		#endregion



		#region Non-Public Method(s)
		private void BuildChildNodes(IEnumerable<TItemType> items)
		{
			//While there are items left to build nodes for...
			while (items?.Any() ?? false)
			{
				//Remove the item from the input collection and
				//determine how it should be added to the node
				TItemType item = items.First();
				items = items.Skip(1);

				//Add the item to the node (or one of its child nodes)
				Add(item);
			}
		}
		#endregion
	}
}
