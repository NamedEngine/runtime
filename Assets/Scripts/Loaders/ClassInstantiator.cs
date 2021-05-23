using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ClassInstantiator : MonoBehaviour {
    static readonly MapObjectParameter EmptyClassParameter = new MapObjectParameter {
        Name = "Class",
        Type = ValueType.String,
        Value = nameof(Language.Classes.Empty)
    };
    
    public Dictionary<string, LogicObject> InstantiateMapObjects(Dictionary<string, LogicObject> classes,
        MapObjectInfo[] objectInfos, Dictionary<string, GameObject> classPrefabs, LogicEngine.LogicEngineAPI engineAPI,
        IdGenerator idGenerator, GameObject mapObject) {
        
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
        if (!classes.ContainsKey(className)) {
            throw new ArgumentException("");  // TODO
        }

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
            .Where(p => p.Name != EmptyClassParameter.Name)
            .ToList();
        
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
