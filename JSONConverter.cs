using JsonSerialization.Parsers;

namespace JsonSerialization
{
    /// <summary>
    /// Converts an Object to or from a JSON. 
    /// </summary>
    public static class JSONConverter
    {
        /// <summary>
        /// Serialize object <paramref name="obj"/> to JSON
        /// </summary>
        /// <param name="obj">object to Serialize</param>
        /// <returns>JSON string</returns>
        public static string Serialize(object obj)
        {
            return JsonSerializer.Serialize(obj);
        }

        /// <summary>
        /// Deserialize <paramref name="json"/> to C# object of type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json">The JSON string.</param>
        /// <returns>New object of type <typeparamref name="T"/></returns>
        public static T Deserialize<T>(string json)
        {
            return JsonDeserializer.Deserialize<T>(json);
        }
    }
}
