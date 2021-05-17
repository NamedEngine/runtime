using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class FileLoader : MonoBehaviour {
    [SerializeField] PathLimiter pathLimiter;

    public byte[] LoadBytes(string path) {
        return File.ReadAllBytes(pathLimiter.CompleteAndCheckRange(path));
    }
    
    public string LoadText(string path) {
        return File.ReadAllText(pathLimiter.CompleteAndCheckRange(path));
    }

    public (T, string)[] LoadAllWithExtensionAndNames<T>(Func<string, T> loaderMethod, string extension = null, string root = null) {
        var files = new List<string>();
        var directories = new Queue<string>();

        var startingDir = root ?? "";
        var shouldEndWith = extension ?? "";
        directories.Enqueue(startingDir);
        while (directories.Count != 0) {
            var currentDirectory = pathLimiter.CompleteAndCheckRange(directories.Dequeue());
            
            var newFiles = Directory.GetFiles(currentDirectory);
            files.AddRange(newFiles);
            
            var newDirectories = Directory.GetDirectories(currentDirectory);
            foreach (var newDirectory in newDirectories) {
                directories.Enqueue(newDirectory);
            }
        }

        var loadedFiles = files
            .Where(f => f.EndsWith(shouldEndWith))
            .Select(f => (loaderMethod(f), f))
            .ToArray();
        
        return loadedFiles;
    }
    
    public T[] LoadAllWithExtension<T>(Func<string, T> loaderMethod, string extension = null, string root = null) {
        return LoadAllWithExtensionAndNames(loaderMethod, extension, root)
            .Select(filePair => filePair.Item1)
            .ToArray();
    }
}
