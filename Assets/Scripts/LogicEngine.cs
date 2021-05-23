using System;
using System.Collections.Generic;
using System.Linq;
using Language;
using Rules;
using UnityEngine;

public class LogicEngine : MonoBehaviour {
    [SerializeField] MapLoader mapLoader;
    [SerializeField] GameObject mapObject;
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

    Dictionary<string, LogicObject> _logicClasses;
    Dictionary<string, LogicObject> _logicObjects;

    bool _levelChanged;

    readonly IdGenerator _objectNameGenerator = new IdGenerator();

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
            var objName = string.IsNullOrEmpty(name) ? _engine._objectNameGenerator.NewId() : name;

            if (_engine._logicObjects.ContainsKey(objName)) {
                return null;
            }

            var newObject = _engine.classInstantiator.CreateObject(objName, className, _engine._logicClasses,
                MapObjectInfo.GetEmpty(), _engine._classPrefabs, this);
            _engine._logicObjects.Add(objName, newObject);

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

        public void LoadLevel(string path) {
            _engine._levelChanged = true;
            
            if (_engine._logicObjects != null) {
                _engine._logicObjects.Values.ToList().ForEach(lo => lo.AfterFinishProcessing());
                _engine._logicObjects.Clear();
            }
            
            ClearMap();
            
            var objectInfos = _engine.mapLoader.LoadMap(path.ToProperPath(), _engine.mapObject);

            _engine._logicObjects = _engine.classInstantiator.InstantiateMapObjects(_engine._logicClasses, objectInfos,
                _engine._classPrefabs, this, _engine._objectNameGenerator, _engine.mapObject, path);

            try {
                _engine._logicObjects.Values.ToList().ForEach(lo => lo.BeforeStartProcessing());
            }
            catch (LogicException e) {
                _engine.OnLogicError(e);
            }
        }

        void ClearMap() {
            for (var i = 0; i < _engine.mapObject.transform.childCount; i++) {
                Destroy(_engine.mapObject.transform.GetChild(i).gameObject);
            }
        }

        public bool LevelChanged => _engine._levelChanged;
    }

    void OnLogicError(Exception e) {
        logicExceptionHandler.DisplayError(e.Message);
        enabled = false;
        // TODO: maybe also disable all logicObjects
    }

    void Start() {
        try {
            _classPrefabs = classPrefabs.ToDictionary(info => info.className, info => info.prefab);

            _logicClasses = logicLoader.LoadLogicClasses(_objectNameGenerator);

            const string mapPath = "Maps/main.tmx";
            new LogicEngineAPI(this).LoadLevel(mapPath);
        }
        catch (ParseException e) {
            OnLogicError(e);
        }
    }

    void Update() {
        _levelChanged = false;
        
        var objectKeys = _logicObjects.Keys.ToArray();
        foreach (var objectKey in objectKeys) {
            if (_levelChanged) {
                return;
            }

            try {
                _logicObjects[objectKey].ProcessLogic();
            }
            catch (LogicException e) {
                OnLogicError(e);
            }
        }
    }
}
