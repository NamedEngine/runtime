using System;
using System.Collections.Generic;
using System.Linq;
using Language;
using UnityEngine;
using VariableIdDict = System.Collections.Generic.Dictionary<string, (string, IVariable)>;
using AllClassesVariables = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, (string, IVariable)>>;

public class LogicLoader : MonoBehaviour {
    [SerializeField] FileLoader fileLoader;
    [SerializeField] TemporaryInstantiator temporaryInstantiator;
    [SerializeField] GameObject kinematicObjectPrefab;
    public Dictionary<string, LogicObject> LoadLogicClasses(IdGenerator idGenerator) {
        var parser = new DrawIOParser(idGenerator);

        var filePairs = fileLoader
            .LoadAllWithExtensionAndNames(fileLoader.LoadText, ".xml")
            .ToList();

        filePairs.ForEach(pair =>
            Rules.RuleChecker.CheckParsing<Rules.Parsing.DrawIO, string>(pair.Item1, pair.Item2)
        );

        var filesWithParsedNodes = filePairs
            .Select(filePair => (filePair.Item2, parser.Parse(filePair.Item1)))
            .ToArray();

        var parsedNodes = filesWithParsedNodes
            .SelectMany(dictPair => dictPair.Item2)
            .ToDictionary();

        var idToFile = filesWithParsedNodes
            .Select(dictPair => 
                dictPair.Item2.ToDictionary(kv => kv.Key, kv => dictPair.Item1))
            .SelectMany(x => x)
            .ToDictionary();

        Rules.RuleChecker.CheckLogic(parsedNodes, idToFile, temporaryInstantiator);

        var objects = CreateAndSetupObjects(parsedNodes, idGenerator);
        return objects;
    }

    Dictionary<string, LogicObject> CreateAndSetupObjects(Dictionary<string, ParsedNodeInfo> parsedNodes, IdGenerator idGenerator) {
        var baseClasses = InstantiateBaseClasses();
        
        var objects = CreateObjects(parsedNodes);
        
        var allVariablesArray = objects
            .Select(pair => GetVariables(parsedNodes[pair.Key], parsedNodes))
            .ToArray();

        var allVariables = allVariablesArray
            .Zip(objects, (variables, pair) => (parsedNodes[pair.Key].name, variables))
            .ToDictionary();
        foreach (var baseClass in baseClasses) {
            allVariables.Add(baseClass.Class, baseClass.Variables.ToDictionary(pair => idGenerator.NewId(), pair => (pair.Key, pair.Value)));
        }
        
        Dictionary<string, (string, IVariable)> GetVariablesWithInherited(string className,
            Dictionary<string, (string, IVariable)> classVariables) {
            var baseClass = LogicUtils.GetBaseClassByClass(className, parsedNodes);
            if (baseClass == null) {
                return classVariables;
            }

            return new DictionaryWrapper<string, (string, IVariable)>
                (new IReadOnlyDictionary<string, (string, IVariable)>[] {
                    classVariables,
                    GetVariablesWithInherited(baseClass, allVariables[baseClass])
                })
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
        
        var setupedObjectsBaseClasses = setupedObjects
            .Select(obj => {
                var baseClassName = LogicUtils.GetBaseClassByClass(obj.Class, parsedNodes);
                if (baseClassName == null && obj is BaseClass baseClass) {
                    baseClassName = baseClass.ShouldInheritFrom();
                }

                return baseClassName;
            })
            .ToArray();

        var objectsWithBaseClasses = setupedObjects.Zip(setupedObjectsBaseClasses, (o, c) => (o, c)).ToList();
        objectsWithBaseClasses.Sort((firstPair, secondPair) => {
            var (first, firstBase) = firstPair;
            var (second, secondBase) = secondPair;
            if (firstBase == secondBase) return 0;
            if (firstBase == null) return -1;
            if (secondBase == null) return 1;
            if (first.Class == secondBase) return -1;
            if (firstBase == second.Class) return 1;
            return 0;
        });
        
        foreach (var (obj, baseClass) in objectsWithBaseClasses) {
            if (baseClass == null) {
                continue;
            }
            
            setupedObjects.First(so => so.Class == baseClass).Inherit(obj, null);
        }

        // Debug.Log("Ready!");
        var readyObjects = setupedObjects
            .Select(obj => (objectClass: obj.Class, obj))
            .ToDictionary();
        
        return readyObjects;
    }

    LogicObject[] InstantiateBaseClasses() {
        return LogicUtils.InstantiateBaseClasses(() => {
            var go = Instantiate(kinematicObjectPrefab, gameObject.transform);
            return new InstantiatorWrapper(go);
        });
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
        var stateInfos = LogicUtils.GetAllSuccessors(classInfo, parsedNodes)
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
            throw new ArgumentException("");  // TODO
        }
        
        var currentState = currentStates.FirstOrDefault() ?? "";

        // Debug.Log("ACTUALLY Setuping object: " + classInfo.name);
        logicObject.SetupObject(generalState, states, currentState, objectVariables, classInfo.name, null, null);
    }

