using System;
using System.Collections.Generic;
using System.Linq;

namespace Rules.Logic {
    public class ParameterNode : ILogicChecker {
        static void CheckParents(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var parentlessNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Parameter)
                .Where(info => info.parent == null);
            
            foreach (var nodeInfo in parentlessNodes) {
                var message = $"{nodeInfo.ToNameAndType()} should have a parent block";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
            
            var wrongParentNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Parameter)
                .Where(info => parsedNodes[info.parent].type != NodeType.Variable
                               && parsedNodes[info.parent].type != NodeType.Operator
                               && parsedNodes[info.parent].type != NodeType.Condition
                               && parsedNodes[info.parent].type != NodeType.Action);
            
            foreach (var nodeInfo in wrongParentNodes) {
                var parentInfo = parsedNodes[nodeInfo.parent];
                var message = $"{nodeInfo.ToNameAndType()} should not be a child of" +
                              $"\n({parentInfo.ToNameAndType()})" +
                              "\nOnly variables, operators, conditions & actions can have child blocks";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }

        static string NameWithParent(ParsedNodeInfo parameter, Dictionary<string, ParsedNodeInfo> parsedNodes) {
            var parentInfo = parsedNodes[parameter.parent];
            var paramIndex = parentInfo.parameters.ToList().IndexOf(parameter.id);
            return
                $"{parameter.ToNameAndType()} (#{paramIndex})(parent: {parentInfo.ToNameAndType()})";
        }
        
        static void CheckNext(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Parameter)
                .Select(info => (info, info.next
                    .Select(nextId => parsedNodes[nextId])
                    .ToArray()))
                .Where(pair => pair.Item2.Length > 0);

            foreach (var (nodeInfo, improperNext) in improperNodes) {
                var message = NameWithParent(nodeInfo, parsedNodes) + "\ncan't be connected to following blocks:";
                foreach (var nextInfo in improperNext) {
                    message += $"\n\"{nextInfo.type}\"- \"{nextInfo.name}\"";
                }

                message += "\nParameters can't be connected to any block";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }

        static void CheckName(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile,
            TemporaryInstantiator instantiator) {
            var cantCastValuesNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Parameter)
                .Where(info => info.prev.Length == 0)
                .Select(info => {
                    var parentInfo = parsedNodes[info.parent];
                    
                    ValueType[] types;
                    switch (parentInfo.type) {
                        case NodeType.Variable:
                            var variableType = LogicUtils.GetVariableTypeAndName(parentInfo.name).Value.Item1;
                            types = new [] {variableType};
                            break;
                        case NodeType.Action:
                        case NodeType.Condition:
                        case NodeType.Operator:
                            var parameterIndex = parentInfo.parameters.ToList().IndexOf(info.id);
                            types = LogicUtils
                                .GetConstrainable(parentInfo, parsedNodes, idToFile, instantiator)
                                .GetConstraints()[parameterIndex]
                                .Select(value => value.GetValueType())
                                .ToArray();
                            break;
                        default:
                            throw new ArgumentException("This should not be possible!");
                    }

                    var possibleTypes = types
                        .Where(t => t != ValueType.Null)
                        .ToArray();

                    return (info, possibleTypes);
                })
                .Where(pair => {
                    var (info, possibleTypes) = pair;
                    return possibleTypes.All(t => ValueTypeConverter.GetValueByType(t, info.name) == null);
                });
            
            foreach (var (nodeInfo, possibleTypes) in cantCastValuesNodes) {
                var message = NameWithParent(nodeInfo, parsedNodes);
                if (possibleTypes.Length == 0) {
                    message += "\ncan't use its label as a source value";
                } else {
                    message += "\ncan't cast its label to any of the following possible types:\n";
                    message += string.Join(", ", possibleTypes);
                }

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }

        static void CheckPrev(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile,
            TemporaryInstantiator instantiator) {
            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Parameter)
                .Select(info => (info, info.prev
                    .Select(prevId => parsedNodes[prevId])
                    .Where(prevInfo => prevInfo.type != NodeType.Variable
                                       && prevInfo.type != NodeType.VariableRef
                                       && prevInfo.type != NodeType.Operator
                                       && prevInfo.type != NodeType.ClassRef
                                       && prevInfo.type != NodeType.Class)
                    .ToArray()))
                .Where(pair => pair.Item2.Length > 0);

