using System.Collections.Generic;
using System.Linq;

namespace Rules.Logic {
    public static class ClassNode {
        public static void CheckNext(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Class)
                .Select(info => (info, info.next
                    .Select(nextId => parsedNodes[nextId])
                    .Where(nextInfo => nextInfo.type == NodeType.Operator
                                       || nextInfo.type == NodeType.Parameter
                                       || nextInfo.type == NodeType.ClassRef
                                       || nextInfo.type == NodeType.VariableRef)
                    .ToArray()))
                .Where(pair => pair.Item2.Length > 0)
                .ToArray();

            foreach (var (nodeInfo, improperNext) in improperNodes) {
                var message = $"\"{nodeInfo.name}\" block of \"{nodeInfo.type}\" type can't be connected to following blocks:";
                foreach (var nextInfo in improperNext) {
                    message += $"\n\"{nextInfo.type}\"- \"{nextInfo.name}\"";
                }
                message += "\nClasses can only be connected to children classes,\nstates, variables, conditions & actions";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }
        
        public static void CheckPrev(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Class)
                .Select(info => (info, info.prev
                    .Select(prevId => parsedNodes[prevId])
                    .Where(prevInfo => prevInfo.type != NodeType.Class && prevInfo.type != NodeType.ClassRef)
                    .ToArray()))
                .Where(pair => pair.Item2.Length > 0)
                .ToArray();

            foreach (var (nodeInfo, improperPrev) in improperNodes) {
                var message = $"\"{nodeInfo.name}\" block of \"{nodeInfo.type}\" type can't receive connection from following blocks:";
                foreach (var prevInfo in improperPrev) {
                    message += $"\n\"{prevInfo.type}\"- \"{prevInfo.name}\"";
                }
                message += "\nIt can only receive a connection from a parent class or its reference";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }

            var tooManyPrevNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Class)
                .Where(info => info.prev.Length > 1)
                .ToArray();
            
            foreach (var nodeInfo in tooManyPrevNodes) {
                var message = $"\"{nodeInfo.name}\" block of \"{nodeInfo.type}\" type can only have one parent class";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }
        
        public static void CheckParents(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Class)
                .Where(info => info.parent != null)
                .ToArray();

            foreach (var nodeInfo in improperNodes) {
                var message = $"\"{nodeInfo.name}\" block of \"{nodeInfo.type}\" type should not be a child block\nOnly parameters are child blocks";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }
        
        public static void CheckChildren(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Class)
                .Where(info => info.parent != null)
                .ToArray();

            foreach (var nodeInfo in improperNodes) {
                var message = $"\"{nodeInfo.name}\" block of \"{nodeInfo.type}\" type should not have children blocks" +
                              "\nOnly variables, operators, conditions & actions have those";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }
        
        public static void CheckName(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.Class)
                .Where(info => info.name == ""
                               || parsedNodes.Values.Count(i => i.type == NodeType.Class && i.name == info.name) > 1)
                .ToArray();

            foreach (var nodeInfo in improperNodes) {
                var message = $"\"{nodeInfo.name}\" block of \"{nodeInfo.type}\" type ";
                message += nodeInfo.name == "" ? "has an empty name" : "doesn't have a unique name";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }
    }
}