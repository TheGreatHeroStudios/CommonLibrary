using System;
using System.Collections.Generic;
using System.Text;

namespace TGH.Common.DataStructuresTests.Scaffolding
{
	public class TestItem
	{
		#region Constructor(s)
		public TestItem(int testProperty)
		{
			TestProperty = testProperty;
		}
		#endregion



		#region Public Propertie(s)
		public int TestProperty { get; set; }
		#endregion



		#region Override(s)
		public override string ToString()
		{
			return TestProperty.ToString();
		}
		#endregion
	}
}
