using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TGH.Common.Extensions;

namespace TGH.Common.Patterns.IoC
{
	/// <summary>
	///		Exposes static methods for interacting with an IoC container (for solutions 
	///		that don't have one built-in).  Allows the registration of services and their
	///		dependencies for which instances can be automatically resolved at runtime.
	/// </summary>
	public class DependencyManager
	{
		#region Class-Specific Constant(s)
		private const string ERROR_OVERLOADED_CONSTRUCTOR =
			"The type '{0}' could not be registered because it defines more than one constructor, " +
			"making its construction too ambiguous for automatic dependency resolution.  To register " +
			"this type, use the overload of '{1}' which supports the 'resolutionStrategy' parameter.";

		private const string ERROR_TYPE_ALREADY_REGISTERED =
			"The type '{0}' has already been registered to resolve an instance of type '{1}'.  " +
			"To register multiple resolution types for the same registered type, supply a " +
			"different type argument for '{2}' when calling '{3}'.";

		private const string ERROR_TYPE_NOT_ASSIGNABLE =
			"Could not register type '{0}' as a resolution type for type '{1}' because instances of " +
			"type '{0}' are not assignable to variables of type '{1}'.";

		private const string ERROR_DEPENDENCY_NOT_REGISTERED =
			"The type '{0}' being registered depends on type '{1}' which is not registered " +
			"in the container.  Please add a registration for type '{1}' with a scope of " +
			"'{2}' or greater.";

		private const string ERROR_PRIMITIVE_DEPENDENCY =
			"The type '{0}' cannot be registered because it depends on a primitive value type ({1}) " +
			"which is not valid for dependency resolution.  To register this type, use the overload " +
			"of '{2}' which supports the 'resolutionStrategy' parameter.";

		private const string ERROR_OVERLOADED_DEPENDENCY =
			"The resolution type '{0}' being registered depends on type '{1}' which has multiple resolution " +
			"types registered for it.  By default, this is not supported for automatic dependency " +
			"resolution.  To fix this, either remove the overloaded registrations for type '{1}', " +
			"or set the '{2}.{3}' flag to 'true'.";

		private const string ERROR_DEPENDENCY_SHORTER_SCOPE =
			"The type '{0}' depends on type '{1}' whose registration(s) all have a shorter lifetime.  " +
			"To register the type, ensure that all dependent types are registered with a scope " +
			"of '{2}' or greater or lower the scope of '{0}' (to ensure proper runtime resolution).";

		private const string ERROR_STRATEGY_NOT_OVERWRITABLE =
			"The resolution type '{0}' being registered already has a resolution strategy set for " +
			"registration type '{1}' which can't be overwritten.  To override this behavior, set the " +
			"parameter '{2}' to 'true' when calling '{3}'";

		private const string ERROR_TYPE_NOT_REGISTERED =
			"The requested type '{0}' is not registered in the container.";

		private const string ERROR_NO_RESOLUTION_TYPE_SUITABLE =
			"The requested type '{0}' is registered, but none of its possible resolution type(s) " +
			"are suitable due to incompatible lifetime scopes.";

		private const string ERROR_SPECIFIC_TYPE_NOT_REGISTERED =
			"The specific resolution type '{0}' is not registered to be resolved for type '{1}'";

		private const string ERROR_SPECIFIC_TYPE_NOT_SUITABLE =
			"The specific resolution type '{0}' is registered to be resolved for type '{1}', " +
			"but is not suitable due to an incompatible lifetime scope.";
		#endregion



		#region Non-Public Member(s)
		private static Dictionary<Type, Dictionary<Type, ServiceScope>> _registeredServices;
		private static Dictionary<(Type registrationType, Type resolutionType), Func<object>> _resolutionStrategies;
		private static Dictionary<(Type registrationType, Type resolutionType), object> _cachedInstances;
		#endregion



