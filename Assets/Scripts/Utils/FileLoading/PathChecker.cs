using System;
using System.IO;
using System.Linq;
using UnityEngine;

public class PathChecker : MonoBehaviour {
    [SerializeField] string root;
    [SerializeField] string[] supportedImageFormats;

    public string CompleteAndCheck(string path, PathType type) {
        var fullRootPath = Path.GetFullPath(root);
        var resultingPath = Path.GetFullPath(Path.Combine(root, path));

        if (!resultingPath.StartsWith(fullRootPath)) {
            throw new OutOfRootException(path, root);
        }

        if (!File.Exists(resultingPath) && !Directory.Exists(resultingPath)) {
            throw new NotFoundException(path);
        }

        var wrongTypeException = new WrongTypeException(path, type);
        switch (type) {
            case PathType.Directory:
                if (!Directory.Exists(resultingPath)) {
                    throw wrongTypeException;
                }
                break;
            case PathType.Text:
                if (!File.Exists(resultingPath)
                    || supportedImageFormats.Any(f => resultingPath.HasExtension(f))) {
                    throw wrongTypeException;
                }
                break;
            case PathType.Image:
                if (!File.Exists(resultingPath)
                    || !supportedImageFormats.Any(f => resultingPath.HasExtension(f))) {
                    throw wrongTypeException;
                }
                break;
            case PathType.Binary:
                if (!File.Exists(resultingPath)) {
                    throw wrongTypeException;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        return resultingPath;
    }
}