    Dictionary<string, (string, IVariable)> GetVariables(ParsedNodeInfo classInfo, Dictionary<string,ParsedNodeInfo> parsedNodes) {
        return classInfo.next
            .Select(id => parsedNodes[id])
            .Where(info => info.type == NodeType.Variable)
            .Select(info => GetVariablePair(info, parsedNodes))
            .ToDictionary();
    }

    static KeyValuePair<string, (string, IVariable)> GetVariablePair(ParsedNodeInfo variableInfo, Dictionary<string,ParsedNodeInfo> parsedNodes) {
        var pair = LogicUtils.GetVariableTypeAndName(variableInfo.name);
        if (pair is null) {
            throw new ArgumentException($"Improper variable label: \"{variableInfo.name}\"");
        }
        
        var (type, variableName) = pair.Value;
        if (variableName == "") {
            variableName = variableInfo.id;
        }
        
        if (variableInfo.parameters.Length > 1) {
            throw new ArgumentException("");  // TODO
        }
        var value = variableInfo.parameters.Length == 1 ? parsedNodes[variableInfo.parameters[0]].name : "";

        return new KeyValuePair<string, (string, IVariable)>(variableInfo.id, (variableName, ValueTypeConverter.GetVariableByType(type, value)));
    }

    KeyValuePair<string, LogicState> GetStatePair(ParsedNodeInfo stateInfo, LogicObject logicObject,
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

    LogicChain GetChain(ParsedNodeInfo chainStartInfo, LogicObject logicObject,
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
                if (!chainables.Contains(next)) {
                    chainables.Add(next);
                }
                var nextIndex = chainables.IndexOf(next);
                chainableRelations.Add((current, nextIndex));
            }
        }
        // Debug.Log("Chainables: " + string.Join("\n", chainables));

        Dictionary<ParsedNodeInfo, int> ProcessOperatorGraph(HashSet<ParsedNodeInfo> prevLevel, int nextLevelNum,
            Dictionary<ParsedNodeInfo, int> accumulator = null) {
            if (accumulator == null) {
                accumulator = new Dictionary<ParsedNodeInfo, int>();
            }

            if (prevLevel.Count == 0) {
                return accumulator;
            }

            var newLevel = prevLevel
                .SelectMany(info => info.parameters)
                .Select(parameterId => parsedNodes[parameterId])
                .Where(parameterInfo => parameterInfo.prev.Length != 0)
                .Select(parameterInfo => parsedNodes[parameterInfo.prev.First()])
                .Where(valueInfo => valueInfo.type == NodeType.Operator)
                .ToHashSet();

            foreach (var info in newLevel) {
                accumulator[info] = nextLevelNum;
            }

            return ProcessOperatorGraph(newLevel, nextLevelNum + 1, accumulator);
        }

        var operatorsWithDepth = ProcessOperatorGraph(chainables.ToHashSet(), 0);
        var operators = operatorsWithDepth
            .OrderByDescending(pair => pair.Value)
            .Select(pair => pair.Key)
            .ToArray();

        // Debug.Log("Operators: " + string.Join("\n\n", operators));

        var operatorPositions = operators
            .Select((oper, pos) => new KeyValuePair<string, int>(oper.id, pos))
            .ToDictionary();
        
        // Debug.Log("Operator positions:\n" + string.Join("\n", operatorPositions.Select(pair => pair.Key + " - " + pair.Value)));
        
        // Debug.Log("Getting operator instantiators for chain " + chainStartInfo.id);
        var operatorInstantiators = operators
            .Select(info => GetNodeInstantiator<IValue>(info, stateNameAndSetterById, variables, allVariables, operatorPositions, parsedNodes))
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
        chain.SetupChain(logicObject, new BaseContext(null, objectVariables, null), chainInfo);
        // Debug.Log("SETUPED chain " + chainStartInfo.id);
        
