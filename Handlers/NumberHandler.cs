using System.Globalization;

namespace JsonSerialization.Handlers
{
    internal static class NumberHandler
    {
        public static T Deserialize<T>(object parsedJson) where T : IParsable<T>
        {
            return T.Parse((string)parsedJson, new NumberFormatInfo()); ;
        }
    }
}
