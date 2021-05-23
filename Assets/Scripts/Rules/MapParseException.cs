using System;

namespace Rules {
    [Serializable]
    public class MapParseException : ParseException {
        public MapParseException(string file, string message) : base($"Map: {file};\n{message}") { }
    }
}
