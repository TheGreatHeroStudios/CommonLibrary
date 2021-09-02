using System;
using System.Collections.Generic;
using System.Text;

namespace TGH.Common.PatternsTests.Scaffolding
{
	public class TestClassWithDependency : ITestInterfaceWithDependency
	{
		#region Non-Public Member(s)
		private ITestInterface1 _testDependency;
		#endregion



		#region Constructor(s)
		public TestClassWithDependency(ITestInterface1 dependency)
		{
			_testDependency = dependency;
		}
		#endregion
	}
}