		#region Static Constructor
		static DependencyManager()
		{
			_registeredServices =
				new Dictionary<Type, Dictionary<Type, ServiceScope>>();

			_resolutionStrategies =
				new Dictionary<(Type registrationType, Type resolutionType), Func<object>>();

			_cachedInstances = new Dictionary<(Type registrationType, Type resolutionType), object>();
		}
		#endregion



		#region Propertie(s)
		/// <summary>
		///		Determines whether the <seealso cref="DependencyManager"/> should
		///		automatically resolve dependencies of registered types that have 
		///		more than one type which can be resolved for them.
		/// </summary>
		/// <remarks>
		///		When set to 'true', dependency resolution for types with more than
		///		one registration will use the first registration available.<para/>
		///		When set to 'false', registration of types with such dependencies
		///		will fail, and resolution for any previously registered types that
		///		fit this category will also fail.
		/// </remarks>
		public static bool AutoResolveOverloadedDependencies { get; set; }


		/// <summary>
		///		Gets a count of the number of services and their 
		///		unique resolution type(s) registered in the container.
		/// </summary>
		public static int RegisteredServiceCount =>
			_registeredServices
				.Sum
				(
					registration =>
						registration.Value.Count
				);


		/// <summary>
		///		Gets a count of the valid instances (both managed and singleton)
		///		which are currently cached for re-use within the container.
		/// </summary>
		public static int CachedInstanceCount =>
			_cachedInstances
				.Where(cachedInstance => cachedInstance.Value != null)
				.Count();


		/// <summary>
		///		Gets a collection of tuples representing each managed service registered
		///		within the container and their corresponding resolution types.
		/// </summary>
		public static IEnumerable<(Type registrationType, Type resolvedType)> ManagedRegistrations =>
			_registeredServices
				.SelectMany
				(
					registeredService => registeredService.Value,
					(registeredService, resolutionType) =>
						resolutionType.Value == ServiceScope.Managed ?
							(
								registrationType: registeredService.Key,
								resolvedType: resolutionType.Key
							) :
							default
				)
				.Where
				(
					registrationTuple =>
						registrationTuple.registrationType != null
				);
		#endregion



		#region Public Method(s)
		/// <summary>
		///		Determines whether a specified <typeparamref name="TRegistrationType"/>
		///		is registered in the container.
		/// </summary>
		/// <typeparam name="TRegistrationType">
		///		The type to look for in the container.
		/// </typeparam>
		/// <returns>
		///		'True' if the specified type is registered 
		///		in the container, otherwise 'False'.
		/// </returns>
		public static bool HasRegistration<TRegistrationType>()
		{
			return
				_registeredServices.ContainsKey(typeof(TRegistrationType));
		}


		/// <summary>
		///		Determines whether a specified <typeparamref name="TRegistrationType"/>
		///		is registered in the container to resolve a type of <typeparamref name="TResolutionType"/>.
		/// </summary>
		/// <typeparam name="TRegistrationType">
		///		The registration type to look for in the container.
		/// </typeparam>
		/// <typeparam name="TResolutionType">
		///		The type which should be resolvable for the 
		///		provided <typeparamref name="TRegistrationType"/>
		/// </typeparam>
		/// <returns>
		///		'True' if the specified type is registered in the container and is capable of 
		///		resolving an instance of type <typeparamref name="TResolutionType"/>, otherwise 'False'.
		/// </returns>
		public static bool HasRegistration<TRegistrationType, TResolutionType>()
		{
			return
				_registeredServices.ContainsKey(typeof(TRegistrationType)) &&
				_registeredServices[typeof(TRegistrationType)].ContainsKey(typeof(TResolutionType));
		}


