using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

using VariableIdDict = System.Collections.Generic.Dictionary<string, (string, IVariable)>;
using AllClassesVariables = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, (string, IVariable)>>;

public class LogicLoader : MonoBehaviour {
    [SerializeField] FileLoader fileLoader;
    [SerializeField] GameObject kinematicObjectPrefab;
    readonly IdGenerator _idGenerator = new IdGenerator();
    public Dictionary<string, LogicObject> LoadLogicClasses() {
        _idGenerator.Reset();
        var parser = new DrawIOParser();

        var parsedNodes = fileLoader
            .LoadAllWithExtension(fileLoader.LoadText, ".xml")
            .Select(logicText => parser.Parse(logicText))  // TODO: replace ids by new ones from shared pool before merging into one dict (problem: same ids in different files) 
            .SelectMany(x => x)
            .ToDictionary();

        // TODO: APPLY LANGUAGE RULES

        // foreach (var node in parsedNodes.Values) {
        //     Debug.Log(node);
        // }

        var objects = CreateAndSetupObjects(parsedNodes);
        return objects;
    }

    Dictionary<string, LogicObject> CreateAndSetupObjects(Dictionary<string, ParsedNodeInfo> parsedNodes) {
        var baseClasses = InstantiateBaseClasses();
        
        var objects = CreateObjects(parsedNodes);
        
        var allVariablesArray = objects
            .Select(pair => GetVariables(parsedNodes[pair.Key], parsedNodes))
            .ToArray();

        var allVariables = allVariablesArray
            .Zip(objects, (variables, pair) => (parsedNodes[pair.Key].name, variables))
            .ToDictionary();
        foreach (var baseClass in baseClasses) {
            allVariables.Add(baseClass.Class, baseClass.Variables.ToDictionary(pair => _idGenerator.NewId(), pair => (pair.Key, pair.Value)));
        }
        
        string GetBaseClassByClass(string className) {
            bool IsNeededNode(KeyValuePair<string, ParsedNodeInfo> pair) => pair.Value.name == className && pair.Value.type == NodeType.Class;
            var hasNode = parsedNodes.Any(IsNeededNode);
            if (!hasNode) {
                return null;
            }

            var node = parsedNodes.First(IsNeededNode).Value;
            if (node.prev.Length == 0) {
                return Language.Classes.Empty.EmptyClassName;
            }

            return parsedNodes[node.prev.First()].name;
        }
        Dictionary<string, (string, IVariable)> GetVariablesWithInherited(string className,
            Dictionary<string, (string, IVariable)> classVariables) {
            var baseClass = GetBaseClassByClass(className);
            if (baseClass == null) {
                return classVariables;
            }

            return new DictionaryWrapper<string, (string, IVariable)>
                (new []{ classVariables, GetVariablesWithInherited(baseClass, allVariables[baseClass]) })
                .ToDictionary();
        }

        var allVariablesWithInherited = allVariables
            .Select(pair => (pair.Key, GetVariablesWithInherited(pair.Key, pair.Value)))
            .ToDictionary();

        // Debug.Log("Created objects!");
        foreach (var (pair, variables) in objects.Zip(allVariablesArray, (pair, variables) => (pair, variables))) {
            // Debug.Log("Setuping object: " + pair.Key);
            SetupObject(parsedNodes[pair.Key], pair.Value, variables, allVariablesWithInherited, parsedNodes);
        }

        var setupedObjects = objects.Values.ToList();
        setupedObjects.AddRange(baseClasses);
        setupedObjects.Sort((first, second) => {
            if (first.BaseClass == second.BaseClass) return 0;
            if (first.BaseClass == null) return -1;
            if (second.BaseClass == null) return 1;
            if (first.BaseClass == second.Class) return -1;
            if (first.Class == second.BaseClass) return 1;
            return 0;
        });
        
        var setupedObjectsBaseClasses = setupedObjects
            .Select(obj => obj.Class)
            .Select(GetBaseClassByClass).ToArray();
        
        foreach (var (obj, baseClass) in setupedObjects.Zip(setupedObjectsBaseClasses, (o, s) => (o, s))) {
            if (baseClass == null) {
                continue;
            }
            
            setupedObjects.First(so => so.Class == baseClass).Inherit(obj);
        }

        // Debug.Log("Ready!");
        var readyObjects = setupedObjects
            .Select(obj => (objectClass: obj.Class, obj))
            .ToDictionary();
        
        return readyObjects;
    }

