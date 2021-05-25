using System;

[Serializable]
public class NotFoundException : FileLoadException {
    public NotFoundException(string path) : base(path, "Specified path does not exist or access to it was denied") { }
}
