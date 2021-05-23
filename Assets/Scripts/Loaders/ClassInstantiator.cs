using System;
using System.Collections.Generic;
using System.Linq;
using Rules;
using UnityEngine;

public class ClassInstantiator : MonoBehaviour {
    public Dictionary<string, LogicObject> InstantiateMapObjects(Dictionary<string, LogicObject> classes,
        MapObjectInfo[] objectInfos, Dictionary<string, GameObject> classPrefabs, LogicEngine.LogicEngineAPI engineAPI,
        IdGenerator idGenerator, GameObject mapObject, string mapFile) {
        RuleChecker.CheckMapToLogic(classes, objectInfos, mapFile);
        
        var resultingObjects = new Dictionary<string, LogicObject>();
        foreach (var objectInfo in objectInfos) {
            var classParameter = MapUtils.GetClassParameter(objectInfo);
            var className = classParameter.Value;

            var newObjName = objectInfo.Name != "" ? objectInfo.Name : idGenerator.NewId();
            var newObject = CreateObject(newObjName, className, classes, objectInfo, classPrefabs, engineAPI);
            newObject.gameObject.transform.parent = mapObject.transform;

            resultingObjects.Add(newObjName, newObject);
        }

        return resultingObjects;
    }

    public LogicObject CreateObject(string objectName, string className, Dictionary<string, LogicObject> classes,
        MapObjectInfo objectInfo, Dictionary<string, GameObject> classPrefabs,
        LogicEngine.LogicEngineAPI engineAPI) {
        var @class = classes[className];
        
        GameObject prefab = null;
        foreach (var classInChain in @class.GetInheritanceChain()) {
            if (!classPrefabs.ContainsKey(classInChain)) {
                continue;
            }
            
            prefab = classPrefabs[classInChain];
            break;
        }
        
        var newGameObject = Instantiate(prefab, new Vector3(), Quaternion.identity);
        
        var newObject = @class.Clone(newGameObject, engineAPI, objectName);

        newObject.transform.position = objectInfo.Rect.position;
        newObject.transform.rotation = Quaternion.Euler(0, 0, -1 * objectInfo.Rotation);
        
        var size = newGameObject.GetComponent<Size>();
        if (size != null) {
            size.Value = objectInfo.Rect.size;
        }

        if (objectInfo.SortingLayer != null) {
            var spriteRenderer = newGameObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer) {
                spriteRenderer.sortingLayerName = objectInfo.SortingLayer;
            }
            if (objectInfo.Sprite != null) {
                spriteRenderer.sprite = objectInfo.Sprite;
            }
        }
        
        var parameters = objectInfo.Parameters
            .Where(p => p.Name != MapUtils.EmptyClassParameter.Name)
            .ToList();
        
        foreach (var parameter in parameters) {
            var variable = ValueTypeConverter.GetVariableByType(parameter.Type, parameter.Value);
            var transferred = variable.TryTransferValueTo(newObject.Variables[parameter.Name]);
            if (!transferred) {
                throw new Exception("Variable was not transferred during object instantiation");
            }
        }

        return newObject;
    }
}
