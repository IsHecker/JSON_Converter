using JsonSerialization.Identfiers;
using static JsonSerialization.Identfiers.JsonIdentifier;
using System.Reflection;
using System.Text;
using JsonSerialization.Utilities;

namespace JsonSerialization.Handlers
{
    internal static class ArrayHandler
    {
        public unsafe static void Serialize(object obj, StringBuilder jsonBuilder)
        {
            var array = obj as Array;
            int len = array.Length;

            jsonBuilder.Append('[');

            for (int i = 0; i < len; i++)
            {
                var value = array.GetValue(i);

                var dataType = IdentifyDataType(value.GetType());
                switch (dataType)
                {
                    case JsonDataType.Object:
                        ObjectHandler.Serialize(value, jsonBuilder);
                        break;

                    case JsonDataType.Array:
                        ArrayHandler.Serialize(value.GetType().GetElementType(), jsonBuilder);
                        break;

                    case JsonDataType.String:
                    case JsonDataType.Number:
                    case JsonDataType.Boolean:
                    case JsonDataType.Null:
                        jsonBuilder.Append(Utils.GetLiteralValue(value));
                        break;
                }

                if (i + 1 < len)
                    jsonBuilder.Append(',');
            }

            jsonBuilder.Append(']');
        }

        public static T[] Deserialize<T>(object parsedJsonArray)
        {
            // type of values that the array can hold.
            Type typeOfArray = typeof(T);

            var list = parsedJsonArray as List<object>;
            return ArrayAssignIterator().ToArray();

            IEnumerable<T> ArrayAssignIterator()
            {
                foreach (var element in list)
                {
                    if (element == null)
                        continue;

                    // Assuming that the element value is already a literal value.
                    object value = element;

                    // Checking every loop if the element is a Dictionary, List, or literal value.
                    Type currentElementType = element.GetType();

                    if (currentElementType == typeof(Dictionary<string, object>))
                    {
                        value = typeof(ObjectHandler)
                        .CallGenericFunction("Deserialize", BindingFlags.Public | BindingFlags.Static
                        , typeOfArray, null, element);
                    }
                    else if (JsonIdentifier.IsArray(typeOfArray))
                    {
                        value = typeof(ArrayHandler)
                        .CallGenericFunction("Deserialize", BindingFlags.Public | BindingFlags.Static
                        , typeOfArray.GetElementType(), null, element);
                    }

                    if (value != null)
                        yield return (T)value;
                }
            }
        }
    }
}
