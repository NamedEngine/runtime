using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class LogicLoader : MonoBehaviour {
    [SerializeField] FileLoader fileLoader;
    [SerializeField] GameObject kinematicObjectPrefab;
    IdGenerator _idGenerator = new IdGenerator();
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
        var objects = CreateObjects(parsedNodes);
        // Debug.Log("Created objects!");
        foreach (var pair in objects) {
            // Debug.Log("Setuping object: " + pair.Key);
            SetupObject(parsedNodes[pair.Key], pair.Value, parsedNodes);
        }

        // Debug.Log("Ready!");
        var readyObjects = objects
            .Select(pair => (parsedNodes[pair.Key].name, pair.Value))
            .ToDictionary();
        readyObjects.Add("", CreateEmptyLogicObject());
        return readyObjects;
    }
    
    LogicObject CreateEmptyLogicObject() {
        var emptyObject = Instantiate(kinematicObjectPrefab, gameObject.transform).AddComponent<LogicObject>();
        
        var emptyState = new LogicState(new LogicChain[] { });
        var emptyStates = new Dictionary<string, LogicState> {{"", emptyState}};
        var emptyVariables = CreateSpecialVariables(emptyObject);
        
        emptyObject.SetupObject(emptyState, emptyStates, "", emptyVariables);
        emptyObject.Class = "";

        return emptyObject;
    }

    Dictionary<string, LogicObject> CreateObjects(Dictionary<string,ParsedNodeInfo> parsedNodes) {
        return parsedNodes
            .Where(pair => pair.Value.type == NodeType.Class)
            .Select(pair => new KeyValuePair<string, LogicObject>(pair.Key, Instantiate(kinematicObjectPrefab, gameObject.transform).AddComponent<LogicObject>()))
            .ToDictionary();
    }

    void SetupObject(ParsedNodeInfo classInfo, LogicObject logicObject, Dictionary<string,ParsedNodeInfo> parsedNodes) {
        // Debug.Log("Getting variables");
        var variables = GetVariables(classInfo, logicObject, parsedNodes);
        foreach (var pair in variables) {
            // Debug.Log("Variable: " + pair.Key + " AKA " + pair.Value.Item1);
        }
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
            .Select<(string id, string stateName),(string, (string, System.Action<LogicObject>))>(pair => 
                (pair.id, (pair.stateName, obj => obj.SetState(pair.stateName)))
            )
            .ToDictionary();
        
        // Debug.Log("Getting general state");
        var generalState = GetStatePair(classInfo, logicObject, stateNameAndSetterById, variables, parsedNodes).Value;
        
        // Debug.Log("Getting other states");
        var states = stateInfos
            .Select(stateInfo => GetStatePair(stateInfo, logicObject, stateNameAndSetterById, variables, parsedNodes))
            .ToDictionary();
        // foreach (var state in states.Keys) {
        //     Debug.Log("State: " + state);
        // }

        var currentStates = classInfo.next
            .Select(child => parsedNodes[child])
            .Where(info => info.type == NodeType.State)
            .Select(info => stateNameAndSetterById[info.id].Item1)
            .ToArray();
        
        if (currentStates.Length != 1) {
            throw new ArgumentException("");  // TODO: move to LANG RULES
        }
        var currentState = currentStates[0];
        
        
        // Debug.Log("ACTUALLY Setuping object: " + classInfo.name);
        logicObject.SetupObject(generalState, states, currentState, objectVariables);
    }

    Dictionary<string, (string, IVariable)> GetVariables(ParsedNodeInfo classInfo, LogicObject logicObject, Dictionary<string,ParsedNodeInfo> parsedNodes) {
        var variables = classInfo.next
            .Select(id => parsedNodes[id])
            .Where(info => info.type == NodeType.Variable)
            .Select(info => GetVariablePair(info, parsedNodes))
            .ToDictionary();
        
        var specials = CreateSpecialVariables(logicObject);
        foreach (var (specialName, specialVariable) in specials) {
            // if special variable is not used and/or overridden in the script
            if (variables.All(triple => triple.Value.Item1 != specialName)) {
                variables.Add(_idGenerator.NewId(), (specialName, specialVariable));
                continue;
            }

            var (id, (_, variable)) = variables.First(triple => triple.Value.Item1 == specialName);

            var transferred = variable.TryTransferValueTo(specialVariable);
            if (!transferred) {
                throw new ArgumentException("");  // TODO: move to LANG RULES
            }

            variables[id] = (specialName, specialVariable);
        }

        return variables;
    }

    static Dictionary<string, IVariable> CreateSpecialVariables(LogicObject logicObject) {
        return Assembly.GetExecutingAssembly().GetTypes()
            .Where(type => type.Namespace == nameof(Variables))
            .Select(type => (type.Name, type.GetConstructor(new[] {typeof(GameObject)})))
            .Select(pair => (pair.Name, pair.Item2.Invoke(new object[] {logicObject.gameObject}) as IVariable))
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
        Dictionary<string, (string, System.Action<LogicObject>)> stateNameAndSetterById,
        Dictionary<string, (string, IVariable)> variables, Dictionary<string, ParsedNodeInfo> parsedNodes) {
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
            .Select(info => GetChain(info, logicObject, stateNameAndSetterById, variables, parsedNodes))
            .ToArray();

        return new KeyValuePair<string, LogicState>(stateName, new LogicState(chains));
    }

    static LogicChain GetChain(ParsedNodeInfo chainStartInfo, LogicObject logicObject, Dictionary<string, (string, System.Action<LogicObject>)> stateNameAndSetterById, 
        Dictionary<string, (string, IVariable)> variables, Dictionary<string,ParsedNodeInfo> parsedNodes) {
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
            .Select(info => GetNodeInstantiator<IValue, IConstrainable>(info, stateNameAndSetterById, variables, operatorPositions, parsedNodes))
            .ToArray();

        // Debug.Log("Getting chainable instantiators for chain " + chainStartInfo.id);
        var chainableInstantiators = chainables
            .Select(info => GetNodeInstantiator<Chainable>(info, stateNameAndSetterById, variables, operatorPositions, parsedNodes))
            .ToArray();
        
        // Debug.Log("Got all instantiators for chain " + chainStartInfo.id);
        var objectVariables = variables
            .ToDictionary(pair => pair.Value.Item1, pair => pair.Value.Item2);
        var chainInfo = new LogicChainInfo(operatorInstantiators, chainableInstantiators, chainableRelations.ToArray());
        
        // Debug.Log("Setuping chain " + chainStartInfo.id);
        chain.SetupChain(logicObject, objectVariables, chainInfo);
        // Debug.Log("SETUPED chain " + chainStartInfo.id);
        
        return chain;
    }

    static Func<LogicObject, Dictionary<string, IVariable>, IValue[], TOut> GetNodeInstantiator<TOut, TReal>(ParsedNodeInfo node,
        Dictionary<string, (string, System.Action<LogicObject>)> stateNameAndSetterById, Dictionary<string, (string, IVariable)> variables,
        Dictionary<string, int> operatorPositions, Dictionary<string, ParsedNodeInfo> parsedNodes)
        where TOut : class where TReal : class, IConstrainable {
        // Debug.Log("Getting instantiator for node " + node.id + " with type " + node.type + " and name " + node.name);
        if (node.type == NodeType.State) {
            // Debug.Log("Returning state setter");
            var setter = stateNameAndSetterById[node.id].Item2;
            return (logicObject, dictionary, values) => new DummySetState(() => setter(logicObject)) as TOut;
        }

        // you know what? this fucking language doesn't have templates nor multiple base classes, so fuck it, I am using reflection now
        var fullname = node.type + "s." + node.name;
        var type = Type.GetType(fullname);
        if (type == null) {
            throw new ArgumentException("Could not find " + node.type + " with name " + node.name);  // TODO: move to LANG RULES
        }
        
        var constructor = type.GetConstructor(new[] {typeof(GameObject), typeof(IValue[]), typeof(bool)});
        if (constructor == null) {
            throw new ApplicationException("Could not find constructor for " + node.type + " with name " + node.name);  // TODO: move to LANG RULES
        }
        
        // Debug.Log("Getting value locators");
        var valueLocators = node.parameters
            .Select(parameterId => parsedNodes[parameterId])
            .Select(parameterInfo => GetValueLocator<TReal>(parameterInfo, constructor, variables, operatorPositions, parsedNodes))
            .ToArray();
        
        return (logicObject, dictionary, values) => {
            var usedValues = valueLocators
                .Select(loc => loc(logicObject, dictionary, values))
                .ToArray();
                
            return constructor
                .Invoke(new object[] {logicObject.gameObject, usedValues, false}) as TOut;
        };
    }

    static Func<LogicObject, Dictionary<string, IVariable>, IValue[], T> GetNodeInstantiator<T>(ParsedNodeInfo node,
        Dictionary<string, (string, System.Action<LogicObject>)> stateNameAndSetterById, Dictionary<string, (string, IVariable)> variables,
        Dictionary<string, int> operatorPositions, Dictionary<string, ParsedNodeInfo> parsedNodes)
        where T : class, IConstrainable =>
        GetNodeInstantiator<T, T>(node, stateNameAndSetterById, variables, operatorPositions, parsedNodes);

    static Func<LogicObject, Dictionary<string, IVariable>, IValue[], IValue> GetValueLocator<T>(ParsedNodeInfo parameter,
        ConstructorInfo parentConstructor, Dictionary<string, (string, IVariable)> variables, Dictionary<string, int> operatorPositions,
        Dictionary<string, ParsedNodeInfo> parsedNodes) where T : class, IConstrainable {
        var parameterIndex = parsedNodes[parameter.parent].parameters.ToList().FindIndex(id => id == parameter.id);
        // Debug.Log("Getting locator for parameter #" + parameterIndex + " in parent with id " + parameter.parent);
        
        var sourceId = parameter.prev.FirstOrDefault() ?? "";
        if (sourceId == "") {  // if parameter has no source
            // Debug.Log("Creating BareParameterLocator");
            return GetBareParameterInstantiator<T>(parameter, parentConstructor, parsedNodes);
        }
        // Debug.Log("Value source: " + sourceId);

        var sourceType = parsedNodes[sourceId].type;
        // Debug.Log("Value source type: " + sourceType);
        switch (sourceType) {
            case NodeType.Variable:
                // Debug.Log("Creating locator for variable with name: " + variables[sourceId].Item1);
                return (logicObject, dictionary, values) => dictionary[variables[sourceId].Item1];
            case NodeType.Operator:
                // Debug.Log("Creating locator for operator with position: " + operatorPositions[sourceId]);
                return (logicObject, dictionary, values) => values[operatorPositions[sourceId]];
            default:
                throw new ApplicationException("This should not be possible!");
        }
    }

    static Func<LogicObject, Dictionary<string, IVariable>, IValue[], IValue> GetBareParameterInstantiator<T>(ParsedNodeInfo parameter,
        ConstructorInfo parentConstructor, Dictionary<string, ParsedNodeInfo> parsedNodes) where T : class, IConstrainable {
        var typeReference = parentConstructor.Invoke(new object[] {null, null, true}) as T;
        var constraints = typeReference.GetConstraints();
            
        var parameterIndex = parsedNodes[parameter.parent].parameters.ToList().FindIndex(id => id == parameter.id);
        var possibleValueTypes = constraints[parameterIndex]
            .Where(t => ValueTypeConverter.ValueTypes.Any(t.IsType)) // getting rid of NullValues
            .Select(t => ValueTypeConverter.ValueTypes.First(t.IsType))
            .Where(vt => ValueTypeConverter.GetValueByType(vt, parameter.name) != null)
            .ToArray();

        if (possibleValueTypes.Length == 0) {
            throw new ArgumentException("Can't convert value \"" + parameter.name +
                                        "\" to any possible types of parameter #" + parameterIndex + " of node " +
                                        parentConstructor.ReflectedType);  // TODO: move to LANG RULES
        }

        var possibleType = possibleValueTypes[0];
        return (logicObject, dictionary, values) => ValueTypeConverter.GetValueByType(possibleType, parameter.name);
    }
}