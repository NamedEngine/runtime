using System;
using System.Collections.Generic;
using System.Linq;

namespace Rules.Logic {
    public class ChainableNode : ILogicChecker {
        static bool IsChainable(ParsedNodeInfo nodeInfo) {
            return nodeInfo.type == NodeType.Action || nodeInfo.type == NodeType.Condition;
        }
        static void CheckNext(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var improperNodes = parsedNodes.Values
                .Where(IsChainable)
                .Select(info => (info, info.next
                    .Select(nextId => parsedNodes[nextId])
                    .Where(nextInfo => !IsChainable(nextInfo)
                                       && nextInfo.type != NodeType.State)
                    .ToArray()))
                .Where(pair => pair.Item2.Length > 0);

            foreach (var (nodeInfo, improperNext) in improperNodes) {
                var message = $"{nodeInfo.ToNameAndType()} can't be connected to following blocks:";
                foreach (var nextInfo in improperNext) {
                    message += $"\n\"{nextInfo.type}\"- \"{nextInfo.name}\"";
                }
                message += "\nConditions & actions can only be connected to states, conditions & actions";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }
        
        static void CheckPrev(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var wrongPrevCombinationNodes = parsedNodes.Values
                .Where(IsChainable)
                .Where(info => {
                    var allChainable = info.prev.Length > 0 && info.prev
                        .Select(prevId => parsedNodes[prevId])
                        .All(IsChainable);
                    var oneClass = info.prev.Length == 1 && info.prev
                        .Select(prevId => parsedNodes[prevId])
                        .All(i => i.type == NodeType.Class);
                    var oneState = info.prev.Length == 1 && info.prev
                        .Select(prevId => parsedNodes[prevId])
                        .All(i => i.type == NodeType.State);

                    return !(allChainable || oneClass || oneState);
                });
            
            foreach (var nodeInfo in wrongPrevCombinationNodes) {
                var message =
                    $"{nodeInfo.ToNameAndType()} can't receive connection from following set of blocks:";
                if (nodeInfo.prev.Length == 0) {
                    message += "\nThere are none!";
                }
                foreach (var prevInfo in nodeInfo.prev.Select(prevId => parsedNodes[prevId])) {
                    message += $"\n\"{prevInfo.type}\"- \"{prevInfo.name}\"";
                }

                message += "\nConditions & actions should only receive connections from either one state, one class or any number of conditions & actions";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }

        static void CheckParents(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var improperNodes = parsedNodes.Values
                .Where(IsChainable)
                .Where(info => info.parent != null);

            foreach (var nodeInfo in improperNodes) {
                var message = $"{nodeInfo.ToNameAndType()} should not be a child block\nOnly parameters are child blocks";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }

        static void CheckName(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile, TemporaryInstantiator instantiator) {
            var improperNodes = parsedNodes.Values
                .Where(IsChainable)
                .Where(info => LogicUtils.GetConstrainable(info, parsedNodes, idToFile, instantiator) == null);
            
            foreach (var nodeInfo in improperNodes) {
                var message = $"{nodeInfo.ToNameAndType()} should have a name of an existing {nodeInfo.type}";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }

        static void CheckChildren(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile, TemporaryInstantiator instantiator) {
            var improperNodes = parsedNodes.Values
                .Where(IsChainable)
                .Where(info => info.parameters.Any(id => parsedNodes[id].type != NodeType.Parameter));

            foreach (var nodeInfo in improperNodes) {
                var message = $"{nodeInfo.ToNameAndType()} type should only have\nnodes of \"Parameter\" type as children";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }

            int MandatoryParamsNum(ParsedNodeInfo nodeInfo) {
                var constraints = LogicUtils.GetConstrainable(nodeInfo, parsedNodes, idToFile, instantiator).GetConstraints();
                var optionalNum = constraints
                    .Select(possibleValues => possibleValues.Any(value => !(value is NullValue)))
                    .Count(canBeNull => canBeNull);
                
                return constraints.Length - optionalNum;
            }
            int MaxParamsNum(ParsedNodeInfo nodeInfo) {
                return LogicUtils.GetConstrainable(nodeInfo, parsedNodes, idToFile, instantiator).GetConstraints().Length;
            }
            
            var wrongParamsNumNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Operator)
                .Where(info => info.parameters.Length < MandatoryParamsNum(info)
                               || info.parameters.Length > MaxParamsNum(info));
            
            foreach (var nodeInfo in wrongParamsNumNodes) {
                var message = $"{nodeInfo.ToNameAndType()} should have n parameters," +
                              $"\n where {MandatoryParamsNum(nodeInfo)} <= n <= {MaxParamsNum(nodeInfo)}";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
            
        }

        public List<Action<Dictionary<string, ParsedNodeInfo>, Dictionary<string, string>, TemporaryInstantiator>> GetCheckerMethods() {
            return new List<Action<Dictionary<string, ParsedNodeInfo>, Dictionary<string, string>, TemporaryInstantiator>>{
                (parsedNodes, idToFile, instantiator) => CheckNext(parsedNodes, idToFile),
                (parsedNodes, idToFile, instantiator) => CheckPrev(parsedNodes, idToFile),
                (parsedNodes, idToFile, instantiator) => CheckParents(parsedNodes, idToFile),
                CheckName,
                CheckChildren,
            };
        }
    }
}