        return chain;
    }

    Func<ArgumentLocationContext, TOut> GetNodeInstantiator<TOut>(ParsedNodeInfo node,
        Dictionary<string, (string, Action<LogicObject, LogicEngine.LogicEngineAPI>)> stateNameAndSetterById, VariableIdDict variables,
        AllClassesVariables allVariables, Dictionary<string, int> operatorPositions, Dictionary<string, ParsedNodeInfo> parsedNodes)
        where TOut : class {
        // Debug.Log("Getting instantiator for node " + node.id + " with type " + node.type + " and name " + node.name);
        if (node.type == NodeType.State) {
            // Debug.Log("Returning state setter");
            var setter = stateNameAndSetterById[node.id].Item2;
            return (argLocContext) => new SetState(() => setter(argLocContext.LogicObject, argLocContext.Base.EngineAPI)) as TOut;
        }

        var constraints = LogicUtils.GetConstrainable(node, parsedNodes, null, temporaryInstantiator).GetConstraints();
        temporaryInstantiator.Clear();
        // Debug.Log("Getting value locators");
        var argumentLocators = node.parameters
            .Select(parameterId => parsedNodes[parameterId])
            .Select(parameterInfo => GetArgumentLocator(parameterInfo, constraints, variables, allVariables, operatorPositions, parsedNodes))
            .ToArray();
        
        return (argumentLocationContext) => {
            var usedValues = argumentLocators
                .Select(loc => loc(argumentLocationContext))
                .ToArray();

            var context = new ConstrainableContext(argumentLocationContext.Base,
                argumentLocationContext.LogicObject.gameObject, usedValues);
            var instance = LogicUtils.GetConstrainable(node, parsedNodes, null, temporaryInstantiator, context,
                false) as TOut;
            temporaryInstantiator.Clear();

            return instance;
        };
    }

    static Func<ArgumentLocationContext, IValue> GetArgumentLocator(ParsedNodeInfo parameter,
        IValue[][] constraints, VariableIdDict variables, AllClassesVariables allVariables, Dictionary<string, int> operatorPositions,
        Dictionary<string, ParsedNodeInfo> parsedNodes) {
        var sourceId = parameter.prev.FirstOrDefault() ?? "";
        if (sourceId == "") {  // if parameter has no source
            // Debug.Log("Creating BareParameterLocator");
            return GetBareParameterInstantiator(parameter, constraints, parsedNodes);
        }
        // Debug.Log("Value source: " + sourceId);
        
        ClassRef GetClassRef(string classRefNodeId) => new ClassRef(parsedNodes[classRefNodeId].name);

        var sourceNode = parsedNodes[sourceId];
        // Debug.Log("Value source type: " + sourceNode.type);
        switch (sourceNode.type) {
            case NodeType.Variable:
                // Debug.Log("Creating locator for variable with name: " + variables[sourceId].Item1);
                return (argumentLocationContext) => argumentLocationContext.Base.VariableDict[variables[sourceId].Item1];
            case NodeType.Operator:
                // Debug.Log("Creating locator for operator with position: " + operatorPositions[sourceId]);
                return (argumentLocationContext) => argumentLocationContext.PreparedOperators[operatorPositions[sourceId]];
            case NodeType.Class:
            case NodeType.ClassRef:
                return (argumentLocationContext) => GetClassRef(sourceId);
            case NodeType.VariableRef:
                var classRefNodeId = sourceNode.prev.First();
                var classRef = GetClassRef(classRefNodeId);
                var varName = sourceNode.name;
                var varType = allVariables[classRef.ClassName].Values.First(pair => pair.Item1 == varName).Item2.GetValueType();
                return (argumentLocationContext) => new VariableRef(classRef, varName, varType);
            default:
                throw new ApplicationException("This should not be possible!");
        }
    }

    static Func<ArgumentLocationContext, IValue> GetBareParameterInstantiator(ParsedNodeInfo parameter,
        IValue[][] constraints, Dictionary<string, ParsedNodeInfo> parsedNodes) {
        var parameterIndex = parsedNodes[parameter.parent].parameters.ToList().FindIndex(id => id == parameter.id);
        var possibleValueTypes = constraints[parameterIndex]
            .Where(t => !(t is NullValue)) // getting rid of NullValues
            .Select(t => t.GetValueType())
            .Where(vt => ValueTypeConverter.GetValueByType(vt, parameter.name) != null)
            .ToArray();

        if (possibleValueTypes.Length == 0) {
            throw new ArgumentException("Can't convert value \"" + parameter.name +
                                        "\" to any possible types of parameter #" + parameterIndex + " of node ");
        }

        var possibleType = possibleValueTypes[0];
        return (argumentLocationContext) => ValueTypeConverter.GetValueByType(possibleType, parameter.name);
    }
}
