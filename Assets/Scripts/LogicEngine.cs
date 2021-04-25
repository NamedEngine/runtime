using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicEngine : MonoBehaviour {
    [SerializeField] MapLoader mapLoader;
    

    Dictionary<string, LogicObject> _logicClasses;
    Dictionary<string, LogicObject> _logicObjects;

    void Start() {
        LoadLogic();
        // LoadLevel("main");
    }

    void Update() {
        foreach (var logicObject in _logicObjects) {
            logicObject.Value.ProcessLogic();
        }
    }

    void LoadLogic() {
        throw new NotImplementedException();
    }
}