    LogicObject[] InstantiateBaseClasses() {
        LogicObject InstantiateType(Type t) {
            var go = Instantiate(kinematicObjectPrefab, gameObject.transform);
            var logicObject = go.AddComponent(t) as LogicObject;

            return logicObject;
        }

        return Assembly.GetExecutingAssembly().GetTypes()
            .Where(type => type.Namespace == typeof(Language.Classes.Empty).Namespace)
            .Select(InstantiateType).ToArray();
    }
    

    Dictionary<string, LogicObject> CreateObjects(Dictionary<string,ParsedNodeInfo> parsedNodes) {
        return parsedNodes
            .Where(pair => pair.Value.type == NodeType.Class)
            .Select(pair => new KeyValuePair<string, LogicObject>(pair.Key, Instantiate(kinematicObjectPrefab, gameObject.transform).AddComponent<LogicObject>()))
            .ToDictionary();
    }

    void SetupObject(ParsedNodeInfo classInfo, LogicObject logicObject, VariableIdDict variables, 
        AllClassesVariables allVariables, Dictionary<string,ParsedNodeInfo> parsedNodes) {
        // Debug.Log("Getting variables");
        var objectVariables = variables
            .ToDictionary(pair => pair.Value.Item1, pair => pair.Value.Item2);

        // Debug.Log("Getting all states");
        HashSet<ParsedNodeInfo> GetAllSuccessors(ParsedNodeInfo node) {
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
        
        var stateInfos = GetAllSuccessors(classInfo)
            .Where(info => info.type == NodeType.State)
            .ToArray();

        var stateNameAndSetterById = stateInfos
            .Select(stateInfo => (stateInfo.id, stateInfo.name.IfEmpty(stateInfo.id)))
            .Select<(string id, string stateName),(string, (string, Action<LogicObject, LogicEngine.LogicEngineAPI>))>(pair => 
                (pair.id, (pair.stateName, (obj, engineAPI) => obj.SetState(pair.stateName, engineAPI)))
            )
            .ToDictionary();
        
        // Debug.Log("Getting general state");
        var generalState = GetStatePair(classInfo, logicObject, stateNameAndSetterById, variables, allVariables, parsedNodes).Value;
        
        // Debug.Log("Getting other states");
        var states = stateInfos
            .Select(stateInfo => GetStatePair(stateInfo, logicObject, stateNameAndSetterById, variables, allVariables, parsedNodes))
            .ToDictionary();
        // foreach (var state in states.Keys) {
        //     Debug.Log("State: " + state);
        // }

        var currentStates = classInfo.next
            .Select(child => parsedNodes[child])
            .Where(info => info.type == NodeType.State)
            .Select(info => stateNameAndSetterById[info.id].Item1)
            .ToArray();
        
        if (currentStates.Length > 1) {
            throw new ArgumentException("");  // TODO: move to LANG RULES
        }
        
        var currentState = currentStates.FirstOrDefault() ?? "";

        // Debug.Log("ACTUALLY Setuping object: " + classInfo.name);
        logicObject.SetupObject(generalState, states, currentState, objectVariables, classInfo.name);
    }

    Dictionary<string, (string, IVariable)> GetVariables(ParsedNodeInfo classInfo, Dictionary<string,ParsedNodeInfo> parsedNodes) {
        return classInfo.next
            .Select(id => parsedNodes[id])
            .Where(info => info.type == NodeType.Variable)
            .Select(info => GetVariablePair(info, parsedNodes))
            .ToDictionary();
    }

    static KeyValuePair<string, (string, IVariable)> GetVariablePair(ParsedNodeInfo variableInfo, Dictionary<string,ParsedNodeInfo> parsedNodes) {
        var split = variableInfo.name.Split(':');
        if (split.Length < 1 || split.Length > 2) {
            throw new ArgumentException("");  // TODO: move to LANG RULES
        }
        
        var parsed = Enum.TryParse(split[0], out ValueType type);
        if (!parsed) {
            throw new ArgumentException("");  // TODO: move to LANG RULES
        }

        var variableName = split.Length == 2 ? split[1].Trim(' ') : "";
        if (variableName == "") {
            variableName = variableInfo.id;
        }
        
        if (variableInfo.parameters.Length > 1) {
            throw new ArgumentException("");  // TODO: move to LANG RULES
        }
        var value = variableInfo.parameters.Length == 1 ? parsedNodes[variableInfo.parameters[0]].name : "";

        return new KeyValuePair<string, (string, IVariable)>(variableInfo.id, (variableName, ValueTypeConverter.GetVariableByType(type, value)));
    }

    static KeyValuePair<string, LogicState> GetStatePair(ParsedNodeInfo stateInfo, LogicObject logicObject,
        Dictionary<string, (string, Action<LogicObject, LogicEngine.LogicEngineAPI>)> stateNameAndSetterById,
        VariableIdDict variables, AllClassesVariables allVariables, Dictionary<string, ParsedNodeInfo> parsedNodes) {
        string stateName;
        if (stateInfo.type == NodeType.Class) {
            // cause not really a state (general class state)
            stateName = stateInfo.name;
        } else {
            stateName = stateNameAndSetterById[stateInfo.id].Item1;
        }
        
        var chainsStarts = stateInfo.next
            .Select(child => parsedNodes[child])
            .Where(info => info.type == NodeType.Action || info.type == NodeType.Condition)
            .ToArray();
        // Debug.Log("State \"" + stateName + "\" has these chain starts: " + string.Join(", ", chainsStarts.Select(i => i.id)));
        var chains = chainsStarts
            .Select(info => GetChain(info, logicObject, stateNameAndSetterById, variables, allVariables, parsedNodes))
            .ToArray();

        return new KeyValuePair<string, LogicState>(stateName, new LogicState(chains));
    }

    static LogicChain GetChain(ParsedNodeInfo chainStartInfo, LogicObject logicObject,
        Dictionary<string, (string, Action<LogicObject, LogicEngine.LogicEngineAPI>)> stateNameAndSetterById, 
        VariableIdDict variables, AllClassesVariables allVariables, Dictionary<string,ParsedNodeInfo> parsedNodes) {
        var chain = logicObject.gameObject.AddComponent<LogicChain>();

        List<ParsedNodeInfo> chainables = new List<ParsedNodeInfo> {chainStartInfo};
        List<(int, int)> chainableRelations = new List<(int, int)>();
        for (int current = 0; current < chainables.Count; current++) {
            if (chainables[current].type == NodeType.State) {
                continue;
            }
            
            var nextInChain = chainables[current].next
                .Select(child => parsedNodes[child])
                .ToArray();

            foreach (var next in nextInChain) {
                if (chainables.Contains(next)) {
                    continue;
                }
                
                var nextIndex = chainables.Count;
                chainableRelations.Add((current, nextIndex));
                chainables.Add(next);
            }
        }
        // Debug.Log("Chainables: " + string.Join("\n", chainables));

        HashSet<ParsedNodeInfo> GetAllOperatorPredecessors(ParsedNodeInfo oper) {
            var rawPredecessors = oper.parameters
                .Select(parameterId => parsedNodes[parameterId])
                .Where(parameterInfo => parameterInfo.prev.Length != 0)
                .Select(parameterInfo => parameterInfo.prev.First())
                .Select(predecessorId => parsedNodes[predecessorId])
                .Where(predecessorInfo => predecessorInfo.type == NodeType.Operator)
                .ToHashSet();

            var predecessors = new HashSet<ParsedNodeInfo>(rawPredecessors);
            predecessors.Add(oper);
            foreach (var predecessor in rawPredecessors) {
                predecessors.UnionWith(GetAllOperatorPredecessors(predecessor));
            }

            return predecessors;
        }
        var operators = chainables
            .SelectMany(info => info.parameters)
            .Select(parameterId => parsedNodes[parameterId])
            .Where(parameterInfo => parameterInfo.prev.Length != 0)
            .Select(parameterInfo => parsedNodes[parameterInfo.prev.First()])
            .Where(valueInfo => valueInfo.type == NodeType.Operator)
            .Aggregate(new HashSet<ParsedNodeInfo>(), (set, info) => {
                set.UnionWith(GetAllOperatorPredecessors(info));
                return set;
            })
            .ToList();
        operators.Sort((op1, op2) => {
            if (GetAllOperatorPredecessors(op1).Contains(op2)) return 1; // op1 > op2 => op2 goes earlier as predecessor
            if (GetAllOperatorPredecessors(op2).Contains(op1)) return -1; // op2 > op1 => op1 goes eqrlier as pedecessor
            return 0;
        });
        
        // Debug.Log("Operators: " + string.Join("\n", operators));

        var operatorPositions = operators
            .Select((oper, pos) => new KeyValuePair<string, int>(oper.id, pos))
            .ToDictionary();
        
        // Debug.Log("Operator positions:\n" + string.Join("\n", operatorPositions.Select(pair => pair.Key + " - " + pair.Value)));
        
        // Debug.Log("Getting operator instantiators for chain " + chainStartInfo.id);
        var operatorInstantiators = operators
            .Select(info => GetNodeInstantiator<IValue, IConstrainable>(info, stateNameAndSetterById, variables, allVariables, operatorPositions, parsedNodes))
            .ToArray();

        // Debug.Log("Getting chainable instantiators for chain " + chainStartInfo.id);
        var chainableInstantiators = chainables
            .Select(info => GetNodeInstantiator<Chainable>(info, stateNameAndSetterById, variables, allVariables, operatorPositions, parsedNodes))
            .ToArray();
        
        // Debug.Log("Got all instantiators for chain " + chainStartInfo.id);
        var objectVariables = variables
            .ToDictionary(pair => pair.Value.Item1, pair => pair.Value.Item2);
        var chainInfo = new LogicChainInfo(operatorInstantiators, chainableInstantiators, chainableRelations.ToArray());
        
        // Debug.Log("Setuping chain " + chainStartInfo.id);
        chain.SetupChain(logicObject, null, objectVariables, chainInfo);
        // Debug.Log("SETUPED chain " + chainStartInfo.id);
        
        return chain;
    }

    static Func<LogicObject, LogicEngine.LogicEngineAPI, Dictionary<string, IVariable>, IValue[], TOut> GetNodeInstantiator<TOut, TReal>(ParsedNodeInfo node,
        Dictionary<string, (string, Action<LogicObject, LogicEngine.LogicEngineAPI>)> stateNameAndSetterById, VariableIdDict variables,
        AllClassesVariables allVariables, Dictionary<string, int> operatorPositions, Dictionary<string, ParsedNodeInfo> parsedNodes)
        where TOut : class where TReal : class, IConstrainable {
        // Debug.Log("Getting instantiator for node " + node.id + " with type " + node.type + " and name " + node.name);
        if (node.type == NodeType.State) {
            // Debug.Log("Returning state setter");
            var setter = stateNameAndSetterById[node.id].Item2;
            return (logicObject, engineAPI, dictionary, values) => new DummySetState(() => setter(logicObject, engineAPI)) as TOut;
        }

        // you know what? this fucking language doesn't have templates nor multiple base classes, so fuck it, I am using reflection now
        var fullname = node.type + "s." + node.name;
        var type = Type.GetType(fullname) ?? Type.GetType(fullname + "`1");
        if (type == null) {
            throw new ArgumentException("Could not find " + node.type + " with name " + node.name);  // TODO: move to LANG RULES
        }

        if (node.type == NodeType.Operator && type.IsGenericType) { // got a generic operator -> it uses VariableRef
            var possibleValueTypes = node.parameters
                .Select(parameterId => parsedNodes[parameterId])
                .Where(parameterNode => parameterNode.prev.Length != 0)
                .Select(parameterNode => parsedNodes[parameterNode.prev.First()])
                .Where(sourceNode => sourceNode.type == NodeType.VariableRef)
                .Select(refNode => (refNode.name, parsedNodes[refNode.prev.First()].name)) // value and class names
                .Select(names =>
                    allVariables[names.Item2]
                        .First(pair => pair.Value.Item1 == names.Item1)) // getting specified variables
                .Select(pair => pair.Value.Item2.GetValueType())
                .ToArray();

            if (possibleValueTypes.Length == 0) {
                throw new ArgumentException("");  // TODO: move to LANG RULES
            }

            if (possibleValueTypes.Any(vt => vt != possibleValueTypes[0])) {  // not all types are same
                throw new ArgumentException("");  // TODO: move to LANG RULES
            }

            type = type.MakeGenericType(ValueTypeConverter.GetType(possibleValueTypes[0]));
        }

        var constructor = type.GetConstructor(new[] {typeof(GameObject), typeof(LogicEngine.LogicEngineAPI), typeof(IValue[]), typeof(bool)});
        if (constructor == null) {
            throw new ApplicationException("Could not find constructor for " + node.type + " with name " + node.name);  // TODO: move to LANG RULES
        }
        
        // Debug.Log("Getting value locators");
        var valueLocators = node.parameters
            .Select(parameterId => parsedNodes[parameterId])
            .Select(parameterInfo => GetValueLocator<TReal>(parameterInfo, constructor, variables, allVariables, operatorPositions, parsedNodes))
            .ToArray();
        
        return (logicObject, engineAPI, dictionary, values) => {
            var usedValues = valueLocators
                .Select(loc => loc(logicObject, engineAPI, dictionary, values))
                .ToArray();
                
            return constructor
                .Invoke(new object[] {logicObject.gameObject, engineAPI, usedValues, false}) as TOut;
        };
    }

    static Func<LogicObject, LogicEngine.LogicEngineAPI, Dictionary<string, IVariable>, IValue[], T> GetNodeInstantiator<T>(ParsedNodeInfo node,
        Dictionary<string, (string, Action<LogicObject, LogicEngine.LogicEngineAPI>)> stateNameAndSetterById, VariableIdDict variables,
        AllClassesVariables allVariables, Dictionary<string, int> operatorPositions, Dictionary<string, ParsedNodeInfo> parsedNodes)
        where T : class, IConstrainable =>
        GetNodeInstantiator<T, T>(node, stateNameAndSetterById, variables, allVariables, operatorPositions, parsedNodes);

    static Func<LogicObject, LogicEngine.LogicEngineAPI, Dictionary<string, IVariable>, IValue[], IValue> GetValueLocator<T>(ParsedNodeInfo parameter,
        ConstructorInfo parentConstructor, VariableIdDict variables, AllClassesVariables allVariables, Dictionary<string, int> operatorPositions,
        Dictionary<string, ParsedNodeInfo> parsedNodes) where T : class, IConstrainable {
        // var parameterIndex = parsedNodes[parameter.parent].parameters.ToList().FindIndex(id => id == parameter.id);
        // Debug.Log("Getting locator for parameter #" + parameterIndex + " in parent with id " + parameter.parent);
        
        var sourceId = parameter.prev.FirstOrDefault() ?? "";
        if (sourceId == "") {  // if parameter has no source
            // Debug.Log("Creating BareParameterLocator");
            return GetBareParameterInstantiator<T>(parameter, parentConstructor, parsedNodes);
        }
        // Debug.Log("Value source: " + sourceId);
        
        ClassRef GetClassRef(string classRefNodeId) => new ClassRef(parsedNodes[classRefNodeId].name);

        var sourceNode = parsedNodes[sourceId];
        // Debug.Log("Value source type: " + sourceNode.type);
        switch (sourceNode.type) {
            case NodeType.Variable:
                // Debug.Log("Creating locator for variable with name: " + variables[sourceId].Item1);
                return (logicObject, engineAPI, dictionary, values) => dictionary[variables[sourceId].Item1];
            case NodeType.Operator:
                // Debug.Log("Creating locator for operator with position: " + operatorPositions[sourceId]);
                return (logicObject, engineAPI, dictionary, values) => values[operatorPositions[sourceId]];
            case NodeType.ClassRef:
                return (logicObject, engineAPI, dictionary, values) => GetClassRef(sourceId);
            case NodeType.VariableRef:
                var classRefNodeId = sourceNode.prev.First();
                var classRef = GetClassRef(classRefNodeId);
                var varName = sourceNode.name;
                var varType = allVariables[classRef.ClassName].Values.First(pair => pair.Item1 == varName).Item2.GetValueType();
                return (logicObject, engineAPI, dictionary, values) => new VariableRef(classRef, varName, varType);
            default:
                throw new ApplicationException("This should not be possible!");
        }
    }

    static Func<LogicObject, LogicEngine.LogicEngineAPI, Dictionary<string, IVariable>, IValue[], IValue> GetBareParameterInstantiator<T>(ParsedNodeInfo parameter,
        ConstructorInfo parentConstructor, Dictionary<string, ParsedNodeInfo> parsedNodes) where T : class, IConstrainable {
        var typeReference = parentConstructor.Invoke(new object[] {null, null, null, true}) as T;
        Debug.Assert(typeReference != null, nameof(typeReference) + " != null");
        
        var constraints = typeReference.GetConstraints();
            
        var parameterIndex = parsedNodes[parameter.parent].parameters.ToList().FindIndex(id => id == parameter.id);
        var possibleValueTypes = constraints[parameterIndex]
            .Where(t => t.GetValueType() != ValueType.Null) // getting rid of NullValues
            .Select(t => t.GetValueType())
            .Where(vt => ValueTypeConverter.GetValueByType(vt, parameter.name) != null)
            .ToArray();

        if (possibleValueTypes.Length == 0) {
            throw new ArgumentException("Can't convert value \"" + parameter.name +
                                        "\" to any possible types of parameter #" + parameterIndex + " of node " +
                                        parentConstructor.ReflectedType);  // TODO: move to LANG RULES
        }

        var possibleType = possibleValueTypes[0];
        return (logicObject, engineAPI, dictionary, values) => ValueTypeConverter.GetValueByType(possibleType, parameter.name);
    }
}