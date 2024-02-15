using JsonSerialization.Handlers;
using JsonSerialization.Utilities;
using System.Reflection;
using static JsonSerialization.Identfiers.JsonIdentifier;

namespace JsonSerialization.Parsers
{
    internal static class JsonDeserializer
    {
        /// <summary>
        /// contains all the properties of all models with their path levels.
        /// </summary>
        private readonly static Dictionary<string, string> allTypesMembers = new();

        /// <summary>
        /// Contains all the types that have been previously seen and processed along with their respective paths.
        /// </summary>
        //  Pathing is utilized to avoid conflicts between two or more distinct models sharing the same property name.
        //  '/' used for path leveling, ' ' means the base level.
        private readonly static Dictionary<Type, string> seenTypes = new();

        public static T Deserialize<T>(string json)
        {
            GetTypeMembers(typeof(T), allTypesMembers, string.Empty);

            JsonParser parser = new(json, allTypesMembers);
            var parsedJson = parser.Parse();

            if (IsArray(typeof(T)))
            {
                object array = typeof(ArrayHandler)
                            .CallGenericFunction("Deserialize", BindingFlags.Public | BindingFlags.Static
                            , typeof(T).GetElementType(), null, parsedJson);

                return (T)array;
            }

            return ObjectHandler.Deserialize<T>(parsedJson);
        }

        /// <summary>
        /// Get the needed type members and them to Dictionary with there path.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="members">the Dictionary that holds all the members</param>
        /// <param name="pathLevel">the level of path to access a specific property..</param>
        private static void GetTypeMembers(Type type, Dictionary<string, string> members, string pathLevel)
        {
            // for the passed type if it was array.
            // Getting the base type of the array to get it's properties.
            while (IsArray(type))
            {
                type = type.GetElementType();
            }

            // if the type is a primitive type returns.
            if (!IsObject(type))
                return;

            type.TypeProperties(property =>
            {
                var propertyType = property.PropertyType;

                while (IsArray(propertyType))
                {
                    propertyType = propertyType.GetElementType();
                }

                bool isArray = IsArray(propertyType);
                bool isObject = IsObject(propertyType);

                var key = pathLevel + property.Name;

                if (type != propertyType && !isArray && isObject)
                {
                    key += '@';

                    if (seenTypes.TryGetValue(propertyType, out string value))
                    {
                        members.Add(key, value);
                        return;
                    }

                    var level = pathLevel + '/';
                    seenTypes.TryAdd(propertyType, level);
                    members.TryAdd(key, level);

                    GetTypeMembers(propertyType, members, level);
                    return;
                }

                if (!isArray && isObject)
                    seenTypes.TryAdd(propertyType, pathLevel);

                members.TryAdd(key, "");
            });
        }
    }
}
