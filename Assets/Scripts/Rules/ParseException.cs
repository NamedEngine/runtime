using System;

namespace Rules {
    [Serializable]
    public abstract class ParseException : Exception {
        protected ParseException(string message) : base(message) { }
    }
}
