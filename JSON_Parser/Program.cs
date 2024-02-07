using System.Reflection;
using System.Text;

namespace Json_Convertor
{
    public static class JSONConvertor
    {
        private static readonly Dictionary<string, object> keyValue = new();

        /// <summary>
        /// Generate a JSON representation of the public fields of an object.
        /// </summary>
        /// <param name="obj">The object to convert to JSON form.</param>
        /// <returns>The object's data in JSON format.</returns>
        public static string ToJson(object obj)
        {
            StringBuilder jsonBuilder = new();
            ObjectHandler(obj, jsonBuilder);
            return jsonBuilder.ToString();
        }

        /// <summary>
        /// Create an object from its JSON representation.
        /// </summary>
        /// <typeparam name="T">The type of object represented by the Json.</typeparam>
        /// <param name="json">The JSON representation of the object.</param>
        /// <returns>An instance of the object.</returns>
        public static T FromJson<T>(string json)
        {
            return (T)FromJson(json, typeof(T));
        }

        /// <summary>
        /// Converts the <paramref name="obj"/> to JSON Object Format or Syntax.
        /// </summary>
        /// <param name="obj">The Object that needs to be converted.</param>
        /// <param name="jsonText">The Orignal JSON Text.</param>
        /// <exception cref="NotImplementedException"></exception>
        private static void ObjectHandler(object obj, StringBuilder jsonText)
        {
            if (obj is string)
                return;

            jsonText.Append('{');

            int len = obj.GetType().GetProperties().Length;
            obj.ObjectProperties((prop, i) =>
            {
                object jsonValue = ToJsonValue(prop.GetValue(obj));

                jsonText.Append($"\"{prop.Name}\":");   // Add the Key Name.

                JSONTokenType jSONTypes = GetJsonType(prop.PropertyType);

                switch (jSONTypes)
                {
                    case JSONTokenType.Object:
                        ObjectHandler(jsonValue, jsonText);
                        break;
                    case JSONTokenType.Array:
                        ArrayHandler(jsonValue, jsonText);
                        break;
                    case JSONTokenType.String:
                    case JSONTokenType.Primitive:
                        jsonText.Append(jsonValue);
                        break;
                    default:
                        throw new Exception("This Type is not Supported!");
                }

                if (i + 1 < len)
                    jsonText.Append(',');
            });

            jsonText.Append('}');
        }


        /// <summary>
        /// Converts the <paramref name="objArr"/> to JSON Array Format or Syntax.
        /// </summary>
        /// <param name="objArr">The Array that needs to be converted.</param>
        /// <param name="jsonText">The Orignal JSON Text.</param>
        /// <exception cref="NotImplementedException"></exception>
        private static void ArrayHandler(object objArr, StringBuilder jsonText)
        {
            //"Images":["img1","img2","img3"]
            //"Numbers":[1,2,3]

            //"Names": [{"FirstName": "M", "SecondName": "R"}, {"FirstName": "S", "SecondName": "H"}]

            if (objArr is string)
                return;

            var array = objArr as Array;
            int len = array.Length;
            Type arrayType = array.GetType().GetElementType();


            // it points to the Function that Adds the Element or Value in a suitable format of each Type to the JSON Text.
            Action<object> AddValueToJson = null;  

            JSONTokenType jsonType = GetJsonType(arrayType);

            AddValueToJson = jsonType switch
            {
                JSONTokenType.Object => (obj) => ObjectHandler(obj, jsonText),  //  format the obj as a Json Object.
                JSONTokenType.Array => (array) => ArrayHandler(array, jsonText),   //  format the array as a Json Array.
                JSONTokenType.String or JSONTokenType.Primitive => (value) =>  //   format the value as a normal Json literal value.
                {
                    object Jsonvalue = ToJsonValue(value);
                    jsonText.Append(Jsonvalue);
                }
                ,
                _ => throw new NotImplementedException()
            };


            jsonText.Append('[');

            for (int i = 0; i < len; i++)
            {
                AddValueToJson(array.GetValue(i));

                if (i + 1 < len)
                    jsonText.Append(',');
            }

            jsonText.Append(']');
        }

        /// <summary>
        /// Converts the single <paramref name="value"/> to a JSON Formatted Value.
        /// </summary>
        /// <param name="value">The Value to Convert.</param>
        /// <returns>The formatted JSON Value.</returns>
        private static object ToJsonValue(object value)
        {
            //  checks the property type if it's string it returns the value between quotes.

            Type type = value?.GetType();
            var jsonValue = IsString(type) ? $"\"{value}\"" : value;
            return jsonValue ?? "null";     /// checks if the value is null and returns null string if true.
        }

        private static JSONTokenType GetJsonType(Type type)
        {
            if (IsArray(type))
                return JSONTokenType.Array;
            else if (IsObject(type))
                return JSONTokenType.Object;
            else if (IsString(type))
                return JSONTokenType.String;

            return JSONTokenType.Primitive;
        }

