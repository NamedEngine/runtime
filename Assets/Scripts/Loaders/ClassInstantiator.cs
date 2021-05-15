using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ClassInstantiator : MonoBehaviour {
    static readonly MapObjectParameter EmptyClassParameter = new MapObjectParameter {
        Name = "Class",
        Type = ValueType.String,
        Value = ""
    };
    
    public Dictionary<string, LogicObject> InstantiateMapObjects(Dictionary<string, LogicObject> classes,
        MapObjectInfo[] objectInfos, Dictionary<string, GameObject> classPrefabs, LogicEngine.LogicEngineAPI engineAPI,
        IdGenerator idGenerator) {
        
        var resultingObjects = new Dictionary<string, LogicObject>();
        foreach (var objectInfo in objectInfos) {
            var classParameter = objectInfo.Parameters.FirstOrDefault(p => p.Name == EmptyClassParameter.Name);
            if (classParameter.IsDefault()) {
                classParameter = EmptyClassParameter;
            }

            if (classParameter.Type != ValueType.String) {
                throw new ArgumentException("");  // TODO
            }

            var className = classParameter.Value;
            var newObject = CreateObject(className, classes, objectInfo, classPrefabs, engineAPI);

            var newObjName = objectInfo.Name != "" ? objectInfo.Name : idGenerator.NewId();
            resultingObjects.Add(newObjName, newObject);
        }

        return resultingObjects;
    }

    public LogicObject CreateObject(string className, Dictionary<string, LogicObject> classes,
        MapObjectInfo objectInfo, Dictionary<string, GameObject> classPrefabs,
        LogicEngine.LogicEngineAPI engineAPI) {
        if (!classes.ContainsKey(className)) {
            throw new ArgumentException("");  // TODO
        }

        var prefab = classPrefabs.First(pair => classes[className].IsClass(pair.Key)).Value;
        var newGameObject = Instantiate(prefab, objectInfo.Rect.position, Quaternion.identity);
        var newObject = classes[className].Clone(newGameObject, engineAPI);
        var size = newGameObject.GetComponent<Size>();
        if (size != null) {
            size.Value = objectInfo.Rect.size;
        }

        if (objectInfo.SortingLayer != null) {
            var spriteRenderer = newGameObject.GetComponent<SpriteRenderer>();
            spriteRenderer.sortingLayerName = objectInfo.SortingLayer;
            if (objectInfo.Sprite != null) {
                spriteRenderer.sprite = objectInfo.Sprite;
            }
        }

        var parameters = objectInfo.Parameters
            .Where(p => p.Name != EmptyClassParameter.Name);
        foreach (var parameter in parameters) {
            if (!newObject.Variables.ContainsKey(parameter.Name)) {
                throw new ArgumentException("");  // TODO
            }
            
            var variable = ValueTypeConverter.GetVariableByType(parameter.Type, parameter.Value);
            var transferred = variable.TryTransferValueTo(newObject.Variables[parameter.Name]);
            if (!transferred) {
                throw new ArgumentException("");  // TODO
            }
        }

        return newObject;
    }
}