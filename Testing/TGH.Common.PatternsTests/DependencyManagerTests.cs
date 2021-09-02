using System;
using TGH.Common.Patterns.IoC;
using TGH.Common.PatternsTests.Scaffolding;
using Xunit;

namespace TGH.Common.PatternsTests
{
	public class DependencyManagerTests
	{

		#region Class-Specific Constant(s)
		private const string DUPLICATE_REGISTRATION_ERROR =
			"The type 'ITestInterface1' has already been registered to resolve an instance " +
			"of type 'TestClass1'.  To register multiple resolution types for the same registered " +
			"type, supply a different type argument for 'TResolutionType' when calling 'RegisterService'.";

		private const string UNASSIGNABLE_REGISTRATION_ERROR =
			"Could not register type 'TestClass2' as a resolution type for type 'ITestInterface1' because " +
			"instances of type 'TestClass2' are not assignable to variables of type 'ITestInterface1'.";

		private const string UNREGISTERED_DEPENDENCY_ERROR =
			"The type 'TestClassWithDependency' being registered depends on type 'ITestInterface1' " +
			"which is not registered in the container.  Please add a registration for type " +
			"'ITestInterface1' with a scope of 'Volatile' or greater.";

		private const string OVERLOADED_DEPENDENCY_ERROR =
			"The resolution type 'TestClassWithDependency' being registered depends on type 'ITestInterface1' " +
			"which has multiple resolution types registered for it.  By default, this is not supported " +
			"for automatic dependency resolution.  To fix this, either remove the overloaded registrations " +
			"for type 'ITestInterface1', or set the 'DependencyManager.AutoResolveOverloadedDependencies' " +
			"flag to 'true'.";

		private const string DEPENDENCY_SHORTER_LIFETIME_ERROR =
			"The type 'TestClassWithDependency' depends on type 'ITestInterface1' whose registration(s) all " +
			"have a shorter lifetime.  To register the type, ensure that all dependent types are registered " +
			"with a scope of 'Managed' or greater or lower the scope of 'TestClassWithDependency' " +
			"(to ensure proper runtime resolution).";

		private const string NON_OVERWRITABLE_STRATEGY_ERROR =
			"The resolution type 'TestClass1' being registered already has a resolution strategy set for " +
			"registration type 'ITestInterface1' which can't be overwritten.  To override this behavior, " +
			"set the parameter 'overwriteExisting' to 'true' when calling 'RegisterService'";

		private const string MULTI_CONSTRUCTOR_ERROR =
			"The type 'MultiCtorClass1' could not be registered because it defines more than " +
			"one constructor, making its construction too ambiguous for automatic dependency " +
			"resolution.  To register this type, use the overload of 'RegisterService' which " +
			"supports the 'resolutionStrategy' parameter.";

		private const string INVALID_CONSTRUCTOR_ERROR =
			"The type 'InvalidCtorClass1' cannot be registered because it depends on a primitive " +
			"value type (Int32) which is not valid for dependency resolution.  To register this type, " +
			"use the overload of 'RegisterService' which supports the 'resolutionStrategy' parameter.";

		private const string SERVICE_NOT_REGISTERED_ERROR =
			"The requested type 'ITestInterface1' is not registered in the container.";

		private const string RESOLUTION_TYPES_UNSUITABLE_ERROR =
			"The requested type 'ITestInterface1' is registered, but none of its possible " +
			"resolution type(s) are suitable due to incompatible lifetime scopes.";

		private const string EXPLICIT_SERVICE_NOT_REGISTERED_ERROR =
			"The specific resolution type 'TestClass1' is not registered to be resolved " +
			"for type 'ITestInterface1'";

		private const string EXPLICIT_RESOLUTION_TYPE_UNSUITABLE_ERROR =
			"The specific resolution type 'TestClass1' is registered to be resolved for " +
			"type 'ITestInterface1', but is not suitable due to an incompatible lifetime scope.";
		#endregion



		#region Positive Test Case(s)
		[Fact]
		public void TestRegisterService()
		{
			//Arrange
			DependencyManager.ClearRegisteredServices();


			//Act: Register test service and its dependenc(ies)
			DependencyManager.RegisterService<ITestInterface1, TestClass1>(ServiceScope.Singleton);

			DependencyManager
				.RegisterService<ITestInterfaceWithDependency, TestClassWithDependency>
				(
					ServiceScope.Singleton
				);


			//Assert: Services registered successfully
			Assert.Equal(2, DependencyManager.RegisteredServiceCount);
		}


