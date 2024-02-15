using JsonSerialization.Identfiers;
using JsonSerialization.Utilities;
using System.Reflection;
using System.Text;
using static JsonSerialization.Identfiers.JsonIdentifier;

namespace JsonSerialization.Handlers
{
    internal static class ObjectHandler
    {
        /// <summary>
        /// Serialize <paramref name="obj"/> object to JSON object and append it to <paramref name="jsonBuilder"/>
        /// </summary>
        /// <param name="obj">object to serialize</param>
        /// <param name="jsonBuilder">JSON string</param>
        public static void Serialize(object obj, StringBuilder jsonBuilder)
        {
            jsonBuilder.Append('{');

            int len =  obj.GetType().GetProperties().Length;

            obj.GetType().TypeProperties(property =>
            {
                string key = property.Name;
                object value = property.GetValue(obj);

                jsonBuilder.Append(string.Format("\"{0}\":", key));

                Type propertyType = property.PropertyType;

                var dataType = IdentifyDataType(value?.GetType());
                switch (dataType)
                {
                    case JsonDataType.Object:
                        ObjectHandler.Serialize(value, jsonBuilder);
                        break;

                    case JsonDataType.Array:
                        ArrayHandler.Serialize(value, jsonBuilder);
                        break;

                    case JsonDataType.String:
                    case JsonDataType.Number:
                    case JsonDataType.Boolean:
                    case JsonDataType.Null:
                        jsonBuilder.Append(Utils.GetLiteralValue(value));
                        break;
                }

                if (--len > 0)
                    jsonBuilder.Append(',');
            });

            jsonBuilder.Append('}');
        }

        /// <summary>
        /// Loop through properties and gets it right value from <paramref name="parsedJsonObject"/> Dictionary and assign them to the instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parsedJsonObject">is a <see cref="Dictionary{string, object}"/>.</param>
        /// <returns>A new assigned Instance</returns>
        public static T Deserialize<T>(object parsedJsonObject)
        {
            var keyValuePairs = (Dictionary<string, object>)parsedJsonObject;
            if (keyValuePairs == null) 
                return default;

            // Create a new instance from the given T.
            object instance = Activator.CreateInstance<T>();

            var properties = instance.GetType().GetProperties();

            foreach (PropertyInfo property in properties)
            {
                var propertyName = property.Name;
                var dataType = IdentifyDataType(property.PropertyType);
                object value = null;

                if (!keyValuePairs.ContainsKey(propertyName))
                    continue;

                // handling the property based on it's datatype.
                // if it's a primitive datatype it's assigned directly to the property.
                switch (dataType)
                {
                    case JsonDataType.Object:
                        value = typeof(ObjectHandler)
                            .CallGenericFunction("Deserialize", BindingFlags.Public | BindingFlags.Static
                            , property.PropertyType, null, keyValuePairs[propertyName]);
                       
                        break;

                    case JsonDataType.Array:
                        value = typeof(ArrayHandler)
                            .CallGenericFunction("Deserialize", BindingFlags.Public | BindingFlags.Static
                            , property.PropertyType.GetElementType(), null, keyValuePairs[propertyName]);

                        break;

                    case JsonDataType.Number:
                        value = typeof(NumberHandler)
                            .CallGenericFunction("Deserialize", BindingFlags.Public | BindingFlags.Static
                            , property.PropertyType, null, keyValuePairs[propertyName]);
                        break;
                    case JsonDataType.String:
                    case JsonDataType.Boolean:
                    case JsonDataType.Null:
                        value = keyValuePairs[propertyName];
                        break;
                }
                property.SetValue(instance, value);
            }
            return (T)instance;
        }
    }
}