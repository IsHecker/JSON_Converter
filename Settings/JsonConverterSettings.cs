using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace JsonSerialization.Settings
{
    internal class JsonConverterSettings
    {
        public bool IndentationEnabled { get; set; } = false;
        public string DateFormat { get; set; } = "yyyy-MM-ddTHH:mm:ss.fffZ";
        public bool ThrowOnError { get; set; } = true; // Throw exceptions on errors
    }
}
