using System;
using System.Collections.Generic;
using System.Linq;

namespace Rules.Logic {
    public class ClassNode : ILogicChecker {
        static void CheckNext(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Class)
                .Select(info => (info, info.next
                    .Select(nextId => parsedNodes[nextId])
                    .Where(nextInfo => nextInfo.type == NodeType.Operator
                                       || nextInfo.type == NodeType.ClassRef)
                    .ToArray()))
                .Where(pair => pair.Item2.Length > 0);

            foreach (var (nodeInfo, improperNext) in improperNodes) {
                var message = $"{nodeInfo.ToNameAndType()} can't be connected to following blocks:";
                foreach (var nextInfo in improperNext) {
                    message += $"\n\"{nextInfo.type}\"- \"{nextInfo.name}\"";
                }
                message += "\nClasses can only be connected to children classes,\nstates, variables, parameters, conditions & actions";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }

            var tooManyDefaultStatesNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Class)
                .Where(info => info.next
                    .Select(nextId => parsedNodes[nextId])
                    .Count(nextInfo => nextInfo.type == NodeType.State) > 1);
            
            foreach (var nodeInfo in tooManyDefaultStatesNodes) {
                var message = $"{nodeInfo.ToNameAndType()} can't have more the 1 default state";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }
        
        static void CheckPrev(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Class)
                .Select(info => (info, info.prev
                    .Select(prevId => parsedNodes[prevId])
                    .Where(prevInfo => prevInfo.type != NodeType.Class
                                       && prevInfo.type != NodeType.ClassRef)
                    .ToArray()))
                .Where(pair => pair.Item2.Length > 0);

            foreach (var (nodeInfo, improperPrev) in improperNodes) {
                var message = $"{nodeInfo.ToNameAndType()} can't receive connection from following blocks:";
                foreach (var prevInfo in improperPrev) {
                    message += $"\n\"{prevInfo.type}\"- \"{prevInfo.name}\"";
                }
                message += "\nIt can only receive a connection from either a parent class or its reference";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }

            var tooManyPrevNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Class)
                .Where(info => info.prev.Length > 1);
            
            foreach (var nodeInfo in tooManyPrevNodes) {
                var message = $"{nodeInfo.ToNameAndType()} can only have one parent class";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }
        
        static void CheckParents(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Class)
                .Where(info => info.parent != null);

            foreach (var nodeInfo in improperNodes) {
                var message = $"{nodeInfo.ToNameAndType()} should not be a child block\nOnly parameters are child blocks";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }
        
        static void CheckChildren(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Class)
                .Where(info => info.parameters.Length != 0);

            foreach (var nodeInfo in improperNodes) {
                var message = $"{nodeInfo.ToNameAndType()} should not have child blocks" +
                              "\nOnly variables, operators, conditions & actions have those";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }
        
        static void CheckName(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Class)
                .Where(info => info.name == ""
                               || parsedNodes.Values.Count(i => i.type == NodeType.Class && i.name == info.name) > 1);

            foreach (var nodeInfo in improperNodes) {
                var message = $"{nodeInfo.ToNameAndType()} ";
                message += nodeInfo.name == "" ? "has an empty name" : "doesn't have a unique name";

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