		[Fact]
		public void TestRegisterServiceWithStrategy()
		{
			//Arrange
			DependencyManager.ClearRegisteredServices();


			//Act: Register service with dependency using explicit strategy
			DependencyManager
				.RegisterService<ITestInterfaceWithDependency, TestClassWithDependency>
				(
					() => new TestClassWithDependency(new TestClass1()),
					ServiceScope.Singleton
				);

			//Assert: Service is registered sucessfully
			Assert.Equal(1, DependencyManager.RegisteredServiceCount);
		}


		[Fact]
		public void TestRegisterMultiConstructorServiceWithStrategy()
		{
			//Arrange
			DependencyManager.ClearRegisteredServices();


			//Act: Register service with dependency using explicit strategy
			DependencyManager
				.RegisterService<ITestInterfaceWithDependency, TestClassWithDependency>
				(
					() => new TestClassWithDependency(new TestClass1()),
					ServiceScope.Singleton
				);

			//Assert: Service is registered sucessfully
			Assert.Equal(1, DependencyManager.RegisteredServiceCount);
		}


		[Theory]
		[InlineData(ServiceScope.Volatile)]
		[InlineData(ServiceScope.Managed)]
		[InlineData(ServiceScope.Singleton)]
		public void TestResolveService(ServiceScope scope)
		{
			//Arrange: Register a service and its dependenc(ies)
			DependencyManager.ClearRegisteredServices();

			DependencyManager.RegisterService<ITestInterface1, TestClass1>(scope);

			DependencyManager
				.RegisterService<ITestInterfaceWithDependency, TestClassWithDependency>
				(
					scope
				);


			//Act: Attempt to resolve the service from the container
			ITestInterfaceWithDependency testService =
				DependencyManager.ResolveService<ITestInterfaceWithDependency>(scope) 
					as ITestInterfaceWithDependency;


			//Assert: The service is successfully resolved.
			Assert.NotNull(testService);
		}


		[Theory]
		[InlineData(ServiceScope.Volatile)]
		[InlineData(ServiceScope.Managed)]
		[InlineData(ServiceScope.Singleton)]
		public void TestResolveWithOverloadedDependency(ServiceScope scope)
		{
			//Arrange: Register a service and multiple overloads of its dependenc(ies)
			DependencyManager.ClearRegisteredServices();

			DependencyManager.RegisterService<ITestInterface1, TestClass1>(scope);
			DependencyManager.RegisterService<ITestInterface1, TestClass1Alternate>(scope);

			//Set the 'AutoResolveOverloadedDependencies' flag to 'true'
			DependencyManager.AutoResolveOverloadedDependencies = true;

			DependencyManager
				.RegisterService<ITestInterfaceWithDependency, TestClassWithDependency>
				(
					scope
				);


			//Act: Attempt to resolve the service from the container
			ITestInterfaceWithDependency testService =
				DependencyManager.ResolveService<ITestInterfaceWithDependency>(scope)
					as ITestInterfaceWithDependency;


			//Assert: The service is successfully resolved.
			Assert.NotNull(testService);
		}


		[Theory]
		[InlineData(ServiceScope.Volatile)]
		[InlineData(ServiceScope.Managed)]
		[InlineData(ServiceScope.Singleton)]
		public void TestResolveWithStrategy(ServiceScope scope)
		{
			//Arrange: Register a service using a strategy
			DependencyManager.ClearRegisteredServices();

			DependencyManager
				.RegisterService<ITestInterfaceWithDependency, TestClassWithDependency>
				(
					() =>
						new TestClassWithDependency
						(
							new TestClass1()
						),
					scope
				);


			//Act: Attempt to resolve the service from the container
			ITestInterfaceWithDependency testService =
				DependencyManager.ResolveService<ITestInterfaceWithDependency>(scope)
					as ITestInterfaceWithDependency;


			//Assert: The service is successfully resolved.
			Assert.NotNull(testService);
		}


