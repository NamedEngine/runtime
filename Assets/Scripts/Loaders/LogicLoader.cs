using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class LogicLoader : MonoBehaviour {
    [SerializeField] FileLoader fileLoader;

    public Dictionary<string, LogicObject> LoadLogicClasses() {
        var parser = new DrawIOParser();

        var parsedNodes = fileLoader
            .LoadAllWithExtension(fileLoader.LoadText, ".xml")
            .Select(logicText => parser.Parse(logicText))
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
        return objects
            .Select(pair => (parsedNodes[pair.Key].name, pair.Value))
            .ToDictionary();
    }

    Dictionary<string, LogicObject> CreateObjects(Dictionary<string,ParsedNodeInfo> parsedNodes) {
        return parsedNodes
            .Where(pair => pair.Value.type == NodeType.Class)
            .Select(pair => new KeyValuePair<string, LogicObject>(pair.Key, gameObject.AddComponent<LogicObject>()))
            .ToDictionary();
    }
    
    void SetupObject(ParsedNodeInfo classInfo, LogicObject logicObject, Dictionary<string,ParsedNodeInfo> parsedNodes) {
        // Debug.Log("Getting variables");
        var variables = GetVariables(classInfo, parsedNodes);
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
            .Select(stateInfo => (stateInfo.id, stateInfo.name != "" ? stateInfo.name : stateInfo.id))
            .Select<(string id, string stateName),(string, (string, System.Action))>(pair => 
                (pair.id, (pair.stateName, () => logicObject.SetState(pair.stateName)))
            )
            .ToDictionary();
        
        // Debug.Log("Getting general state");
        var generalState = GetStatePair(classInfo, stateNameAndSetterById, variables, parsedNodes).Value;
        
        // Debug.Log("Getting other states");
        var states = stateInfos
            .Select(stateInfo => GetStatePair(stateInfo, stateNameAndSetterById, variables, parsedNodes))
            .ToDictionary();
        // foreach (var state in states.Keys) {
        //     Debug.Log("State: " + state);
        // }

        var currentStates = classInfo.next
            .Select(child => parsedNodes[child])
            .Where(info => info.type == NodeType.State)
            .Select(info => info.name)
            .ToArray();
        
        if (currentStates.Length != 1) {
            throw new ArgumentException("");  // TODO: move to LANG RULES
        }
        var currentState = currentStates[0];
        
        
        // Debug.Log("ACTUALLY Setuping object: " + classInfo.name);
        logicObject.SetupObject(generalState, states, currentState, objectVariables);
    }

    Dictionary<string, (string, IVariable)> GetVariables(ParsedNodeInfo classInfo, Dictionary<string,ParsedNodeInfo> parsedNodes) {
        var variables = classInfo.next
            .Select(id => parsedNodes[id])
            .Where(info => info.type == NodeType.Variable)
            .Select(info => GetVariablePair(info, parsedNodes))
            .ToDictionary();

        return variables;
    }

    KeyValuePair<string, (string, IVariable)> GetVariablePair(ParsedNodeInfo variableInfo, Dictionary<string,ParsedNodeInfo> parsedNodes) {
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

        return new KeyValuePair<string, (string, IVariable)>(variableInfo.id, (variableName, GetVariableByType(type, value)));
    }

    IVariable GetVariableByType(ValueType type, string value) {
        T DefaultOrConvert<T>(Func<string, T> converter) {
            return value == "" ? default : converter(value);
        }

        switch (type) {
            case ValueType.String:
                return new Variable<string>(value);
            case ValueType.Int:
                var intValue = DefaultOrConvert(Convert.ToInt32);
                return new Variable<int>(intValue);
            case ValueType.Float:
                var floatValue = DefaultOrConvert(Convert.ToSingle);
                return new Variable<float>(floatValue);
            case ValueType.Bool:
                var boolValue = DefaultOrConvert(Convert.ToBoolean);
                return new Variable<bool>(boolValue);
            default:
                throw new ApplicationException("This should not be possible!");
        }
    }

    KeyValuePair<string, LogicState> GetStatePair(ParsedNodeInfo stateInfo,
        Dictionary<string, (string, System.Action)> stateNameAndSetterById,
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
            .Select(info => GetChain(info, stateNameAndSetterById, variables, parsedNodes))
            .ToArray();

        return new KeyValuePair<string, LogicState>(stateName, new LogicState(chains));
    }
    
    LogicChain GetChain(ParsedNodeInfo chainStartInfo, Dictionary<string, (string, System.Action)> stateNameAndSetterById, 
        Dictionary<string, (string, IVariable)> variables, Dictionary<string,ParsedNodeInfo> parsedNodes) {
        var chain = gameObject.AddComponent<LogicChain>();

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
        chain.SetupChain(objectVariables, chainInfo);
        // Debug.Log("SETUPED chain " + chainStartInfo.id);
        
        return chain;
    }

    Func<Dictionary<string, IVariable>, IValue[], TOut> GetNodeInstantiator<TOut, TReal>(ParsedNodeInfo node,
        Dictionary<string, (string, System.Action)> stateNameAndSetterById, Dictionary<string, (string, IVariable)> variables,
        Dictionary<string, int> operatorPositions, Dictionary<string, ParsedNodeInfo> parsedNodes)
        where TOut : class where TReal : class, IConstrainable {
        // Debug.Log("Getting instantiator for node " + node.id + " with type " + node.type + " and name " + node.name);
        if (node.type == NodeType.State) {
            // Debug.Log("Returning state setter");
            var setter = stateNameAndSetterById[node.id].Item2;
            return (dictionary, values) => new DummySetState(setter) as TOut;
        }

        // you know what? this fucking language doesn't have templates nor multiple base classes, so fuck it, I am using reflection now
        // var prefix = (node.type == NodeType.Action ? nameof(Actions) : nameof(Conditions)) + ".";
        var fullname = node.type + "s." + node.name;
        var type = Type.GetType(fullname);
        if (type == null) {
            throw new ArgumentException("Could not find " + node.type + " with name " + node.name);  // TODO: move to LANG RULES
        }
        
        var constructor = type.GetConstructor(new[] {typeof(IValue[]), typeof(bool)});
        // var constructor = type.GetConstructor(new[] {typeof(IValue[])});
        if (constructor == null) {
            throw new ApplicationException("Could not find constructor for " + node.type + " with name " + node.name);  // TODO: move to LANG RULES
        }
        
        // Debug.Log("Getting value locators");
        var valueLocators = node.parameters
            .Select(parameterId => parsedNodes[parameterId])
            .Select(parameterInfo => GetValueLocator<TReal>(parameterInfo, constructor, variables, operatorPositions, parsedNodes))
            .ToArray();
        
        return (dictionary, values) => {
            var usedValues = valueLocators
                .Select(loc => loc(dictionary, values))
                .ToArray();
                
            return constructor
                .Invoke(new object[] {usedValues, false}) as TOut;
        };
    }

    Func<Dictionary<string, IVariable>, IValue[], T> GetNodeInstantiator<T>(ParsedNodeInfo node,
        Dictionary<string, (string, System.Action)> stateNameAndSetterById, Dictionary<string, (string, IVariable)> variables,
        Dictionary<string, int> operatorPositions, Dictionary<string, ParsedNodeInfo> parsedNodes)
        where T : class, IConstrainable =>
        GetNodeInstantiator<T, T>(node, stateNameAndSetterById, variables, operatorPositions, parsedNodes);
    
    Func<Dictionary<string, IVariable>, IValue[], IValue> GetValueLocator<T>(ParsedNodeInfo parameter,
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
                return (dictionary, values) => dictionary[variables[sourceId].Item1];
            case NodeType.Operator:
                // Debug.Log("Creating locator for operator with position: " + operatorPositions[sourceId]);
                return (dictionary, values) => values[operatorPositions[sourceId]];
            default:
                throw new ApplicationException("This should not be possible!");
        }
    }

    Func<Dictionary<string, IVariable>, IValue[], IValue> GetBareParameterInstantiator<T>(ParsedNodeInfo parameter,
        ConstructorInfo parentConstructor, Dictionary<string, ParsedNodeInfo> parsedNodes) where T : class, IConstrainable {
        var typeReference = parentConstructor.Invoke(new object[] {new IValue[] { }, true}) as T;
        var constraints = typeReference.GetConstraints();
            
        var parameterIndex = parsedNodes[parameter.parent].parameters.ToList().FindIndex(id => id == parameter.id);
        var possibleValueTypes = constraints[parameterIndex]
            .Where(t => ValueTypeConverter.ValueTypes.Any(t.IsType)) // getting rid of NullValues
            .Select(t => ValueTypeConverter.ValueTypes.First(t.IsType))
            .Where(vt => GetValueByType(vt, parameter.name) != null)
            .ToArray();

        if (possibleValueTypes.Length == 0) {
            throw new ArgumentException("Can't convert value \"" + parameter.name +
                                        "\" to any possible types of parameter #" + parameterIndex + " of node " +
                                        parentConstructor.ReflectedType);  // TODO: move to LANG RULES
        }

        var possibleType = possibleValueTypes[0];
        return (dictionary, values) => GetValueByType(possibleType, parameter.name);
    }

    IValue GetValueByType(ValueType type, string value) {
        T DefaultOrConvert<T>(Func<string, T> converter) {
            return value == "" ? default : converter(value);
        }

        try {
            switch (type) {
                case ValueType.String:
                    return new ConcreteValue<string>(value);
                case ValueType.Int:
                    var intValue = DefaultOrConvert(Convert.ToInt32);
                    return new ConcreteValue<int>(intValue);
                case ValueType.Float:
                    var floatValue = DefaultOrConvert(Convert.ToSingle);
                    return new ConcreteValue<float>(floatValue);
                case ValueType.Bool:
                    var boolValue = DefaultOrConvert(Convert.ToBoolean);
                    return new ConcreteValue<bool>(boolValue);
                default:
                    throw new ApplicationException("This should not be possible!");
            }
        }
        catch (FormatException) {
            return null;
        }
    }
}