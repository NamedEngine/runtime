using System;

[Serializable]
public abstract class FileLoadException : Exception {
    public readonly string Path;
    protected FileLoadException(string path, string message) : base($"Path: {path}\n{message}") {
        Path = path;
    }
}
