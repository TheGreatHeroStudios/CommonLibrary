using System;
using System.Collections.Generic;
using TGH.Common.DataStructures;
using TGH.Common.DataStructures.Enums;
using TGH.Common.DataStructuresTests.Scaffolding;
using TGH.Common.Extensions;
using Xunit;

namespace TGH.Common.DataStructuresTests
{
	public class BalancedBinaryTreeTests
	{
		#region Non-Public Member(s)
		private List<TestItem> _testItems;
		#endregion



		#region Constructor(s)
		public BalancedBinaryTreeTests()
		{
			//Arrange
			_testItems =
				new List<TestItem>
				{
					new TestItem(1),
					new TestItem(2),
					new TestItem(3),
					new TestItem(4),
					new TestItem(5),
					new TestItem(6),
					new TestItem(7),
					new TestItem(8),
					new TestItem(9),
					new TestItem(10),
					new TestItem(11),
					new TestItem(12),
					new TestItem(13),
					new TestItem(14),
					new TestItem(15)
				};

			/*
				When converted to a balanced binary tree, a continuous,  
				sorted collection of integers should be organized like so:

										  1
									    /	 \
			                           /	  \
									  /		   \
									 /			\
									/			 \
								   /			  \
								  /				   \
								2					3
							 /	   \			 /	   \
							/		\			/		\
						   4		 6		   5		 7
						 /   \     /   \     /   \     /   \
						8	 12	 10		14	9	 13	 11	    15
			 */
		}
		#endregion



		#region Test Method(s)
		[Fact]
		public void TestConstructDepthFirstTree()
		{
			//Act: Create a BalancedBinaryTree from the list of test data
			BalancedBinaryTree<TestItem> testTree = 
				new BalancedBinaryTree<TestItem>(_testItems, SearchMethod.DepthFirst);

			//Enumerate the tree and aggregate a string representing the results.
			string actualResult = testTree.Delimit(",");


			//Assert: Assert that the items in the tree
			//enumerated in the expected (depth first) order
			string expectedResult = "1,2,4,8,12,6,10,14,3,5,9,13,7,11,15";

			Assert.Equal(expectedResult, actualResult);
		}


		[Fact]
		public void TestConstructBreadthFirstTree()
		{
			//Act: Create a BalancedBinaryTree from the list of test data
			BalancedBinaryTree<TestItem> testTree =
				new BalancedBinaryTree<TestItem>(_testItems);

			//Enumerate the tree and aggregate a string representing the results.
			string actualResult = testTree.Delimit(",");


			//Assert: Assert that the items in the tree
			//enumerated in the expected (breadth first) order
			string expectedResult = "1,2,3,4,6,8,12,10,14,5,7,9,13,11,15";

			Assert.Equal(expectedResult, actualResult);
		}


		[Theory]
		[InlineData(1, 0)]
		[InlineData(12, 4)]
		[InlineData(9, 10)]
		[InlineData(15, 14)]
		public void TestEnumerateDepthFirst(int testData, int expectedOrdinal)
		{
			//Arrange
			TestItem testItem = new TestItem(testData);
			int actualOrdinal = 0;


			//Act: Create a BalancedBinaryTree from the list of test data
			BalancedBinaryTree<TestItem> testTree =
				new BalancedBinaryTree<TestItem>(_testItems, SearchMethod.DepthFirst);

			//Iterate over the tree, incrementing the
			//ordinal until the specified item is found
			foreach(TestItem item in testTree)
			{
				if(item.Equals(testItem))
				{
					break;
				}

				actualOrdinal++;
			}


			//Assert: Assert that the item was found at the expected ordinal
			Assert.Equal(expectedOrdinal, actualOrdinal);
		}



		[Theory]
		[InlineData(1, 0)]
		[InlineData(12, 6)]
		[InlineData(9, 11)]
		[InlineData(15, 14)]
		public void TestEnumerateBreadthFirst(int testData, int expectedOrdinal)
		{
			//Arrange
			TestItem testItem = new TestItem(testData);
			int actualOrdinal = 0;


			//Act: Create a BalancedBinaryTree from the list of test data
			BalancedBinaryTree<TestItem> testTree =
				new BalancedBinaryTree<TestItem>(_testItems);

			//Iterate over the tree, incrementing the
			//ordinal until the specified item is found
			foreach (TestItem item in testTree)
			{
				if (item.Equals(testItem))
				{
					break;
				}

				actualOrdinal++;
			}


			//Assert: Assert that the item was found at the expected ordinal
			Assert.Equal(expectedOrdinal, actualOrdinal);
		}


		[Theory]
		[InlineData(1, true)]
		[InlineData(10, true)]
		[InlineData(15, true)]
		[InlineData(21, false)]
		[InlineData(100, false)]
		public void TestFindItemDepthFirst(int testData, bool expectedFoundInTree)
		{
			//Arrange
			TestItem testItem = new TestItem(testData);


			//Act: Create a BalancedBinaryTree from the list of test data
			BalancedBinaryTree<TestItem> testTree =
				new BalancedBinaryTree<TestItem>(_testItems, SearchMethod.DepthFirst);

			//Determine whether the item could be located in the tree
			bool actuallyFoundInTree = testTree.Find(testItem) != null;


			//Assert
			Assert.Equal(expectedFoundInTree, actuallyFoundInTree);
		}


		[Theory]
		[InlineData(1, true)]
		[InlineData(10, true)]
		[InlineData(15, true)]
		[InlineData(21, false)]
		[InlineData(100, false)]
		public void TestFindItemBreadthFirst(int testData, bool expectedFoundInTree)
		{
			//Arrange
			TestItem testItem = new TestItem(testData);


			//Act: Create a BalancedBinaryTree from the list of test data
			BalancedBinaryTree<TestItem> testTree =
				new BalancedBinaryTree<TestItem>(_testItems);

			//Determine whether the item could be located in the tree
			bool actuallyFoundInTree = testTree.Find(testItem) != null;


			//Assert
			Assert.Equal(expectedFoundInTree, actuallyFoundInTree);
		}
		#endregion
	}
}
