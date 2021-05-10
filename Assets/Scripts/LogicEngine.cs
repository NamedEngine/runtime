using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Rules;
using UnityEngine;

public class LogicEngine : MonoBehaviour {
    [SerializeField] PathLimiter pathLimiter;
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
            var newObject = _engine.classInstantiator.CreateObject(className, _engine._logicClasses,
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

    void Start() {
        void OnLogicError(Exception e) {
            logicExceptionHandler.DisplayError(e.Message);
            enabled = false;
        }
        
        try {
            _classPrefabs = classPrefabs.ToDictionary(info => info.className, info => info.prefab);

            _logicClasses = logicLoader.LoadLogicClasses();

            var mapPath = Path.Combine(pathLimiter.Root, "Maps", "main.tmx");
            var objectInfos = mapLoader.LoadMap(mapPath); // TODO
            _logicObjects = classInstantiator.InstantiateMapObjects(_logicClasses, objectInfos, _classPrefabs,
                new LogicEngineAPI(this), _objectNameGenerator);
        }
        catch (TargetInvocationException e) when (e.InnerException is LogicParseException) {
            OnLogicError(e.InnerException);
        }
        catch (LogicParseException e) {
            OnLogicError(e);
        }
    }

    void Update() {
        foreach (var logicObject in _logicObjects) {
            logicObject.Value.ProcessLogic();
        }
    }
}
