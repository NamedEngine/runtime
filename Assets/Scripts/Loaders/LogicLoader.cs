using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class LogicLoader : MonoBehaviour {
    [SerializeField] FileLoader fileLoader;

    void Start() {
        LoadLogicClasses();  // TODO: remove
    }

    public LogicObject[] LoadLogicClasses() {
        var parser = new DrawIOParser();

        var parsedNodes = fileLoader
            .LoadAllWithExtension(fileLoader.LoadText, ".xml")
            .Select(logicText => parser.Parse(logicText))
            .SelectMany(x => x)
            .ToDictionary();

        // TODO: APPLY LANGUAGE RULES

        return null;  // TODO
    }
    
}