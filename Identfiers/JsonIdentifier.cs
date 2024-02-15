namespace JsonSerialization.Identfiers
{
    internal static class JsonIdentifier
    {
        /// <summary>
        /// Identifies a Property datatype.
        /// </summary>
        /// <param name="type">type of property.</param>
        /// <returns>the property's datatype.</returns>
        public static JsonDataType IdentifyDataType(Type type)
        {
            if (type == null)
            {
                return JsonDataType.Null;
            }

            if (type == typeof(string))
            {
                return JsonDataType.String;
            }

            if (type == typeof(int)
                || type == typeof(float)
                || type == typeof(double)
                || type == typeof(decimal))
            {
                return JsonDataType.Number;
            }

            if (type == typeof(bool))
            {
                return JsonDataType.Boolean;
            }

            if (IsArray(type))
            {
                return JsonDataType.Array;
            }

            return JsonDataType.Object;
        }

        public static JsonToken IdentifyJsonToken(char token)
        {
            if (char.IsDigit(token) || token == '-')
                return JsonToken.Number;

            return token switch
            {
                '{' => JsonToken.StartObject,
                '}' => JsonToken.EndObject,
                '[' => JsonToken.StartArray,
                ']' => JsonToken.EndArray,
                '"' => JsonToken.Quotation,
                ',' => JsonToken.Comma,
                ':' => JsonToken.Colon,
                ' ' or
                '\t' or
                '\n' or
                '\r' => JsonToken.Whitespace,
                't' => JsonToken.Boolean,
                'f' => JsonToken.Boolean,
                'n' => JsonToken.Null,
                _ => JsonToken.String
            };
        }

        /// <summary>
        /// Checks if <paramref name="property"/> is String.
        /// </summary>
        /// <param name="property">The Property to check.</param>
        public static bool IsString(Type property) => property == typeof(string);

        /// <summary>
        /// Checks if <paramref name="type"/> is a Custom Type or an Object.
        /// </summary>
        /// <param name="type">The type to Check</param>
        /// <returns>True if The <paramref name="type"/> is an Object,
        /// False if Not.
        /// </returns>
        public static bool IsObject(Type type)
        {
            return !type.IsPrimitive
                && type != typeof(object)
                && type != typeof(string)
                && type != typeof(decimal)
                && !type.IsEnum;
        }

        /// <summary>
        /// Checks if <paramref name="type"/> is an Array.
        /// </summary>
        /// <param name="type">The type to Check</param>
        /// <returns>True if The <paramref name="type"/> is an Array,
        /// False if Not.
        /// </returns>
        public static bool IsArray(Type type) => type.IsArray;
    }
}
