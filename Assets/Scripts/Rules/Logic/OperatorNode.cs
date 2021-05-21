using System;
using System.Collections.Generic;
using System.Linq;

namespace Rules.Logic {
    public class OperatorNode : ILogicChecker {
        static void CheckNext(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Operator)
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
                message += "\nOperators can only be connected to parameters";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
            
            var notEnoughNextNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Operator)
                .Where(info => info.next.Length == 0);
            
            foreach (var nodeInfo in notEnoughNextNodes) {
                var message = $"{nodeInfo.ToNameAndType()} should be connected to at least one\nparameter";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }
        
        static void CheckPrev(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Operator)
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

                message += "\nOperators can't receive connections";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }

        static void CheckParents(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Operator)
                .Where(info => info.parent != null);

            foreach (var nodeInfo in improperNodes) {
                var message = $"{nodeInfo.ToNameAndType()} should not be a child block\nOnly parameters are child blocks";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }

        static void CheckName(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile, TemporaryInstantiator instantiator) {
            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Operator)
                .Where(info => LogicUtils.GetConstrainable(info, parsedNodes, idToFile, instantiator) == null);
            
            foreach (var nodeInfo in improperNodes) {
                var message = $"{nodeInfo.ToNameAndType()} should have a name of an existing {nodeInfo.type}";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }

        static void CheckChildren(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile, TemporaryInstantiator instantiator) {
            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Operator)
                .Where(info => info.parameters.Any(id => parsedNodes[id].type != NodeType.Parameter));

            foreach (var nodeInfo in improperNodes) {
                var message = $"{nodeInfo.ToNameAndType()} should only have\nnodes of \"Parameter\" type as children";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }

            int MandatoryParamsNum(ParsedNodeInfo info) =>
                LogicUtils.MandatoryParamsNum(info, parsedNodes, idToFile, instantiator);
            int MaxParamsNum(ParsedNodeInfo info) =>
                LogicUtils.MaxParamsNum(info, parsedNodes, idToFile, instantiator);
            
            var wrongParamsNumNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Operator)
                .Where(info => info.parameters.Length < MandatoryParamsNum(info) || info.parameters.Length > MaxParamsNum(info));
            
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