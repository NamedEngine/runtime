using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ClassInstantiator : MonoBehaviour {
    public Dictionary<string, LogicObject> InstantiateClasses(Dictionary<string, LogicObject> classes, MapObjectInfo[] objectInfos, LogicEngine.LogicEngineAPI engineAPI) {
        MapObjectParameter emptyClassParameter = new MapObjectParameter {
            Name = "Class",
            Type = ValueType.String,
            Value = ""
        };

        var resultingObjects = new Dictionary<string, LogicObject>();
        var idGenerator = new IdGenerator();
        foreach (var objectInfo in objectInfos) {
            var classParameter = objectInfo.Parameters.FirstOrDefault(p => p.Name == emptyClassParameter.Name);
            if (classParameter.IsDefault()) {
                classParameter = emptyClassParameter;
            }

            if (classParameter.Type != ValueType.String) {
                throw new ArgumentException("");  // TODO
            }

            var className = classParameter.Value;
            if (!classes.ContainsKey(className)) {
                throw new ArgumentException("");  // TODO
            }

            var newObject = classes[className].Clone(objectInfo.GameObject, className, engineAPI);
            var size = objectInfo.GameObject.GetComponent<Size>();
            size.value = objectInfo.Rect.size;

            var parameters = objectInfo.Parameters
                .Where(p => p.Name != emptyClassParameter.Name);
            foreach (var parameter in parameters) {
                if (!newObject.LogicVariables.ContainsKey(parameter.Name)) {
                    throw new ArgumentException("");  // TODO
                }
                
                var variable = ValueTypeConverter.GetVariableByType(parameter.Type, parameter.Value);
                var transferred = variable.TryTransferValueTo(newObject.LogicVariables[parameter.Name]);
                if (!transferred) {
                    throw new ArgumentException("");  // TODO
                }
            }

            var newObjName = objectInfo.Name != "" ? objectInfo.Name : idGenerator.NewId();
            resultingObjects.Add(newObjName, newObject);
        }

        return resultingObjects;
    }
}