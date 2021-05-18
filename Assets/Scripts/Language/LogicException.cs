using System;

namespace Language {
    [Serializable]
    public class LogicException : Exception {
        public LogicException(string callerName, string message) : base($"Cause: {callerName};\n{message}") { }
    }
}
