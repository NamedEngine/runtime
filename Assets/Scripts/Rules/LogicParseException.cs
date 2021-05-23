using System;

namespace Rules {
    [Serializable]
    public class LogicParseException : ParseException {
        public LogicParseException(string file, string message) : base($"File: {file};\n{message}") { }
    }
}