		/// <summary>
		///		Adds a registration for <typeparamref name="TRegistrationType"/> to
		///		be resolved as an instance of <typeparamref name="TResolutionType"/> wherever
		///		a dependency of type <typeparamref name="TRegistrationType"/> is required.
		/// </summary>
		/// <typeparam name="TRegistrationType">
		///		The type which serves as the key for dependency resolution.
		///		This is typically an interface type, but can be any reference type.
		/// </typeparam>
		/// <typeparam name="TResolutionType">
		///		The type which will be resolved whenever an instance of type
		///		<typeparamref name="TRegistrationType"/> is requested.
		/// </typeparam>
		/// <param name="scope">
		///		The <seealso cref="ServiceScope"/> used for the type registration.
		///		This controls the relative lifetime for instances of the registered service.
		/// </param>
		public static void RegisterService<TRegistrationType, TResolutionType>
		(
			ServiceScope scope = ServiceScope.Volatile
		)
			where TRegistrationType : class
			where TResolutionType : class
		{
			//Validate the type being registered
			ValidateRegistrationType<TRegistrationType, TResolutionType>();

			//Then, validate that each of the resolution type's direct 
			//dependencies are properly registered in the container
			ValidateResolutionTypeDependencies<TResolutionType>(scope);

			//Once all dependencies of the type being registered 
			//have been validated, register the service type itself.
			RegisterServiceTypes<TRegistrationType, TResolutionType>(scope);
		}


		/// <summary>
		///		Adds a registration for <typeparamref name="TRegistrationType"/> which
		///		uses the supplied <paramref name="resolutionStrategy"/> to resolve an
		///		instance of type <typeparamref name="TResolutionType"/> wherever
		///		a dependency of type <typeparamref name="TRegistrationType"/> is required.
		/// </summary>
		/// <typeparam name="TRegistrationType">
		///		The type which serves as the key for dependency resolution.
		///		This is typically an interface type, but can be any reference type.
		/// </typeparam>
		/// <typeparam name="TResolutionType">
		///		The type which will be resolved whenever an instance of type
		///		<typeparamref name="TRegistrationType"/> is requested.
		/// </typeparam>
		/// <param name="resolutionStrategy">
		///		An action which uses the <seealso cref="DependencyManager"/>
		///		and defines the code necessary for resolving an instance of
		///		type <typeparamref name="TResolutionType"/>.
		/// </param>
		/// <param name="scope">
		///		The <seealso cref="ServiceScope"/> used for the type registration.
		///		This controls the relative lifetime for instances of the registered service.
		/// </param>
		/// <param name="overwriteExisting">
		///		Specifies whether supplying a new <paramref name="resolutionStrategy"/>
		///		for an existing service registration should overwrite the previous
		///		strategy.  <para/>
		///		By default, attempting to overwrite an existing strategy
		///		will cause an <seealso cref="ArgumentException"/> to be thrown.
		/// </param>
		/// <remarks>
		///		Registering a service in this manner bypasses resolution type validations
		///		due to the fact that target type resolution is handled by the user-supplied 
		///		<paramref name="resolutionStrategy"/>.
		/// </remarks>
		public static void RegisterService<TRegistrationType, TResolutionType>
		(
			Func<TResolutionType> resolutionStrategy,
			ServiceScope scope = ServiceScope.Volatile,
			bool overwriteExisting = false
		)
			where TRegistrationType : class
			where TResolutionType : class
		{
			//Validate the type being registered
			ValidateRegistrationType<TRegistrationType, TResolutionType>(true);

			//Register a strategy for resolution of a specific type for a specific registration type.
			(Type registrationType, Type resolutionType) registrationKeyTuple =
				(typeof(TRegistrationType), typeof(TResolutionType));

			if (!_resolutionStrategies.ContainsKey(registrationKeyTuple))
			{
				//If the resolution strategy dictionary does not yet contain a strategy 
				//for the registration and resolution tuple being registered, add it.
				_resolutionStrategies.Add(registrationKeyTuple, resolutionStrategy);
			}
			else if (overwriteExisting)
			{
				//If the resolution strategy does already exist, but can be overwritten, update it.
				_resolutionStrategies[registrationKeyTuple] = resolutionStrategy;
			}
			else
			{
				//If the resolution strategy already exists but 
				//can't be overwritten, throw an ArgumentException
				throw new ArgumentException
				(
					string.Format
					(
						ERROR_STRATEGY_NOT_OVERWRITABLE,
						typeof(TResolutionType).Name,
						typeof(TRegistrationType).Name,
						nameof(overwriteExisting),
						nameof(RegisterService)
					)
				);
			}

			//Finally, register the service type for which a strategy is defined
			RegisterServiceTypes<TRegistrationType, TResolutionType>(scope);
		}


