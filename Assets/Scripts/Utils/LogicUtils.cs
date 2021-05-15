using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class LogicUtils {
    public static HashSet<ParsedNodeInfo> GetAllSuccessors(ParsedNodeInfo node, Dictionary<string, ParsedNodeInfo> parsedNodes) {
        var successors = new HashSet<ParsedNodeInfo>();

        var queue = new Queue<ParsedNodeInfo>();
        queue.Enqueue(node);
        while (queue.Count > 0) {
            var currentNode = queue.Dequeue();
            var shouldProcess = successors.Add(currentNode);
            if (!shouldProcess) {
                continue;
            }
                
            var children = currentNode.next
                .Select(childId => parsedNodes[childId])
                .ToArray();
            foreach (var child in children) {
                queue.Enqueue(child);
            }
        }

        return successors;
    }

    public static (ValueType, string)? GetVariableTypeAndName(string nodeName) {
        var split = nodeName.Split(':');
        if (split.Length < 1 || split.Length > 2) {
            return null;
        }
        
        var parsed = Enum.TryParse(split[0], out ValueType type);
        if (!parsed) {
            return null;
        }

        if (split.Length == 2 && split[1].Trim(' ') == "") {
            return null;
        }
        
        var variableName = split.Length == 2 ? split[1].Trim(' ') : "";
        return (type, variableName);
    }
    
    public static string GetBaseClassByClass(string className, Dictionary<string, ParsedNodeInfo> parsedNodes) {
        bool IsCurrentClassNode(KeyValuePair<string, ParsedNodeInfo> pair) => pair.Value.name == className && pair.Value.type == NodeType.Class;
        var hasNode = parsedNodes.Any(IsCurrentClassNode);
        if (!hasNode) {
            return null;
        }

        var node = parsedNodes.First(IsCurrentClassNode).Value;
        if (node.prev.Length == 0) {
            return Language.Classes.Empty.EmptyClassName;
        }

        return parsedNodes[node.prev.First()].name;
    }

    public static Type[] GetBaseClassesTypes() {
        return Assembly.GetExecutingAssembly().GetTypes()
            .Where(type => type.Namespace == typeof(Language.Classes.Empty).Namespace)
            .ToArray();
    }

    public static (Dictionary<string, Dictionary<string, ValueType>> classVariables,
        Dictionary<string, DictionaryWrapper<string, IVariable>> baseClassVariables) 
        GetAllNamedVariables(Dictionary<string, ParsedNodeInfo> parsedNodes, TemporaryInstantiator instantiator) {
        var classVariables = parsedNodes.Values
            .Where(info => info.type == NodeType.Class)
            .ToDictionary(info => info.name,
                info => info.next
                    .Select(nextId => parsedNodes[nextId])
                    .Where(nextInfo => nextInfo.type == NodeType.Variable)
                    .Where(nextInfo => GetVariableTypeAndName(nextInfo.name).HasValue)
                    .Select(nextInfo => GetVariableTypeAndName(nextInfo.name).Value)
                    .Where(pair => pair.Item2 != "")
                    .ToDictionary(pair => pair.Item2, pair => pair.Item1));

        var baseClassVariables = LogicUtils.GetBaseClassesTypes()
            .Select(t => instantiator.GetInstance(t) as LogicObject)
            .ToDictionary(logicClass => logicClass.Class, logicClass => logicClass.Variables);

        return (classVariables, baseClassVariables);
    }

    public static ValueType GetVariableType(string variableName, string className,
        Dictionary<string, ParsedNodeInfo> parsedNodes, TemporaryInstantiator instantiator) {
        var (classVariables, baseClassVariables) = GetAllNamedVariables(parsedNodes, instantiator);
        
        while (true) {
            if (baseClassVariables.ContainsKey(className)) {
                if (baseClassVariables[className].ContainsKey(variableName)) {
                    return baseClassVariables[className][variableName].GetValueType();
                }

                return ValueType.Null;
            }

            if (classVariables[className].ContainsKey(variableName)) {
                return classVariables[className][variableName];
            }

            className = GetBaseClassByClass(className, parsedNodes);
        }
    }

    static readonly Dictionary<string, ConstructorInfo> ConstructorByNodeCache = new Dictionary<string, ConstructorInfo>();
    public static IConstrainable GetConstrainable(ParsedNodeInfo nodeInfo, Dictionary<string, ParsedNodeInfo> parsedNodes,
        Dictionary<string, string> idTofile, TemporaryInstantiator instantiator, GameObject gameObject = null,
        LogicEngine.LogicEngineAPI engineAPI = null, IValue[] arguments = null, bool constraintReference = true) {
        ConstructorInfo constructor;
        if (ConstructorByNodeCache.ContainsKey(nodeInfo.id)) {
            constructor = ConstructorByNodeCache[nodeInfo.id];
        } else {
            var fullname = $"{nameof(Language)}.{nodeInfo.type}s.{nodeInfo.name}";
            var type = Type.GetType(fullname) ?? Type.GetType(fullname + "`1");
            if (type == null) {
                // throw new ArgumentException("Could not find " + nodeInfo.type + " with name " + nodeInfo.name);
                return null;
            }
        
            if (nodeInfo.type == NodeType.Operator && type.IsGenericType) { // got a generic operator -> it uses VariableRef
                var possibleValueTypes = nodeInfo.parameters
                    .Select(parameterId => parsedNodes[parameterId])
                    .Where(parameterNode => parameterNode.prev.Length != 0)
                    .Select(parameterNode => parsedNodes[parameterNode.prev.First()])
                    .Where(sourceNode => sourceNode.type == NodeType.VariableRef)
                    .Select(refNode => (refNode.name, parsedNodes[refNode.prev.First()].name)) // variable and class names
                    .Select(names => GetVariableType(names.Item1, names.Item2, parsedNodes, instantiator))
                    .ToArray();
        
                if (possibleValueTypes.Length == 0) {
                    if (idTofile != null) {
                        throw new Rules.LogicParseException(idTofile[nodeInfo.id],
                            $"{nodeInfo.name}\" block of\"{nodeInfo.type}\" wasn't provided" +
                            "\n with any variable reference");
                    }
                    throw new ArgumentException($"Can't deduce type for generic operator {type.Name}: no types are provided");
                }
        
                if (possibleValueTypes.Any(vt => vt != possibleValueTypes[0])) {  // not all types are same
                    if (idTofile != null) {
                        throw new Rules.LogicParseException(idTofile[nodeInfo.id],
                            $"{nodeInfo.name}\" block of\"{nodeInfo.type}\" was provided" +
                            "\n with any variable references of different types");
                    }
                    throw new ArgumentException($"Can't deduce type for generic operator {type.Name}: types are not the same");
                }
        
                type = type.MakeGenericType(ValueTypeConverter.GetType(possibleValueTypes[0]));
            }
            constructor = type.GetConstructor(new[] {typeof(GameObject), typeof(LogicEngine.LogicEngineAPI), typeof(IValue[]), typeof(bool)});
            ConstructorByNodeCache[nodeInfo.id] = constructor;
        }

        return constructor.Invoke(new object[] {gameObject, engineAPI, arguments, constraintReference}) as IConstrainable;
    }
}