using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
using SpecialVariableInstantiator = System.Func<UnityEngine.GameObject, LogicEngine.LogicEngineAPI, IVariable>;

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
            return nameof(Language.Classes.Empty);
        }

        return parsedNodes[node.prev.First()].name;
    }

    public static LogicObject[] InstantiateBaseClasses(Func<IInstantiator> instantiatorLocator) {
        LogicObject InstantiateType(Type t) {
            var logicObject = instantiatorLocator().GetInstance(t) as Language.BaseClass;
            Debug.Assert(logicObject != null, nameof(logicObject) + " != null");
            
            logicObject.SetupObject(null, new Dictionary<string, LogicState>(), null,
                logicObject.BaseVariables().ToDictionary(
                    pair => pair.Item1,
                    pair => pair.Item2(logicObject.gameObject, null)), 
                logicObject.BaseClassName(), null);

            return logicObject;
        }

        return Assembly.GetExecutingAssembly().GetTypes()
            .Where(type => type.IsSubclassOf(typeof(Language.BaseClass)))
            .Select(InstantiateType).ToArray();
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

        var baseClassVariables = InstantiateBaseClasses(() => instantiator)
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
        Dictionary<string, string> idTofile, TemporaryInstantiator instantiator, ConstrainableContext context = null, bool constraintReference = true) {
        ConstructorInfo constructor;
        if (ConstructorByNodeCache.ContainsKey(nodeInfo.id)) {
            constructor = ConstructorByNodeCache[nodeInfo.id];
        } else {
            var fullname = $"{nameof(Language)}.{nodeInfo.type}s.{nodeInfo.name}";
            var type = Type.GetType(fullname) ?? Type.GetType(fullname + "`1");
            if (type == null) {
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
                    var message = $"{nodeInfo.name}\" block of\"{nodeInfo.type}\" wasn't provided" +
                                  "\n with any variable reference";
                    if (idTofile != null) {
                        throw new Rules.LogicParseException(idTofile[nodeInfo.id], message);
                    }
                    throw new ArgumentException(message);
                }
        
                if (possibleValueTypes.Any(vt => vt != possibleValueTypes[0])) {  // not all types are same
                    var message = $"{nodeInfo.name}\" block of\"{nodeInfo.type}\" was provided" +
                                  "\n with any variable references of different types";
                    if (idTofile != null) {
                        throw new Rules.LogicParseException(idTofile[nodeInfo.id], message);
                    }
                    throw new ArgumentException(message);
                }
        
                type = type.MakeGenericType(ValueTypeConverter.GetType(possibleValueTypes[0]));
            }
            constructor = type.GetConstructor(new[] {typeof(ConstrainableContext), typeof(bool)});
            ConstructorByNodeCache[nodeInfo.id] = constructor;
        }

        Debug.Assert(constructor != null, nameof(constructor) + " != null");
        return constructor.Invoke(new object[] {context, constraintReference}) as IConstrainable;
    }

    public static SpecialVariableInstantiator GetSpecialVariableInstantiator(Type type) {
        if (!type.GetInterfaces().Contains(typeof(ISpecialVariable))) {
            throw new ArgumentException("Type should implement interface ISpecialVariable");
        }
        
        var constructor = type.GetConstructor(new[] {typeof(GameObject), typeof(LogicEngine.LogicEngineAPI)});
        Debug.Assert(constructor != null, nameof(constructor) + " != null");
        
        return (go, api) => constructor.Invoke(new object[] {go, api}) as IVariable;
    }
    
    public static SpecialVariableInstantiator GetSpecialVariableInstantiator<TVar>() where TVar : ISpecialVariable {
        return GetSpecialVariableInstantiator(typeof(TVar));
    }
    
    public static SpecialVariableInstantiator GetSpecialVariableInstantiator<TVar, T>(T defaultValue) where TVar : SpecialVariable<T> {
        return (go, api) => {
            var variable = GetSpecialVariableInstantiator(typeof(TVar))(go, api) as SpecialVariable<T>;
            Debug.Assert(variable != null, nameof(variable) + " != null");
            
            variable.Set(defaultValue);
            return variable;
        };
    }


    public static (string, SpecialVariableInstantiator) GetSpecialVariablePair<TVar>() where TVar : ISpecialVariable {
        return (typeof(TVar).Name, GetSpecialVariableInstantiator<TVar>());
    }
    
    public static (string, SpecialVariableInstantiator) GetSpecialVariablePair<TVar, T>(T defaultValue) where TVar : SpecialVariable<T> {
        return (typeof(TVar).Name, GetSpecialVariableInstantiator<TVar, T>(defaultValue));
    }
}
