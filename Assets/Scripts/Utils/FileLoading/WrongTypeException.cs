using System;

[Serializable]
public class WrongTypeException : FileLoadException {
    public readonly PathType ExpectedType;

    public WrongTypeException(string path, PathType expectedType, string message = null) : base(path,
        message ?? $"Specified path does not correspond with the expected type \"{expectedType}\"") {
        ExpectedType = expectedType;
    }
}
