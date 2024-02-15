using JsonSerialization.Identfiers;

namespace JsonSerialization.Parsers
{
    /// <summary>
    /// Parses a JSON string and constructs a Dictionary representation of the parsed data.
    /// </summary>
    internal class JsonParser
    {
        /// <summary> the json text.</summary>
        private readonly string json;

        /// <summary> contains all the wanted properties of distinct models.</summary>
        private readonly Dictionary<string, string> members;

        /// <summary> pointer to the current position in JSON.</summary>
        private int position = 0;

        /// <summary> current path level.</summary>
        string pathLevel = null;


        /// <summary>
        /// Initializes a new instance of the <see cref="JsonParser"/> class with the specified JSON string.
        /// </summary>
        /// <param name="jsonString">The JSON string to be parsed.</param>
        public JsonParser(string jsonString, Dictionary<string, string> members)
        {
            json = jsonString ?? throw new ArgumentNullException(nameof(jsonString));
            this.members = members;
        }

        /// <summary>
        /// Parses the JSON string and returns an object representing the parsed data.
        /// </summary>
        /// <returns>A <see cref="Dictionary{TKey, TValue}"/> object representing the parsed JSON data.</returns>
        public object Parse()
        {
            SkipSymbols();
            return GetCurrentToken() switch
            {
                JsonToken.StartObject => ParseObject(),
                JsonToken.StartArray => ParseArray(),
                JsonToken.Quotation => ParseString(),
                JsonToken.Number => ParseNumber(),
                JsonToken.Boolean => ParseBoolean(),
                JsonToken.Null => ParseNull(),
                _ => throw new NotImplementedException()
            };
        }

        // Implement other parsing methods for objects, arrays, strings, numbers, boolean, and null

        /// <summary>
        /// Parses a JSON object and returns a <see cref="Dictionary{TKey, TValue}"/> representing key-value pairs.
        /// </summary>
        /// <returns>A <see cref="Dictionary{TKey, TValue}"/> representing the parsed JSON object.</returns>
        private object ParseObject()
        {
            SkipSymbols();

            // contains the key-value pairs parsed from JSON.
            var dict = new Dictionary<string, object>();
            // If a type is nested within the current type.
            bool isNestedType;

            // to start after the start of the object '{'.
            position++;
            while (position < json.Length)
            {
                SkipSymbols();

                // Used this type of checking to be more Optimized.
                if (json[position] == '}')
                {
                    break;
                }

                var key = ParseString();

                // path of the key
                var keyPath = pathLevel + key;

                var prevNest = pathLevel;
                isNestedType = members.TryGetValue(keyPath + '@', out string level);

                if (level != null)
                    pathLevel = level;

                if (!members.ContainsKey(keyPath) && !isNestedType)
                {
                    SkipKeyValue();
                    continue;
                }

                dict[key] = Parse();

                pathLevel = prevNest;
            }

            // to start after the end of the object '}'.
            position++;
            return dict;
        }

        /// <summary>
        /// Parses a JSON array and returns a <see cref="List{T}"/> representing the array elements.
        /// </summary>
        /// <returns>A <see cref="List{T}"/> representing the parsed JSON array.</returns>
        private List<object> ParseArray()
        {
            var list = new List<object>();

            // Start with 1 because you've already encountered the opening bracket
            int brackets = 1;

            // Move to the next character after the opening bracket
            position++;
            while (position < json.Length)
            {
                SkipSymbols();

                // Used this type of checking to be more Optimized.
                if (GetCurrentToken() == JsonToken.EndArray)
                    brackets--;

                if (brackets == 0)
                    break;

                list.Add(Parse());

                if (GetCurrentToken() == JsonToken.EndArray)
                    brackets--;
                position++;
            }
            return list;
        }

        /// <summary>
        /// Parses a JSON string and returns the parsed string value.
        /// </summary>
        /// <returns>The parsed string value.</returns>
        private string ParseString()
        {
            SkipSymbols();
            ReadOnlySpan<char> span = json;
            int startIndex = -1;
            bool isInString = false;
            while (true)
            {
                if (GetCurrentToken() == JsonToken.Quotation)
                {
                    // Check if the current Quotation is an Escaped Quotation!
                    if (json[position - 1] == '\\')
                    {
                        position++;
                        continue;
                    }

                    isInString = !isInString;

                    if (startIndex == -1)
                        startIndex = position + 1;

                    if (!isInString)
                        break;
                }
                position++;
            }
            return span[startIndex..position++].ToString();
        }

        /// <summary>
        /// Parses a JSON number and returns the parsed numeric value.
        /// </summary>
        /// <returns>The parsed numeric value (int or float-point).</returns>
        private object ParseNumber()
        {
            SkipSymbols();
            ReadOnlySpan<char> span = json;
            int startIndex = position;

            while (position < json.Length && !IsEndOfValue())
            {
                position++;
            }
            return span[startIndex..position].ToString();
        }

        /// <summary>
        /// Parses a JSON boolean and returns the parsed boolean value.
        /// </summary>
        /// <returns>The parsed boolean value (true or false).</returns>
        private bool ParseBoolean()
        {
            SkipSymbols();
            char c = json[position];
            while (position < json.Length && !IsEndOfValue())
            {
                position++;
            }
            return c == 't';
        }

        /// <summary>
        /// Parses a JSON null value and returns null.
        /// </summary>
        /// <returns>Null.</returns>
        private object ParseNull()
        {
            SkipSymbols();
            while (position < json.Length && !IsEndOfValue())
            {
                position++;
            }
            return null;
        }

        /// <summary>
        /// Skips over Unnecessary characters in the JSON string.
        /// </summary>
        private unsafe void SkipSymbols()
        {
            while (position < json.Length)
            {
                switch (GetCurrentToken())
                {
                    case JsonToken.Comma:
                    case JsonToken.Colon:
                    case JsonToken.Whitespace:
                        position++;
                        continue;
                }
                return;
            }
        }

        /// <summary>
        /// Skips the unwanted Value.
        /// </summary>
        private unsafe void SkipKeyValue()
        {
            bool isInString = false;
            int arrayBrackets = 0;
            int objectBrackets = 0;
            bool isEscaped = false;

            // Didn't use token enum checking to be more faster.
            fixed (char* c = json)
            {
                while (position < json.Length)
                {
                    var token = *(c + position);

                    if (token == '\\')
                        isEscaped = true;

                    switch (token)
                    {
                        case '{':
                            objectBrackets++;
                            break;
                        case '}':
                            if (objectBrackets > 0)
                                objectBrackets--;
                            break;
                        case '[':
                            arrayBrackets++;
                            break;
                        case ']':
                            if (arrayBrackets > 0)
                                arrayBrackets--;
                            break;
                        case '"':
                            if (isEscaped)
                            {
                                isEscaped = false;
                                position++;
                                continue;
                            }
                            isInString = !isInString;
                            break;
                    }

                    if (!isInString && objectBrackets == 0 && arrayBrackets == 0 && token is ',' or '}')
                        break;

                    position++;
                }
            }
        }

        /// <summary>
        /// Gets the current token in the JSON string without advancing the position.
        /// </summary>
        /// <returns>The current token in the JSON string.</returns>
        private JsonToken GetCurrentToken()
        {
            return JsonIdentifier.IdentifyJsonToken(json[position]);
        }

        /// <summary>
        /// Checks if the current Position is the end of the value
        /// </summary>
        /// <returns>true if the current Position is the end of the value, false if not.</returns>
        private bool IsEndOfValue()
        {
            return GetCurrentToken() is JsonToken.Comma or JsonToken.EndArray or JsonToken.EndObject;
        }
    }
}
