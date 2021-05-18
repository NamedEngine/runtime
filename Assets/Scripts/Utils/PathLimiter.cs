using System;
using System.IO;
using UnityEngine;

public class PathLimiter : MonoBehaviour {
    [SerializeField] string root;
    public string Root => root;

    public string CompleteAndCheckRange(string path) {
        var fullRootPath = Path.GetFullPath(root);
        var resultingPath = Path.GetFullPath(Path.Combine(root, path));

        if (!resultingPath.StartsWith(fullRootPath)) {
            throw new ArgumentOutOfRangeException("Path \"" + resultingPath + "\" is not in root path: \"" + fullRootPath + "\"");
            // todo: to MAYBE logic exception + path existence check
        }

        return resultingPath;
    }
}
