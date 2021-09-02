using System;
using System.Collections.Generic;
using System.Text;

namespace TGH.Common.Patterns.IoC
{
	/// <summary>
	///		Defines the relative lifetime of a service registered
	///		in the <seealso cref="DependencyManager"/>.
	/// </summary>
	/// <remarks>
	///		Services with a shorter lifetime can depend on
	///		those with a longer lifetime, but not vice-versa.
	/// </remarks>
	public enum ServiceScope
	{
		/// <summary>
		///		Services with a 'Volatile' scope will provide a new 
		///		instance each time one is requested from the container.
		/// </summary>
		Volatile,

		/// <summary>
		///		Services with a 'Managed' scope will continue to provide the same instance until  
		///		a call to <seealso cref="DependencyManager.RecycleManagedServices"/> is made.
		/// </summary>
		Managed,

		/// <summary>
		///		Services with a 'Singleton' scope will return the 
		///		same instance for the lifetime of the program.
		/// </summary>
		Singleton
	}
}
