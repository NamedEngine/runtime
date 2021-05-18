using System;

namespace Rules {
    [Serializable]
    public class LogicParseException : Exception {
        public LogicParseException(string file, string message) : base($"File: {file};\n{message}") { }
    }
}
