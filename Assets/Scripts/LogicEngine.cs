using System;
using System.Collections.Generic;
using System.Linq;
using Language;
using Rules;
using UnityEngine;

public class LogicEngine : MonoBehaviour {
    [SerializeField] MapLoader mapLoader;
    [SerializeField] LogicLoader logicLoader;
    [SerializeField] ClassInstantiator classInstantiator;
    [SerializeField] SizePositionConverter sizePositionConverter;

    [SerializeField] LogicExceptionHandler logicExceptionHandler;

    [Serializable]
    public struct ClassPrefab {
        public string className;
        public GameObject prefab;
    }
    [SerializeField] List<ClassPrefab> classPrefabs;
    Dictionary<string, GameObject> _classPrefabs;

    IdGenerator _objectNameGenerator = new IdGenerator();
    
    public class LogicEngineAPI {
        readonly LogicEngine _engine;

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

        public LogicObject CreateObject(string name, string className) {
            if (_engine._logicObjects.ContainsKey(name)) {
                return null;
            }
            
            var newObject = _engine.classInstantiator.CreateObject(name, className, _engine._logicClasses,
                MapObjectInfo.GetEmpty(), _engine._classPrefabs, this);
            _engine._logicObjects.Add(name, newObject);

            return newObject;
        }

        public SizePositionConverter GetSizePosConverter() {
            return _engine.sizePositionConverter;
        }

        public bool DestroyObject(string name, string className) {
            if (!_engine._logicObjects.ContainsKey(name)) {
                return false;
            }
            
            var foundObject = _engine._logicObjects[name];
            if (!foundObject.IsClass(className)) {
                return false;
            }

            Destroy(foundObject.gameObject);
            return true;
        }
    }
    
    Dictionary<string, LogicObject> _logicClasses;
    Dictionary<string, LogicObject> _logicObjects;

    void OnLogicError(Exception e) {
        logicExceptionHandler.DisplayError(e.Message);
        enabled = false;
        // TODO: maybe also disable all logicObjects
    }

    void Start() {
        try {
            _classPrefabs = classPrefabs.ToDictionary(info => info.className, info => info.prefab);

            _logicClasses = logicLoader.LoadLogicClasses();

            var mapPath = "Maps/main.tmx".ToProperPath();
            var objectInfos = mapLoader.LoadMap(mapPath); // TODO

            _logicObjects = classInstantiator.InstantiateMapObjects(_logicClasses, objectInfos, _classPrefabs,
                new LogicEngineAPI(this), _objectNameGenerator);
        }
        catch (LogicParseException e) {
            OnLogicError(e);
        }

        try {
            _logicObjects.Values.ToList().ForEach(lo => lo.BeforeStartProcessing());
        }
        catch (LogicException e) {
            OnLogicError(e);
        }
    }

    void Update() {
        foreach (var logicObject in _logicObjects) {
            try {
                logicObject.Value.ProcessLogic();
            }
            catch (LogicException e) {
                OnLogicError(e);
            }
        }
    }
}
