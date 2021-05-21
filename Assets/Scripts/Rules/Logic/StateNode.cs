using System;
using System.Collections.Generic;
using System.Linq;

namespace Rules.Logic {
    public class StateNode : ILogicChecker {
        static void CheckNext(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.State)
                .Select(info => (info, info.next
                    .Select(nextId => parsedNodes[nextId])
                    .Where(nextInfo => nextInfo.type != NodeType.Condition
                                       && nextInfo.type != NodeType.Action)
                    .ToArray()))
                .Where(pair => pair.Item2.Length > 0);

            foreach (var (nodeInfo, improperNext) in improperNodes) {
                var message = $"{nodeInfo.ToNameAndType()} can't be connected to following blocks:";
                foreach (var nextInfo in improperNext) {
                    message += $"\n\"{nextInfo.type}\"- \"{nextInfo.name}\"";
                }
                message += "\nStates can only be connected to conditions & actions";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }

        static void CheckPrev(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.State)
                .Select(info => (info, info.prev
                    .Select(prevId => parsedNodes[prevId])
                    .Where(prevInfo => prevInfo.type != NodeType.Class
                                       && prevInfo.type != NodeType.Condition
                                       && prevInfo.type != NodeType.Action)
                    .ToArray()))
                .Where(pair => pair.Item2.Length > 0 || pair.Item1.prev.Length == 0);

            foreach (var (nodeInfo, improperPrev) in improperNodes) {
                var message =
                    $"{nodeInfo.ToNameAndType()} can't receive connection from following blocks:";
                if (nodeInfo.prev.Length == 0) {
                    message += "\nThere are none!";
                }
                foreach (var prevInfo in improperPrev) {
                    message += $"\n\"{prevInfo.type}\"- \"{prevInfo.name}\"";
                }

                message += "\nStates should only receive connections from either one class or any number of conditions & actions";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
            
            var tooManyClassesNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.State)
                .Where(info => info.prev
                    .Select(prevId => parsedNodes[prevId])
                    .Count(prevInfo => prevInfo.type == NodeType.Class) > 1);
            
            foreach (var nodeInfo in tooManyClassesNodes) {
                var message = $"{nodeInfo.ToNameAndType()} can't be related to more then 1 class";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }

        static void CheckParents(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.State)
                .Where(info => info.parent != null);

            foreach (var nodeInfo in improperNodes) {
                var message = $"{nodeInfo.ToNameAndType()} should not be a child block\nOnly parameters are child blocks";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }

        static void CheckChildren(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.State)
                .Where(info => info.parent != null);

            foreach (var nodeInfo in improperNodes) {
                var message = $"{nodeInfo.ToNameAndType()} should not have child blocks" +
                              "\nOnly variables, operators, conditions & actions have those";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }

        static void CheckName(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var classStates = parsedNodes.Values
                .Where(info => info.type == NodeType.Class)
                .Select(info => LogicUtils
                    .GetAllSuccessors(info, parsedNodes)
                    .Where(succInfo => succInfo.type == NodeType.State)
                    .ToDictionary(succInfo => succInfo.id, succInfo => succInfo))
                .ToArray();

            Dictionary<string, ParsedNodeInfo> GetClassStates(ParsedNodeInfo nodeInfo) {
                var states = classStates.FirstOrDefault(stateDict => stateDict.ContainsKey(nodeInfo.id));
                if (states == default) {
                    var message =
                        $"{nodeInfo.ToNameAndType()} should be connected in some way to a class";
                    throw new LogicParseException(idToFile[nodeInfo.id], message);
                }
                
                return states;
            }

            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.State)
                .Where(info => info.name != ""
                               && GetClassStates(info).Values.Count(stateInfo => stateInfo.name == info.name) > 1);
                               
            foreach (var nodeInfo in improperNodes) {
                var message = $"{nodeInfo.ToNameAndType()} doesn't have a unique name";

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
