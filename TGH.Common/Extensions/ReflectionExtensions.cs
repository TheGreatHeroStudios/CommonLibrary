using System;

namespace TGH.Common.Extensions
{
	public static class ReflectionExtensions
	{
		/// <summary>
		///		Inverts the logic of <seealso cref="Type.IsAssignableFrom(Type?)"/>
		///		to be more easily read and understood within other logic.
		/// </summary>
		/// <param name="instanceType">
		///		The type of the object instance being checked.
		/// </param>
		/// <param name="targetType">
		///		The target type being assigned to.
		/// </param>
		/// <returns>
		///		'True' is an instance of type '<paramref name="instanceType"/>'
		///		can be assigned to a variable of type '<paramref name="targetType"/>'.
		///		Otherwise, 'False'.
		/// </returns>
		public static bool IsAssignableTo(this Type instanceType, Type targetType)
		{
			return targetType.IsAssignableFrom(instanceType);
		}


		/// <summary>
		///		Determines if a given <paramref name="type"/> is either a primitive value type
		///		(inclusive of <seealso cref="string"/>) or a nullable primitive value type.
		/// </summary>
		/// <param name="type">
		///		The type to be checked
		/// </param>
		/// <returns>
		///		'True' if the type is a value type, nullable value type, or string.
		///		Otherwise, 'False'.
		/// </returns>
		public static bool IsPrimitiveOrNullablePrimitive(this Type type)
		{
			return
				type.Equals(typeof(string)) ||
				type.IsValueType ||
				Nullable.GetUnderlyingType(type) != null;
		}
	}
}