		[Theory]
		[InlineData(ServiceScope.Volatile)]
		[InlineData(ServiceScope.Managed)]
		[InlineData(ServiceScope.Singleton)]
		public void TestResolveWithDependencyStrategy(ServiceScope scope)
		{
			//Arrange: Register a service and its dependency using a strategy
			DependencyManager.ClearRegisteredServices();

			DependencyManager
				.RegisterService<ITestInterface1, TestClass1>
				(
					() => new TestClass1(),
					scope
				);

			DependencyManager
				.RegisterService<ITestInterfaceWithDependency, TestClassWithDependency>(scope);


			//Act: Attempt to resolve the service from the container
			ITestInterfaceWithDependency testService =
				DependencyManager.ResolveService<ITestInterfaceWithDependency>(scope)
					as ITestInterfaceWithDependency;


			//Assert: The service is successfully resolved.
			Assert.NotNull(testService);
		}


		[Theory]
		[InlineData(ServiceScope.Volatile)]
		[InlineData(ServiceScope.Managed)]
		[InlineData(ServiceScope.Singleton)]
		public void TestResolveExplicitService(ServiceScope scope)
		{
			//Arrange: Register a service and its dependenc(ies)
			DependencyManager.ClearRegisteredServices();

			DependencyManager.RegisterService<ITestInterface1, TestClass1>(scope);

			DependencyManager
				.RegisterService<ITestInterfaceWithDependency, TestClassWithDependency>
				(
					scope
				);


			//Act: Attempt to resolve the explicit service from the container
			TestClassWithDependency testService =
				DependencyManager
					.ResolveService<ITestInterfaceWithDependency, TestClassWithDependency>(scope);


			//Assert: The service is successfully resolved.
			Assert.NotNull(testService);
		}


		[Theory]
		[InlineData(ServiceScope.Volatile)]
		[InlineData(ServiceScope.Managed)]
		[InlineData(ServiceScope.Singleton)]
		public void TestResolveExplicitWithOverloadedDependencies(ServiceScope scope)
		{
			//Arrange: Register a service and multiple overloads of its dependenc(ies)
			DependencyManager.ClearRegisteredServices();

			DependencyManager.RegisterService<ITestInterface1, TestClass1>(scope);
			DependencyManager.RegisterService<ITestInterface1, TestClass1Alternate>(scope);

			//Set the 'AutoResolveOverloadedDependencies' flag to 'true'
			DependencyManager.AutoResolveOverloadedDependencies = true;

			DependencyManager
				.RegisterService<ITestInterfaceWithDependency, TestClassWithDependency>
				(
					scope
				);


			//Act: Attempt to resolve the explicit service from the container
			TestClassWithDependency testService =
				DependencyManager
					.ResolveService<ITestInterfaceWithDependency, TestClassWithDependency>(scope);


			//Assert: The service is successfully resolved.
			Assert.NotNull(testService);
		}


		[Theory]
		[InlineData(ServiceScope.Volatile)]
		[InlineData(ServiceScope.Managed)]
		[InlineData(ServiceScope.Singleton)]
		public void TestResolveExplicitServiceWithStrategy(ServiceScope scope)
		{
			//Arrange: Register a service using a strategy
			DependencyManager.ClearRegisteredServices();

			DependencyManager
				.RegisterService<ITestInterfaceWithDependency, TestClassWithDependency>
				(
					() =>
						new TestClassWithDependency
						(
							new TestClass1()
						),
					scope
				);


			//Act: Attempt to resolve the explicit service from the container
			TestClassWithDependency testService =
				DependencyManager
					.ResolveService<ITestInterfaceWithDependency, TestClassWithDependency>(scope);


			//Assert: The service is successfully resolved.
			Assert.NotNull(testService);
		}


		[Theory]
		[InlineData(ServiceScope.Volatile)]
		[InlineData(ServiceScope.Managed)]
		[InlineData(ServiceScope.Singleton)]
		public void TestResolveExplicitServiceWithDependencyStrategy(ServiceScope scope)
		{
			//Arrange: Register a service and its dependency using a strategy
			DependencyManager.ClearRegisteredServices();

			DependencyManager
				.RegisterService<ITestInterface1, TestClass1>
				(
					() => new TestClass1(),
					scope
				);

			DependencyManager
				.RegisterService<ITestInterfaceWithDependency, TestClassWithDependency>(scope);


			//Act: Attempt to resolve the explicit service from the container
			TestClassWithDependency testService =
				DependencyManager
					.ResolveService<ITestInterfaceWithDependency, TestClassWithDependency>(scope);


			//Assert: The service is successfully resolved.
			Assert.NotNull(testService);
		}
		
		
		[Fact]
		public void TestRecycleManagedServices()
		{
			//Arrange
			DependencyManager.ClearRegisteredServices();

			DependencyManager.RegisterService<ITestInterface1, TestClass1>(ServiceScope.Managed);
			DependencyManager.RegisterService<ITestInterface2, TestClass2>(ServiceScope.Managed);


			//Act: Request both registered services to cache them.
			DependencyManager.ResolveService<ITestInterface1>();
			DependencyManager.ResolveService<ITestInterface2>();


			//Assert: Assert that two managed instances have been cached
			Assert.Equal(2, DependencyManager.CachedInstanceCount);


			//Act: Recycle managed services
			DependencyManager.RecycleManagedServices();


			//Assert: Assert that the two managed instances have been invalidated.
			Assert.Equal(0, DependencyManager.CachedInstanceCount);
		}
		#endregion



