using System;
using System.IO;
using UnityEngine;

public class PathLimiter : MonoBehaviour {
    [SerializeField] string root;
    public string Root => root;

    public void CheckRange(string path) {  // TODO: maybe also return modified path
        var fullRootPath = Path.GetFullPath(root);
        var fullPath = Path.GetFullPath(path);

        if (!fullPath.StartsWith(fullRootPath)) {
            throw new ArgumentOutOfRangeException("Path \"" + fullPath + "\" is not in root path: \"" + fullRootPath + "\"");
        }
    }
}