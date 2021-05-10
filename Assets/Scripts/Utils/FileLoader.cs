using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class FileLoader : MonoBehaviour {
    [SerializeField] PathLimiter pathLimiter;

    public byte[] LoadBytes(string path) {
        pathLimiter.CheckRange(path);
        return File.ReadAllBytes(path);
    }
    
    public string LoadText(string path) {
        pathLimiter.CheckRange(path);
        return File.ReadAllText(path);
    }

    public (T, string)[] LoadAllWithExtensionAndNames<T>(Func<string, T> loaderMethod, string extension) {
        var files = new List<string>();
        var directories = new Queue<string>();
        directories.Enqueue(pathLimiter.Root);
        while (directories.Count != 0) {
            var currenDirectory = directories.Dequeue();
            
            var newFiles = Directory.GetFiles(currenDirectory);
            files.AddRange(newFiles);
            
            var newDirectories = Directory.GetDirectories(currenDirectory);
            foreach (var newDirectory in newDirectories) {
                directories.Enqueue(newDirectory);
            }
        }

        var loadedFiles = files
            .Where(f => f.EndsWith(extension))
            .Select(f => (loaderMethod(f), f))
            .ToArray();
        
        return loadedFiles;
    }
    
    public T[] LoadAllWithExtension<T>(Func<string, T> loaderMethod, string extension) {
        return LoadAllWithExtensionAndNames(loaderMethod, extension)
            .Select(filePair => filePair.Item1)
            .ToArray();
    }
}