        private static JSONTokenType GetJsonTokenType(char token)
        {
            return token switch
            {
                '"' => JSONTokenType.String,
                '{' => JSONTokenType.Object,
                '}' => JSONTokenType.EndObject,
                '[' => JSONTokenType.Array,
                ']' => JSONTokenType.EndArray,
                ',' => JSONTokenType.comma,
                ' ' => JSONTokenType.Whitespace,
                _ => JSONTokenType.Primitive,
            };
        }

        /// <summary>
        /// Checks if <paramref name="property"/> is String.
        /// </summary>
        /// <param name="property">The Property to check.</param>
        private static bool IsString(Type property) => property == typeof(string);

        /// <summary>
        /// Checks if <paramref name="type"/> is a Custom Type or an Object.
        /// </summary>
        /// <param name="type">The type to Check</param>
        /// <returns>True if The <paramref name="type"/> is an Object,
        /// False if Not.
        /// </returns>
        private static bool IsObject(Type type) => !type.IsPrimitive && !IsString(type);

        /// <summary>
        /// Checks if <paramref name="type"/> is an Array.
        /// </summary>
        /// <param name="type">The type to Check</param>
        /// <returns>True if The <paramref name="type"/> is an Array,
        /// False if Not.
        /// </returns>
        private static bool IsArray(Type type) => type.IsArray;

        private static object FromJson(string jsonText, Type type)
        {
            if (string.IsNullOrEmpty(jsonText))
            {
                return null;
            }

            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (type.IsAbstract)
            {
                throw new ArgumentException($"Cannot deserialize JSON to new instances of the Abstract type '{type.Name}.'");
            }
            

            int i = 0;
            ObjectFromJSON(jsonText, ref i);
            object obj = ObjectAssignHandler(type);
            keyValue.Clear();
            return obj;
        }

        private static object ObjectAssignHandler(Type type)
        {
            object obj = Activator.CreateInstance(type);
            AssignObjectMembers(obj);
            return obj;
        }

        private static void AssignObjectMembers(object obj)
        {
            obj.ObjectProperties((property, _) =>
            {
                Type propertyType = property.PropertyType;
                JSONTokenType tokenType = GetJsonType(propertyType);
                string key = property.Name;
                keyValue.TryGetValue(key, out var value);
                keyValue.Remove(key);

                if (value == null)
                {
                    property.SetValue(obj, null);
                    return;
                }

                switch (tokenType)
                {
                    case JSONTokenType.Object:
                        property.SetValue(obj, ObjectAssignHandler(propertyType));
                        break;
                    case JSONTokenType.Array:
                        var method = typeof(JSONConvertor).GetMethod("ArrayAssignHandler", BindingFlags.NonPublic | BindingFlags.Static);
                        property.SetValue(obj, method.MakeGenericMethod(propertyType.GetElementType()).Invoke(null, new object[] { (string)value }));
                        break;
                    case JSONTokenType.String:
                    case JSONTokenType.Primitive:
                        property.SetValue(obj, value);
                        break;
                }
            });

        }

#pragma warning disable IDE0051 // Remove unused private members
        private static T[] ArrayAssignHandler<T>(string arrayText)
        {
            // The Element Type that the passed Array holds.
            Type elementsType = typeof(T);
            return ArrayAssignIterator().ToArray();

            IEnumerable<T> ArrayAssignIterator()
            {
                int len = arrayText.Length;
                for (int i = 1; i < len; i++)
                {
                    var tokenType = GetJsonTokenType(arrayText[i]);
                    object element = null;
                    switch (tokenType)
                    {
                        case JSONTokenType.Object:
                            ObjectFromJSON(arrayText, ref i);
                            element = ObjectAssignHandler(elementsType);
                            break;
                        case JSONTokenType.Array:
                            element = JSONSubArray(arrayText, ref i);
                            break;
                        case JSONTokenType.String:
                            element = GetStringFromJSON(arrayText, ref i);
                            break;
                        case JSONTokenType.Primitive:
                            element = GetPrimitiveFromJson(arrayText, ref i);
                            break;
                    }
                    if (element != null)
                        yield return (T)element;
                }
            }

            object JSONSubArray(string arrayText, ref int i)
            {
                ReadOnlySpan<char> subArray = arrayText;
                var tokenType = JSONTokenType.StartArray;
                int startIndex = i;
                while(tokenType != JSONTokenType.EndArray)
                {
                    i++;
                    tokenType = GetJsonTokenType(subArray[i]);
                }

                // pass it the degraded array type. ex: int[][] to int[]
                return typeof(JSONConvertor)
                    .GetMethod("ArrayAssignHandler", BindingFlags.NonPublic | BindingFlags.Static)
                    .MakeGenericMethod(typeof(T).GetElementType())
                    .Invoke(null, new object[] { subArray[startIndex..++i].ToString() });
            }
        }
#pragma warning restore IDE0051 // Remove unused private members