		/// <summary>
		///		Resolves a service instance for the supplied 
		///		<typeparamref name="TRegistrationType"/> from the container.
		/// </summary>
		/// <remarks>
		///		This overload resolves the best match for the supplied <typeparamref name="TRegistrationType"/>.
		///		In instances where multiple resolution types are registered, the caller may want to make use of the 
		///		<seealso cref="ResolveService{TRegistrationType, TResolutionType}"/> overload to explicitly specify
		///		which resolution type should be retrieved from the container.
		/// </remarks>
		/// <typeparam name="TRegistrationType">
		///		The registered service type for which a concrete instance should be resolved.
		/// </typeparam>
		/// <returns>
		///		A concrete instance of a service deriving from (or implementing)
		///		<typeparamref name="TRegistrationType"/> resolved from the container.
		/// </returns>
		public static object ResolveService<TRegistrationType>
		(
			ServiceScope minimumScope = ServiceScope.Volatile
		)
			where TRegistrationType : class
		{
			//Resolve the service registration which will be used to resolve an instance
			KeyValuePair<Type, ServiceScope> resolvedRegistration =
				ResolveServiceRegistration<TRegistrationType>(minimumScope);

			(Type registeredType, Type resolutionType) registrationTuple =
				(typeof(TRegistrationType), resolvedRegistration.Key);

			//Based on the scope of the resolved registration, determine whether
			//to resolve a new instance, or retrieve one from the cache.
			object serviceInstance = ResolveServiceInstance(registrationTuple, resolvedRegistration.Value);

			return serviceInstance;
		}


		/// <summary>
		///		Resolves an instance of type <typeparamref name="TResolutionType"/> registered 
		///		for the supplied <typeparamref name="TRegistrationType"/> from the container.
		/// </summary>
		/// <remarks>
		///		This overload will attempt to resolve the specified <typeparamref name="TResolutionType"/>
		///		as a resolution type for the supplied <typeparamref name="TRegistrationType"/>.
		///		If resolution of this specific type fails, the method will throw a <seealso cref="ArgumentException"/>
		///		In instances where a specific resolution type is not important, the caller may want to make use of the 
		///		<seealso cref="ResolveService{TRegistrationType}"/> overload to allow the manager to resolve the
		///		best <typeparamref name="TResolutionType"/> automatically.
		/// </remarks>
		/// <typeparam name="TRegistrationType">
		///		The registered service type for which an instance of type
		///		<typeparamref name="TResolutionType"/> should be resolved.
		/// </typeparam>
		/// <typeparam name="TResolutionType">
		///		The concrete type which should be resolved for the 
		///		supplied <typeparamref name="TRegistrationType"/>.
		/// </typeparam>
		/// <returns>
		///		A concrete instance of <typeparamref name="TResolutionType"/> resolved from the container.
		/// </returns>
		public static TResolutionType ResolveService<TRegistrationType, TResolutionType>
		(
			ServiceScope minimumScope = ServiceScope.Volatile
		)
			where TRegistrationType : class
			where TResolutionType : class
		{
			//Resolve the service registration which will be used to resolve an instance
			KeyValuePair<Type, ServiceScope> resolvedRegistration =
				ResolveServiceRegistration<TRegistrationType>(minimumScope, typeof(TResolutionType));

			(Type registeredType, Type resolutionType) registrationTuple =
				(typeof(TRegistrationType), resolvedRegistration.Key);

			//Based on the scope of the resolved registration, determine whether
			//to resolve a new instance, or retrieve one from the cache.
			TResolutionType serviceInstance =
				ResolveServiceInstance(registrationTuple, resolvedRegistration.Value) as TResolutionType;

			return serviceInstance;
		}


