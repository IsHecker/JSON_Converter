using System.Reflection;

namespace JsonSerialization.Utilities
{
    internal static class Utils
    {
        private readonly static Dictionary<Type, MethodInfo> genericMethods = new();
        public static object CallGenericFunction(this Type self, string functionName, BindingFlags bindingFlags, Type type, object instance, params object[] param)
        {
            if (genericMethods.TryGetValue(self, out MethodInfo value))
                return value.MakeGenericMethod(type).Invoke(instance, param);

            MethodInfo method;
            if (bindingFlags == default)
                method = self.GetMethod(functionName);
            else
                method = self.GetMethod(functionName, bindingFlags);

            genericMethods.Add(self, method);
            return method.MakeGenericMethod(type).Invoke(instance, param);
        }

        /// <summary>
        /// Loop through the properties of <paramref name="type"/>.
        /// </summary>
        /// <param name="type">type to loop through it</param>
        /// <param name="property">the current passed property</param>
        public static void TypeProperties(this Type type, Action<PropertyInfo> property)
        {
            PropertyInfo[] properties = type.GetProperties();
            ReadOnlySpan<PropertyInfo> propSpan = properties;
            int len = properties.Length;

            for (int i = 0; i < len; i++)
            {
                property(propSpan[i]);
            }
        }

        /// <summary>
        /// Gets the literal value of a property.
        /// </summary>
        /// <param name="value">property value.</param>
        /// <returns>the literal value</returns>
        public static object GetLiteralValue(object value)
        {
            //  checks the property type if it's string it returns the value between quotes.

            var jsonValue = Identfiers.JsonIdentifier.IsString(value?.GetType()) ? $"\"{value}\"" : value;
            return jsonValue ?? "null";     /// checks if the value is null and returns null string if true.
        }
    }
}