        private static void ObjectFromJSON(string jsonText, ref int i)
        {
            ReadOnlySpan<char> span = jsonText;
            int len = span.Length;

            for (++i; i < len; i++)
            {
                char c = span[i];
                if (c != '"' && !char.IsLetter(c))
                    continue;
                string key = GetStringFromJSON(jsonText, ref i);
                object value = GetValueFromJSON(jsonText, ref i);
                

                if (value is null)
                    continue;
                keyValue.TryAdd(key, value);
            }
        }

        private static string ArrayFromJSON(string jsonText, ref int i)
        {
            ReadOnlySpan<char> span = jsonText;
            int arrayStart = i;
            int brackets = 0;
            while (true)
            {
                char c = span[i];
                if (c == '[')
                    brackets++;
                else if (c == ']')
                    brackets--;
                i++;
                if(brackets == 0) break;
            }
            return span[arrayStart..i].ToString();
        }

        private static object GetValueFromJSON(string jsonText, ref int i)
        {
            //  {"Age":1123,"SecondName":true,"Str":   "..."}

            ReadOnlySpan<char> jsonSlicer = jsonText;
            int valueStart = -1;
            string value;

            while (jsonSlicer[i]!= ',' && jsonSlicer[i] != '}')
            {
                i++;
                if (jsonSlicer[i] == ' ' || jsonSlicer[i] == ':') continue;
                switch (GetJsonTokenType(jsonSlicer[i]))
                {
                    case JSONTokenType.String:
                        return GetStringFromJSON(jsonText, ref i);
                    case JSONTokenType.Object:
                        ObjectFromJSON(jsonText, ref i);
                        return null;
                    case JSONTokenType.Array:
                        return ArrayFromJSON(jsonText, ref i);
                    case JSONTokenType.Primitive:
                        if (valueStart == -1)
                            valueStart = i;
                        break;
                }
            }

            value = jsonSlicer[valueStart..i].ToString();

            return PrimitiveTypeIdentfier(value);


            // Identfies which primitvie type the value is from json string.
            
        }

        private static object PrimitiveTypeIdentfier(string value)
        {
            return value switch
            {
                "true" => true,
                "false" => false,
                "null" => null,
                _ => int.Parse(value),
            };
        }

        private static string GetStringFromJSON(string jsonText, ref int i)
        {
            // Also updates int i to a position after quotation marks.
            int stringStart = -1;
            //int len = 0;
            int quotes = 0;
            ReadOnlySpan<char> stringSlicer = jsonText;
            // a temp i for looping.
            int tempi = i;
            while (quotes < 2)
            {
                if (jsonText[tempi] == '"')
                {
                    quotes++;
                    //len = i++;
                    i = tempi++;
                    if (stringStart == -1)
                        stringStart = tempi;
                    continue;
                }
                tempi++;
            }
            return stringSlicer[stringStart..i].ToString();
        }

        private static object GetPrimitiveFromJson(string jsonText, ref int i)
        {
            JSONTokenType tokenType = JSONTokenType.Primitive;
            ReadOnlySpan<char> span = jsonText;
            int startIndex = i;
            while(tokenType is not (JSONTokenType.comma or JSONTokenType.EndObject or JSONTokenType.EndArray))
            {
                i++;
                tokenType = GetJsonTokenType(span[i]);
            }
            return PrimitiveTypeIdentfier(span[startIndex..i].ToString());
        }

        private static void ObjectProperties(this object obj, Action<PropertyInfo, int> property)
        {
            PropertyInfo[] properties = obj.GetType().GetProperties();
            ReadOnlySpan<PropertyInfo> propSpan = properties;
            int len = properties.Length;

            for (int i = 0; i < len; i++)
            {
                property(propSpan[i], i);
            }
        }

        private enum JSONTokenType : byte
        {
            Primitive,
            /// <summary>
            /// The token type is a whitespace.
            /// </summary>
            Whitespace,
            /// <summary>
            /// The token type is the start of a JSON object.
            /// </summary>
            StartObject,
            /// <summary>
            /// The token type is a JSON object.
            /// </summary>
            Object,
            /// <summary>
            /// The token type is the end of a JSON object.
            /// </summary>
            EndObject,
            /// <summary>
            /// The token type is the start of a JSON array.
            /// </summary>
            StartArray,
            /// <summary>
            /// The token type is the a JSON array.
            /// </summary>
            Array,
            /// <summary>
            /// The token type is the end of a JSON array.
            /// </summary>
            EndArray,
            /// <summary>
            /// The token type is a JSON property name.
            /// </summary>
            PropertyName,
            /// <summary>
            /// The token type is a JSON comma.
            /// </summary>
            comma,
            /// <summary>
            /// The token type is a JSON string.
            /// </summary>
            String,
            /// <summary>
            /// The token type is a JSON number.
            /// </summary>
            Number,
            /// <summary>
            /// The token type is the JSON literal true.
            /// </summary>
            True,
            /// <summary>
            /// The token type is the JSON literal false.
            /// </summary>
            False,
            /// <summary>
            /// The token type is the JSON literal null.
            /// </summary>
            Null
        }
    }
}
