using System.Collections.Generic;
using UnityEngine;

public class LogicEngine : MonoBehaviour {
    [SerializeField] MapLoader mapLoader;
    [SerializeField] LogicLoader logicLoader;
    [SerializeField] ClassInstantiator classInstantiator;
    
    public class LogicEngineAPI {
        LogicEngine _engine;
        public LogicEngineAPI(LogicEngine engine) {
            _engine = engine;
        }

        public LogicObject GetObjectByName(string name, string className) {
            if (!_engine._logicObjects.ContainsKey(name)) {
                return null;
            }
            
            var foundObject = _engine._logicObjects[name];
            if (!foundObject.IsClass(className)) {
                return null;
            }

            return foundObject;
        }
    }
    
    Dictionary<string, LogicObject> _logicClasses;
    Dictionary<string, LogicObject> _logicObjects;

    void Start() {
        _logicClasses = logicLoader.LoadLogicClasses();

        var objectInfos = mapLoader.LoadMap("Resources\\Maps\\main.tmx");  // TODO
        _logicObjects = classInstantiator.InstantiateClasses(_logicClasses, objectInfos, new LogicEngineAPI(this));
    }

    void Update() {
        foreach (var logicObject in _logicObjects) {
            logicObject.Value.ProcessLogic();
        }
    }
}
