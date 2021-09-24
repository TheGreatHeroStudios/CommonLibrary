using System;
using System.Collections.Generic;
using TGH.Common.DataStructures;
using TGH.Common.DataStructures.Enums;
using TGH.Common.DataStructuresTests.Scaffolding;
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
		}
		#endregion



		#region Test Method(s)
		[Fact]
		public void TestConstructTree()
		{
			//Act: Create a BalancedBinaryTree from the list of test data
			BalancedBinaryTree<TestItem> testTree = 
				new BalancedBinaryTree<TestItem>(_testItems, SearchMethod.DepthFirst);

			//Assert:


		}
		#endregion
	}
}
