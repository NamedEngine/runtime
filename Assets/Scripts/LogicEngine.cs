using System.Collections.Generic;
using UnityEngine;

public class LogicEngine : MonoBehaviour {
    [SerializeField] MapLoader mapLoader;
    [SerializeField] LogicLoader logicLoader;
    [SerializeField] ClassInstantiator classInstantiator;
    

    Dictionary<string, LogicObject> _logicClasses;
    Dictionary<string, LogicObject> _logicObjects;

    void Start() {
        _logicClasses = logicLoader.LoadLogicClasses();

        var objectInfos = mapLoader.LoadMap("Resources\\Maps\\main.tmx");  // TODO
        _logicObjects = classInstantiator.InstantiateClasses(_logicClasses, objectInfos);
    }

    void Update() {
        foreach (var logicObject in _logicObjects) {
            logicObject.Value.ProcessLogic();
        }
    }
}
