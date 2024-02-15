using System.Text;
using JsonSerialization.Handlers;

namespace JsonSerialization.Parsers
{
    internal class JsonSerializer
    {
        public static string Serialize(object obj)
        {
            StringBuilder jsonBuilder = new();
            ObjectHandler.Serialize(obj, jsonBuilder);
            return jsonBuilder.ToString();
        }
    }
}