		/// <summary>
		///		Recycles any services with a scope of <seealso cref="ServiceScope.Managed"/>
		///		such that the next time an instance is requested, a new one will be generated.
		/// </summary>
		public static void RecycleManagedServices()
		{
			//Get a collection of service registration tuples for any services registered
			//with a 'Managed' lifetime and join them with the collection of cached instances
			//to determine which services should be re-created the next time they're requested.
			List<(Type registrationType, Type resolutionType)> managedCacheKeys =
				_cachedInstances
					.Join
					(
						ManagedRegistrations,
						cachedInstance => cachedInstance.Key,
						managedRegistration => managedRegistration,
						(cachedInstance, managedRegistration) => cachedInstance.Key
					)
					.ToList();

			for (int i = managedCacheKeys.Count - 1; i >= 0; i--)
			{
				//Iterate over the managed services in the cache and invalidate each one
				_cachedInstances[managedCacheKeys[i]] = null;
			}
		}


		/// <summary>
		///		Removes all registered service types and the types used for resolution.
		/// </summary>
		public static void ClearRegisteredServices()
		{
			_registeredServices.Clear();
			_cachedInstances.Clear();
			_resolutionStrategies.Clear();
		}
		#endregion




		#region Non-Public Method(s)
		private static void ValidateRegistrationType<TRegistrationType, TResolutionType>
		(
			bool ignoreDuplicateRegistration = false
		)
			where TRegistrationType : class
			where TResolutionType : class
		{
			if
			(
				_registeredServices.ContainsKey(typeof(TRegistrationType)) &&
				_registeredServices[typeof(TRegistrationType)].ContainsKey(typeof(TResolutionType)) &&
				!ignoreDuplicateRegistration
			)
			{
				//First, validate that the type being registered isn't already
				//registered in the container with the same resolution type
				//(or duplicate registrations are ignored).
				throw new ArgumentException
				(
					string.Format
					(
						ERROR_TYPE_ALREADY_REGISTERED,
						typeof(TRegistrationType).Name,
						typeof(TResolutionType).Name,
						nameof(TResolutionType),
						nameof(RegisterService)
					)
				);
			}
			else if (!typeof(TResolutionType).IsAssignableTo(typeof(TRegistrationType)))
			{
				//Also, validate that an instance of the proposed resolution 
				//type can be assigned to a variable of the registration type
				throw new ArgumentException
				(
					string.Format
					(
						ERROR_TYPE_NOT_ASSIGNABLE,
						typeof(TResolutionType).Name,
						typeof(TRegistrationType).Name
					)
				);
			}
		}


