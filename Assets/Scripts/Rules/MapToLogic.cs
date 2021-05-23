using System;
using System.Collections.Generic;
using System.Linq;

namespace Rules {
    public static class MapToLogic {
        static string GetObjectNaming(MapObjectInfo obj) {
            return obj.Name != "" ? $"object named \"{obj.Name}\"" : "unnamed object";
        }

        static void CheckClassParameter(Dictionary<string, LogicObject> classes, MapObjectInfo[] objectInfos, string file) {
            var classParameters = objectInfos
                .Select(MapUtils.GetClassParameter)
                .Select((p, i) => (p, i))
                .ToArray();

            foreach (var (parameter, index) in classParameters.Where(pair => pair.p.Type != ValueType.String)) {
                var objNaming = GetObjectNaming(objectInfos[index]);
                var message = $"Class parameter in an {objNaming} has inappropriate type:" +
                              $"\nIt has {parameter.Type} type instead of intended {ValueType.String} type";

                throw new MapParseException(file, message);
            }

            var notFoundClassNames = classParameters
                .Select(pair => (pair.p.Value, pair.i))
                .Where(pair => !classes.ContainsKey(pair.Value));

            foreach (var (className, index) in notFoundClassNames) {
                var objNaming = GetObjectNaming(objectInfos[index]);
                var message = $"An {objNaming} has inappropriate class \"{className}\". Appropriate classes:\n" +
                              string.Join(", ", classes.Keys);

                throw new MapParseException(file, message);
            }
        }

        static void CheckParametersPresence(Dictionary<string, LogicObject> classes, MapObjectInfo[] objectInfos, string file) {
            var objectsWithExcessParameters = objectInfos
                .Select(info => (info, MapUtils.GetClassParameter(info).Value))
                .Select(pair => (pair.info, pair.info.Parameters
                    .Where(param => param.Name != MapUtils.EmptyClassParameter.Name
                                    && !classes[pair.Value].Variables.ContainsKey(param.Name))
                    .ToArray(),
                    pair.Value))
                .Where(triple => triple.Item2.Length > 0);

            foreach (var (info, excessParameters, className) in objectsWithExcessParameters) {
                var objNaming = GetObjectNaming(info);
                var message = $"An {objNaming} has parameters which are not present in its class \"{className}\"" +
                              "\nThese parameters: " + string.Join(", ", excessParameters.Select(p => p.Name));

                throw new MapParseException(file, message);
            }
        }

        public static List<Action<Dictionary<string, LogicObject>, MapObjectInfo[], string>> GetCheckerMethods() {
            return new List<Action<Dictionary<string, LogicObject>, MapObjectInfo[], string>> {
                CheckClassParameter,
                CheckParametersPresence
            };
        }
    }
}
