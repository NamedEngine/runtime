using System;
using System.Collections.Generic;
using System.Linq;

namespace Rules.Logic {
    public class VariableNode : ILogicChecker {
        static void CheckNext(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Variable)
                .Select(info => (info, info.next
                    .Select(nextId => parsedNodes[nextId])
                    .Where(nextInfo => nextInfo.type != NodeType.Parameter)
                    .ToArray()))
                .Where(pair => pair.Item2.Length > 0);

            foreach (var (nodeInfo, improperNext) in improperNodes) {
                var message = $"{nodeInfo.ToNameAndType()} can't be connected to following blocks:";
                foreach (var nextInfo in improperNext) {
                    message += $"\n\"{nextInfo.type}\"- \"{nextInfo.name}\"";
                }
                message += "\nVariables can only be connected to parameters";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }
        
        static void CheckPrev(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Variable)
                .Select(info => (info, info.prev
                    .Select(prevId => parsedNodes[prevId])
                    .Where(prevInfo => prevInfo.type != NodeType.Class)
                    .ToArray()))
                .Where(pair => pair.Item2.Length > 0);

            foreach (var (nodeInfo, improperPrev) in improperNodes) {
                var message =
                    $"{nodeInfo.ToNameAndType()} can't receive connection from following blocks:";
                foreach (var prevInfo in improperPrev) {
                    message += $"\n\"{prevInfo.type}\"- \"{prevInfo.name}\"";
                }

                message += "\nIt can only receive a connection from a related class";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
            
            var tooManyPrevNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Variable)
                .Where(info => info.prev.Length != 1);
            
            foreach (var nodeInfo in tooManyPrevNodes) {
                var message = $"{nodeInfo.ToNameAndType()} should have exactly one related class";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }

        static void CheckParents(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Variable)
                .Where(info => info.parent != null);

            foreach (var nodeInfo in improperNodes) {
                var message = $"{nodeInfo.ToNameAndType()} should not be a child block\nOnly parameters are child blocks";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }

        static void CheckName(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile, TemporaryInstantiator instantiator) {
            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Variable)
                .Where(info => !LogicUtils.GetVariableTypeAndName(info.name).HasValue);

            foreach (var nodeInfo in improperNodes) {
                var message = $"\"{nodeInfo.name}\" block of \"{nodeInfo.type}\" has improper label formatting\nExample: \"Type: Name\" or \"Type\"";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }

            var notUniqueVariables = parsedNodes.Values
                .Where(info => info.type == NodeType.Variable)
                .GroupBy(info => info.prev.First())
                .Select(gr => (parsedNodes[gr.Key], gr
                    .GroupBy(node => LogicUtils.GetVariableTypeAndName(node.name).Value.Item2)
                    .Where(sameNameGroup => sameNameGroup.Count() > 1)
                    .Select(sameNameGroup => sameNameGroup.Key)
                    .ToArray()
                ))
                .Where(pair => pair.Item2.Length > 0);

            foreach (var (classNode, variableNames) in notUniqueVariables) {
                var message = $"{classNode.name} class has several variables with same names:\n{string.Join(", ", variableNames)}";

                throw new LogicParseException(idToFile[classNode.id], message);
            }

            var wrongTypedNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Variable)
                .Select(info => (info, parsedNodes[info.prev.First()].name))
                .Where(pair => {
                    var (info, className) = pair;
                    var (type, variableName) = LogicUtils.GetVariableTypeAndName(info.name).Value;
                    var originalType = LogicUtils.GetVariableType(variableName, LogicUtils.GetBaseClassByClass(className, parsedNodes), parsedNodes, instantiator);

                    return originalType != ValueType.Null && type != originalType;
                });

            foreach (var (nodeInfo, className) in wrongTypedNodes) {
                var (_, variableName) = LogicUtils.GetVariableTypeAndName(nodeInfo.name).Value;
                var originalType = LogicUtils.GetVariableType(variableName, LogicUtils.GetBaseClassByClass(className, parsedNodes), parsedNodes, instantiator);
                var message = $"{nodeInfo.ToNameAndType()} has wrong type:\nthis variable has \"{originalType}\" type in a base class";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }

        static void CheckChildren(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Variable)
                .Where(info => info.parameters.Length > 1
                               || info.parameters.Any(id => parsedNodes[id].type != NodeType.Parameter));

            foreach (var nodeInfo in improperNodes) {
                var message = $"{nodeInfo.ToNameAndType()} should have have 0 or 1\nchild node of \"Parameter\" type";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }

        public List<Action<Dictionary<string, ParsedNodeInfo>, Dictionary<string, string>, TemporaryInstantiator>> GetCheckerMethods() {
            return new List<Action<Dictionary<string, ParsedNodeInfo>, Dictionary<string, string>, TemporaryInstantiator>>{
                (parsedNodes, idToFile, instantiator) => CheckNext(parsedNodes, idToFile),
                (parsedNodes, idToFile, instantiator) => CheckPrev(parsedNodes, idToFile),
                (parsedNodes, idToFile, instantiator) => CheckParents(parsedNodes, idToFile),
                CheckName,
                (parsedNodes, idToFile, instantiator) => CheckChildren(parsedNodes, idToFile),
            };
        }
    }
}