		private static void ValidateResolutionTypeDependencies<TResolutionType>
		(
			ServiceScope dependentTypeScope
		)
			where TResolutionType : class
		{
			ConstructorInfo[] serviceConstructors = typeof(TResolutionType).GetConstructors();

			//Validate that only one public constructor exists for the type being registered
			if (serviceConstructors.Length > 1)
			{
				throw new ArgumentException
				(
					string.Format
					(
						ERROR_OVERLOADED_CONSTRUCTOR,
						typeof(TResolutionType).Name,
						nameof(RegisterService)
					)
				);
			}

			IEnumerable<ParameterInfo> constructorParams =
				serviceConstructors
					.SelectMany(ctor => ctor.GetParameters());

			foreach (ParameterInfo param in constructorParams)
			{
				//Validate that each constructor parameter is registered in the container
				if
				(
					!_registeredServices.ContainsKey(param.ParameterType) ||
					!_registeredServices[param.ParameterType].Any()
				)
				{
					//If the constructor parameter is not registered, determine an error
					//message for the exception based on the type of the parameter...
					string errorMessage;

					if (param.ParameterType.IsPrimitiveOrNullablePrimitive())
					{
						//If the parameter is a primitive type, format the error to
						//inform the caller that primitive types can't be registered.
						errorMessage =
							string.Format
							(
								ERROR_PRIMITIVE_DEPENDENCY,
								typeof(TResolutionType).Name,
								param.ParameterType.Name,
								nameof(RegisterService)
							);
					}
					else
					{
						//Otherwise, simply inform the caller that the dependency is not registered.
						errorMessage =
							string.Format
							(
								ERROR_DEPENDENCY_NOT_REGISTERED,
								typeof(TResolutionType).Name,
								param.ParameterType.Name,
								dependentTypeScope
							);
					}

					throw new ArgumentException(errorMessage);
				}
				else if
				(
					!AutoResolveOverloadedDependencies &&
					_registeredServices[param.ParameterType].Count > 1
				)
				{
					//Validate that the parameter type is not registered with
					//multiple resolution types OR that the DependencyManager 
					//has been configured to auto-resolve duplicate registrations
					throw new ArgumentException
					(
						string.Format
						(
							ERROR_OVERLOADED_DEPENDENCY,
							typeof(TResolutionType).Name,
							param.ParameterType.Name,
							nameof(DependencyManager),
							nameof(AutoResolveOverloadedDependencies)
						)
					);
				}
				else if
				(
					!_registeredServices[param.ParameterType]
						.Any(registration => registration.Value >= dependentTypeScope)
				)
				{
					//Validate that the parameter's type registration contains either 
					//the same or a longer lifetime than that of the one being registered.
					throw new ArgumentException
					(
						string.Format
						(
							ERROR_DEPENDENCY_SHORTER_SCOPE,
							typeof(TResolutionType).Name,
							param.ParameterType.Name,
							dependentTypeScope
						)
					);
				}
			}
		}


		private static void RegisterServiceTypes<TRegistrationType, TResolutionType>(ServiceScope scope)
			where TRegistrationType : class
			where TResolutionType : class
		{
			if (!_registeredServices.ContainsKey(typeof(TRegistrationType)))
			{
				//If the registration type is being registered for the first time, add a new entry 
				//to the 'registeredServices' dictionary, and a new dictionary for resolution types.
				_registeredServices.Add(typeof(TRegistrationType), new Dictionary<Type, ServiceScope>());
			}

			if (!_registeredServices[typeof(TRegistrationType)].ContainsKey(typeof(TResolutionType)))
			{
				//Then, register the resolution type itself along with its desired scope
				_registeredServices[typeof(TRegistrationType)].Add(typeof(TResolutionType), scope);
			}
		}


		private static KeyValuePair<Type, ServiceScope> ResolveServiceRegistration<TRegistrationType>
		(
			ServiceScope minimumScope = ServiceScope.Volatile,
			Type expectedType = null
		)
			where TRegistrationType : class
		{
			KeyValuePair<Type, ServiceScope> resolvedType = default;

			if (!_registeredServices.ContainsKey(typeof(TRegistrationType)))
			{
				//Validate that the specified registration type is registered in the container
				throw new ArgumentException
				(
					string.Format
					(
						ERROR_TYPE_NOT_REGISTERED,
						typeof(TRegistrationType).Name
					)
				);
			}
			else if (expectedType == null)
			{
				//If no expected type is specified, resolve the first 
				//suitable type available for the specified registered type.
				resolvedType =
					_registeredServices[typeof(TRegistrationType)]
						.FirstOrDefault(registration => registration.Value >= minimumScope);

				//Validate that a suitable registered type was resolved
				if (resolvedType.Key == null)
				{
					throw new ArgumentException
					(
						string.Format
						(
							ERROR_NO_RESOLUTION_TYPE_SUITABLE,
							typeof(TRegistrationType).Name
						)
					);
				}
			}
			else
			{
				//If an expected resolution type is specified,
				//first validate that the type is registered.
				if (!_registeredServices[typeof(TRegistrationType)].ContainsKey(expectedType))
				{
					throw new ArgumentException
					(
						string.Format
						(
							ERROR_SPECIFIC_TYPE_NOT_REGISTERED,
							expectedType.Name,
							typeof(TRegistrationType).Name
						)
					);
				}

				//Then, ensure that the requested type is
				//suitable to be resolved for the current request
				resolvedType =
					_registeredServices[typeof(TRegistrationType)]
						.FirstOrDefault
						(
							registration =>
								registration.Value >= minimumScope &&
								registration.Key.Equals(expectedType)
						);

				if (resolvedType.Key == null)
				{
					throw new ArgumentException
					(
						string.Format
						(
							ERROR_SPECIFIC_TYPE_NOT_SUITABLE,
							expectedType.Name,
							typeof(TRegistrationType).Name
						)
					);
				}
			}

			return resolvedType;
		}