		#region Negative Test Case(s)
		[Fact]
		public void TestRegistrationWithMultipleConstructors()
		{
			//Arrange
			DependencyManager.ClearRegisteredServices();


			//Act: Attempt to Register a type with multiple constructors
			ArgumentException argex =
				Assert.Throws<ArgumentException>
				(
					() =>
						DependencyManager.RegisterService<ITestInterface1, MultiCtorClass1>()
				);

			//Assert: Registration throws the expected error message
			Assert.Equal(MULTI_CONSTRUCTOR_ERROR, argex.Message);
		}


		[Fact]
		public void TestRegistrationWithInvalidConstructorParameters()
		{
			//Arrange
			DependencyManager.ClearRegisteredServices();


			//Act: Attempt to Register a type with invalid constructor parameters
			ArgumentException argex =
				Assert.Throws<ArgumentException>
				(
					() =>
						DependencyManager.RegisterService<ITestInterface1, InvalidCtorClass1>()
				);

			//Assert: Registration throws the expected error message
			Assert.Equal(INVALID_CONSTRUCTOR_ERROR, argex.Message);
		}


		[Fact]
		public void TestDuplicateRegistration()
		{
			//Arrange
			DependencyManager.ClearRegisteredServices();

			//Act
			DependencyManager.RegisterService<ITestInterface1, TestClass1>();

			//Assert
			ArgumentException argex =
				Assert.Throws<ArgumentException>
				(
					() =>
						DependencyManager.RegisterService<ITestInterface1, TestClass1>()
				);

			Assert.Equal(DUPLICATE_REGISTRATION_ERROR, argex.Message);
		}


		[Fact]
		public void TestUnassignableRegistration()
		{
			//Arrange
			DependencyManager.ClearRegisteredServices();

			//Act
			ArgumentException argex =
				Assert.Throws<ArgumentException>
				(
					//Assert
					() =>
						DependencyManager.RegisterService<ITestInterface1, TestClass2>()
				);

			Assert.Equal(UNASSIGNABLE_REGISTRATION_ERROR, argex.Message);
		}


		[Fact]
		public void TestUnregisteredDependency()
		{
			//Arrange
			DependencyManager.ClearRegisteredServices();

			//Act
			ArgumentException argex =
				Assert.Throws<ArgumentException>
				(
					//Assert
					() =>
						DependencyManager
							.RegisterService<ITestInterfaceWithDependency, TestClassWithDependency>()
				);

			Assert.Equal(UNREGISTERED_DEPENDENCY_ERROR, argex.Message);
		}


		[Fact]
		public void TestOverloadedDependencyRegistrations()
		{
			//Arrange
			DependencyManager.ClearRegisteredServices();

			//Act: Register overloaded dependencies
			DependencyManager.RegisterService<ITestInterface1, TestClass1>();
			DependencyManager.RegisterService<ITestInterface1, TestClass1Alternate>();

			//Assert: By default, overloaded dependencies throw argument exception
			ArgumentException argex =
				Assert.Throws<ArgumentException>
				(
					() =>
						DependencyManager
							.RegisterService<ITestInterfaceWithDependency, TestClassWithDependency>()
				);

			Assert.Equal(OVERLOADED_DEPENDENCY_ERROR, argex.Message);
			

			//Act: Update flag for auto-resolution of overloaded dependencies
			DependencyManager.AutoResolveOverloadedDependencies = true;

			//Assert: Assert than overloaded dependencies do not cause exception to
			//be thrown after setting the 'AutoResolveOverloadedDependencies' flag.
			DependencyManager
				.RegisterService<ITestInterfaceWithDependency, TestClassWithDependency>();

			Assert.Equal(3, DependencyManager.RegisteredServiceCount);
		}


