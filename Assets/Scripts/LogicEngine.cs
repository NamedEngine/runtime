using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LogicEngine : MonoBehaviour {
    [SerializeField] MapLoader mapLoader;
    [SerializeField] LogicLoader logicLoader;
    

    Dictionary<string, LogicObject> _logicClasses;
    Dictionary<string, LogicObject> _logicObjects;

    void Start() {
        _logicClasses = logicLoader.LoadLogicClasses();

        _logicObjects = _logicClasses
            .Select(pair => (pair.Key, pair.Value.Clone(gameObject)))
            .ToDictionary();
    }

    void Update() {
        foreach (var logicObject in _logicObjects) {
            logicObject.Value.ProcessLogic();
        }
    }
}