		private static object ResolveServiceInstance
		(
			(Type registrationType, Type resolutionType) serviceRegistration,
			ServiceScope serviceScope
		)
		{
			object resolvedInstance = null;

			//Determine if a cached instance can be re-used first.
			if
			(
				serviceScope > ServiceScope.Volatile &&
				_cachedInstances.ContainsKey(serviceRegistration) &&
				_cachedInstances[serviceRegistration] != null
			)
			{
				//If so, re-use it
				resolvedInstance = _cachedInstances[serviceRegistration];
			}
			else if (_resolutionStrategies.ContainsKey(serviceRegistration))
			{
				//If a cached instance can not be used (either because the service 
				//scope is 'Volatile' or no previous instance has been cached), check 
				//whether or not an explicit resolution strategy has been defined for it.   
				//If so, use the strategy to define the new instance on the service.
				resolvedInstance = _resolutionStrategies[serviceRegistration].Invoke();
			}
			else
			{
				//If all else fails, construct a new instance of the 
				//service by recursively resolving any of its dependencies.
				resolvedInstance = CreateServiceInstance(serviceRegistration.resolutionType, serviceScope);
			}

			//If the service has a lifetime greater than 'Volatile',
			//check whether or not the resolved instance should be cached
			if (serviceScope > ServiceScope.Volatile)
			{
				if (!_cachedInstances.ContainsKey(serviceRegistration))
				{
					//If the service has never been cached before, create a new entry for it.
					_cachedInstances.Add(serviceRegistration, resolvedInstance);
				}
				else if (_cachedInstances[serviceRegistration] != resolvedInstance)
				{
					//If the cached instance is not the same as
					//the one resolved, update the cached instance
					_cachedInstances[serviceRegistration] = resolvedInstance;
				}
			}

			return resolvedInstance;
		}


		private static object CreateServiceInstance(Type resolvedServiceType, ServiceScope serviceScope)
		{
			//Locate the appropriate overload of 'ResolveService' 
			//to be used for recursive parameter resolution.
			MethodInfo resolveServiceMethod =
				typeof(DependencyManager)
					.GetMethods(BindingFlags.Public | BindingFlags.Static)
					.Where
					(
						method =>
							method.Name.Equals(nameof(ResolveService)) &&
							method.GetGenericArguments().Length == 1
					)
					.Single();

			//Get an array of all constructor parameters necessary for constructing the service instance
			//and project them to an arrray of object instances by recursively resolving services for each.
			//NOTE: These parameter types should have already been validated as resolvable from the container.
			object[] constructorParameters =
				resolvedServiceType
					.GetConstructors()
					.SelectMany(ctor => ctor.GetParameters())
					.Select
					(
						//Recursively resolve a service instance for each 
						//constructor parameter (and any nested object graph)
						ctorParam =>
						{
							//Make the 'ResolveService' method generic and pass the 
							//constructor parameter type as the generic type argument
							MethodInfo parameterResolutionMethod =
								resolveServiceMethod.MakeGenericMethod(ctorParam.ParameterType);

							//Invoke the 'ResolveService' method, passing along the scope of the parent
							return parameterResolutionMethod.Invoke(null, new object[] { serviceScope });
						}
					)
					.ToArray();

			//Create an instance of the service using the activator
			//and passing along any necessary parameters for construction.
			return Activator.CreateInstance(resolvedServiceType, constructorParameters);
		}
		#endregion
	}
}
