using System;

[Serializable]
public class OutOfRootException : FileLoadException {
    public OutOfRootException(string path, string root) : base(path, $"Specified path is out of root \"{root}\"") { }
}
