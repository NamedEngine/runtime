using System;
using System.Collections.Generic;
using System.Linq;

namespace Rules.Logic {
    public class ClassRefNode : ILogicChecker {
        static void CheckNext(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.ClassRef)
                .Select(info => (info, info.next
                    .Select(nextId => parsedNodes[nextId])
                    .Where(nextInfo => nextInfo.type != NodeType.Parameter
                                       && nextInfo.type != NodeType.VariableRef
                                       && nextInfo.type != NodeType.Class)
                    .ToArray()))
                .Where(pair => pair.Item2.Length > 0);

            foreach (var (nodeInfo, improperNext) in improperNodes) {
                var message = $"{nodeInfo.ToNameAndType()} can't be connected to following blocks:";
                foreach (var nextInfo in improperNext) {
                    message += $"\n\"{nextInfo.type}\"- \"{nextInfo.name}\"";
                }
                message += "\nClass references can only be connected to parameters, variable references & classes";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }

            var notEnoughNextNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.ClassRef)
                .Where(info => info.next.Length == 0);
            
            foreach (var nodeInfo in notEnoughNextNodes) {
                var message = $"{nodeInfo.ToNameAndType()} should be connected to at least one"
                              + "\nparameter, variable reference or class";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }

        static void CheckPrev(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.ClassRef)
                .Select(info => (info, info.prev
                    .Select(prevId => parsedNodes[prevId])
                    .ToArray()))
                .Where(pair => pair.Item2.Length > 0);

            foreach (var (nodeInfo, improperPrev) in improperNodes) {
                var message =
                    $"{nodeInfo.ToNameAndType()} can't receive connection from following blocks:";
                foreach (var prevInfo in improperPrev) {
                    message += $"\n\"{prevInfo.type}\"- \"{prevInfo.name}\"";
                }

                message += "\nClass references can't receive connections";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }

        static void CheckParents(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.ClassRef)
                .Where(info => info.parent != null);

            foreach (var nodeInfo in improperNodes) {
                var message = $"{nodeInfo.ToNameAndType()} should not be a child block\nOnly parameters are child blocks";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }

        static void CheckChildren(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.ClassRef)
                .Where(info => info.parameters.Length != 0);

            foreach (var nodeInfo in improperNodes) {
                var message = $"{nodeInfo.ToNameAndType()} should not have child blocks" +
                              "\nOnly variables, operators, conditions & actions have those";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }

        static void CheckName(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.ClassRef)
                .Where(info => info.name != ""
                               && parsedNodes.Values.Count(i => i.type == NodeType.Class && i.name == info.name) == 0);

            foreach (var nodeInfo in improperNodes) {
                var message = $"{nodeInfo.ToNameAndType()} should refer to an existing class";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }

        public List<Action<Dictionary<string, ParsedNodeInfo>, Dictionary<string, string>, TemporaryInstantiator>> GetCheckerMethods() {
            return new List<Action<Dictionary<string, ParsedNodeInfo>, Dictionary<string, string>, TemporaryInstantiator>>{
                (parsedNodes, idToFile, instantiator) => CheckNext(parsedNodes, idToFile),
                (parsedNodes, idToFile, instantiator) => CheckPrev(parsedNodes, idToFile),
                (parsedNodes, idToFile, instantiator) => CheckParents(parsedNodes, idToFile),
                (parsedNodes, idToFile, instantiator) => CheckChildren(parsedNodes, idToFile),
                (parsedNodes, idToFile, instantiator) => CheckName(parsedNodes, idToFile),
            };
        }
    }
}