		[Fact]
		public void TestInvalidDependencyLifetime()
		{
			//Arrange
			DependencyManager.ClearRegisteredServices();

			//Act: Register dependency with default 'Volatile' lifetime
			DependencyManager.RegisterService<ITestInterface1, TestClass1>();

			//Assert: Registering dependent class with longer lifetime throws exception
			ArgumentException argex =
				Assert.Throws<ArgumentException>
				(
					() =>
						DependencyManager
							.RegisterService<ITestInterfaceWithDependency, TestClassWithDependency>
							(
								ServiceScope.Managed
							)
				);

			Assert.Equal(DEPENDENCY_SHORTER_LIFETIME_ERROR, argex.Message);

			//Act: Register dependent class with same (default) lifetime as dependency
			DependencyManager.RegisterService<ITestInterfaceWithDependency, TestClassWithDependency>();

			//Assert: Dependent class is registered without error
			Assert.Equal(2, DependencyManager.RegisteredServiceCount);
		}


		[Fact]
		public void TestNonOverwritableStrategy()
		{
			//Arrange
			DependencyManager.ClearRegisteredServices();


			//Act: Create a strategy for resolving an instance of 'TestClass1'
			DependencyManager.RegisterService<ITestInterface1, TestClass1>
			(
				() => null
			);


			//Assert: Attempting to overwrite the registered 
			//strategy with a new one throws an 'ArgumentException'
			ArgumentException argex =
				Assert.Throws<ArgumentException>
				(
					() =>
						DependencyManager
							.RegisterService<ITestInterface1, TestClass1>
							(
								() => new TestClass1()
							)
				);

			Assert.Equal(NON_OVERWRITABLE_STRATEGY_ERROR, argex.Message);

			
			//Act: Overwrite the strategy by specifying 'true' for 'overwriteExisting' flag
			DependencyManager
				.RegisterService<ITestInterface1, TestClass1>
				(
					() => new TestClass1(),
					overwriteExisting: true
				);


			//Assert: Overwriting succeeds when 'overwriteExisting' is set to true
			Assert.Equal(1, DependencyManager.RegisteredServiceCount);
		}


		[Fact]
		public void TestResolutionOfUnregisteredService()
		{
			//Arrange
			DependencyManager.ClearRegisteredServices();

			//Act
			ArgumentException argex =
				Assert.Throws<ArgumentException>
				(
					() =>
						DependencyManager.ResolveService<ITestInterface1>()
				);

			//Assert
			Assert.Equal(SERVICE_NOT_REGISTERED_ERROR, argex.Message);
		}


		[Fact]
		public void TestResolutionWithNoSuitableRegistrations()
		{
			//Arrange
			DependencyManager.ClearRegisteredServices();
			DependencyManager.RegisterService<ITestInterface1, TestClass1>();

			//Act
			ArgumentException argex =
				Assert.Throws<ArgumentException>
				(
					() =>
						DependencyManager.ResolveService<ITestInterface1>(ServiceScope.Singleton)
				);

			//Assert
			Assert.Equal(RESOLUTION_TYPES_UNSUITABLE_ERROR, argex.Message);
		}


		[Fact]
		public void TestExplicitResolutionOfUnregisteredService()
		{
			//Arrange
			DependencyManager.ClearRegisteredServices();
			DependencyManager.RegisterService<ITestInterface1, TestClass1Alternate>();

			//Act
			ArgumentException argex =
				Assert.Throws<ArgumentException>
				(
					() =>
						DependencyManager.ResolveService<ITestInterface1, TestClass1>()
				);

			//Assert
			Assert.Equal(EXPLICIT_SERVICE_NOT_REGISTERED_ERROR, argex.Message);
		}


		[Fact]
		public void TestExplicitResolutionWithNoSuitableRegistrations()
		{
			//Arrange
			DependencyManager.ClearRegisteredServices();
			DependencyManager.RegisterService<ITestInterface1, TestClass1>();

			//Act
			ArgumentException argex =
				Assert.Throws<ArgumentException>
				(
					() =>
						DependencyManager.ResolveService<ITestInterface1, TestClass1>
						(
							ServiceScope.Singleton
						)
				);

			//Assert
			Assert.Equal(EXPLICIT_RESOLUTION_TYPE_UNSUITABLE_ERROR, argex.Message);
		}
		#endregion
	}
}
