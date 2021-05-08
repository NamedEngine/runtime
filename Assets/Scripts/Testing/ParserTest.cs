using System.IO;
using System.Linq;
using UnityEngine;
public class ParserTest : MonoBehaviour {
    [SerializeField] FileLoader fileLoader;
    void Start() {
        var parser = new DrawIOParser();
        
        var parsedNodes = fileLoader
            .LoadAllWithExtension(fileLoader.LoadText, ".xml")
            .Select(logicText => parser.Parse(logicText))
            .SelectMany(x => x)
            .ToDictionary();

        foreach (var info in parsedNodes.Values) {
            Debug.Log(info);
        }

        Debug.Log("Saving and loading");
        var path = Path.Combine("Resources", "all.classes");
        var binaryParser = new BinaryParser();
        
        File.WriteAllBytes(path, binaryParser.Save(parsedNodes));
        var loadedNodes = binaryParser.Parse(fileLoader.LoadBytes(path));
        
        foreach (var info in loadedNodes.Values) {
            Debug.Log(info);
        }
    }
}