            foreach (var (nodeInfo, improperPrev) in improperNodes) {
                var message = NameWithParent(nodeInfo, parsedNodes) + "\ncan't receive connections from following blocks:";
                foreach (var prevInfo in improperPrev) {
                    message += $"\n\"{prevInfo.type}\"- \"{prevInfo.name}\"";
                }
                message += "\nA parameter can only receive a connection from a variable or its reference,\nan operator, a class or its reference";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
            
            var tooManyPrevNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Parameter)
                .Where(info => parsedNodes[info.parent].type != NodeType.Variable)
                .Where(info => info.prev.Length > 1);
            
            foreach (var nodeInfo in tooManyPrevNodes) {
                var message = NameWithParent(nodeInfo, parsedNodes) + "\nshould only receive connection from one block";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
            
            var connectedVariableParameterNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Parameter)
                .Where(info => parsedNodes[info.parent].type == NodeType.Variable)
                .Where(info => info.prev.Length > 0);
            
            foreach (var nodeInfo in connectedVariableParameterNodes) {
                var message = NameWithParent(nodeInfo, parsedNodes) + "\nshould not receive any connections";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }

            var cantCastValuesNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Parameter)
                .Where(info => info.prev.Length == 1)
                .Select(info => {
                    var prevInfo = parsedNodes[info.prev.First()];

                    IValue prev;
                    switch (prevInfo.type) {
                        case NodeType.Variable:
                            var variableType = LogicUtils.GetVariableTypeAndName(prevInfo.name).Value.Item1;
                            var variableValue = prevInfo.parameters.Length == 1 ? parsedNodes[prevInfo.parameters.First()].name : "";
                            prev = ValueTypeConverter.GetVariableByType(variableType, variableValue);
                            break;
                        case NodeType.VariableRef:
                            var className = parsedNodes[prevInfo.prev.First()].name;
                            var variableRefType =
                                LogicUtils.GetVariableType(prevInfo.name, className, parsedNodes, instantiator);
                            prev = new VariableRef(new ClassRef(className), prevInfo.name, variableRefType);
                            break;
                        case NodeType.Class:
                        case NodeType.ClassRef:
                            prev = new ClassRef(prevInfo.name);
                            break;
                        case NodeType.Operator:
                            prev = LogicUtils.GetConstrainable(prevInfo, parsedNodes, idToFile, instantiator) as IValue;
                            break;
                        default:
                            throw new ArgumentException("This should not be possible!");
                    }
                    
                    return (info, prev);
                })
                .Where(pair => {
                    var (info, prev) = pair;

                    var parentInfo = parsedNodes[info.parent];
                    var paramIndex = parsedNodes[info.parent].parameters.ToList().IndexOf(info.id);
                    
                    var constraints = LogicUtils
                        .GetConstrainable(parentInfo, parsedNodes, idToFile, instantiator)
                        .GetConstraints();
                    return !constraints[paramIndex].Any(value => value.Cast(prev));
                });
            
            foreach (var (nodeInfo, prev) in cantCastValuesNodes) {
                var prevInfo = parsedNodes[nodeInfo.prev.First()];
                
                var message = NameWithParent(nodeInfo, parsedNodes) + 
                              $"\ncan't take its value from {prevInfo.ToNameAndType()}" +
                              $"\nvalue source has type \"{prev.TypeToString()}\", which is not appropriate for this parameter" +
                              "\npossible types: ";
                
                var parentInfo = parsedNodes[nodeInfo.parent];
                var paramIndex = parentInfo.parameters.ToList().IndexOf(nodeInfo.id);
                var possibleTypes = LogicUtils
                    .GetConstrainable(parentInfo, parsedNodes, idToFile, instantiator)
                    .GetConstraints()[paramIndex]
                    .Where(t => !(t is NullValue))
                    .Select(t => t.TypeToString());
                message += string.Join(", ", possibleTypes);

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }


        static void CheckChildren(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Parameter)
                .Where(info => info.parameters.Length != 0);

            foreach (var nodeInfo in improperNodes) {
                var message = NameWithParent(nodeInfo, parsedNodes) + "\nshould not have child blocks" +
                              "\nOnly variables, operators, conditions & actions have those";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }

        public List<Action<Dictionary<string, ParsedNodeInfo>, Dictionary<string, string>, TemporaryInstantiator>> GetCheckerMethods() {
            return new List<Action<Dictionary<string, ParsedNodeInfo>, Dictionary<string, string>, TemporaryInstantiator>>{
                (parsedNodes, idToFile, instantiator) => CheckParents(parsedNodes, idToFile),
                (parsedNodes, idToFile, instantiator) => CheckNext(parsedNodes, idToFile),
                CheckName,
                CheckPrev,
                (parsedNodes, idToFile, instantiator) => CheckChildren(parsedNodes, idToFile),
            };
        }
    }
}
