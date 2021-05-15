using System;
using System.Collections.Generic;
using System.Linq;

namespace Rules.Logic {
    public class VariableRefNode : ILogicChecker {
        static void CheckNext(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.VariableRef)
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
                message += "\nVariable references can only be connected to parameters";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }

            var notEnoughNextNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.VariableRef)
                .Where(info => info.next.Length == 0);
            
            foreach (var nodeInfo in notEnoughNextNodes) {
                var message = $"{nodeInfo.ToNameAndType()} should be connected to at least one\nparameter";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }

        static void CheckPrev(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.VariableRef)
                .Select(info => (info, info.prev
                    .Select(prevId => parsedNodes[prevId])
                    .Where(prevInfo => prevInfo.type != NodeType.Class
                                       && prevInfo.type != NodeType.ClassRef)
                    .ToArray()))
                .Where(pair => pair.Item2.Length > 0);

            foreach (var (nodeInfo, improperPrev) in improperNodes) {
                var message =
                    $"{nodeInfo.ToNameAndType()} can't receive connection from following blocks:";
                foreach (var prevInfo in improperPrev) {
                    message += $"\n\"{prevInfo.type}\"- \"{prevInfo.name}\"";
                }

                message += "\nIt can only receive a connection from either a related class or its reference";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
            
            var tooManyPrevNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.VariableRef)
                .Where(info => info.prev.Length > 1);
            
            foreach (var nodeInfo in tooManyPrevNodes) {
                var message = $"{nodeInfo.ToNameAndType()} can only have one related class";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }

        static void CheckParents(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.VariableRef)
                .Where(info => info.parent != null);

            foreach (var nodeInfo in improperNodes) {
                var message = $"{nodeInfo.ToNameAndType()} should not be a child block\nOnly parameters are child blocks";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }

        static void CheckChildren(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.VariableRef)
                .Where(info => info.parameters.Length != 0);

            foreach (var nodeInfo in improperNodes) {
                var message = $"{nodeInfo.ToNameAndType()} should not have child blocks" +
                              "\nOnly variables, operators, conditions & actions have those";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }

        static void CheckName(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile, TemporaryInstantiator instantiator) {
            var (classVariables, baseClassVariables) = LogicUtils.GetAllNamedVariables(parsedNodes, instantiator);

            bool ClassHasVariable(string className, string variableName) {
                if (baseClassVariables.ContainsKey(className)) {
                    return baseClassVariables[className].ContainsKey(variableName);
                }

                var has = classVariables[className].ContainsKey(variableName);
                return has || ClassHasVariable(LogicUtils.GetBaseClassByClass(className, parsedNodes), variableName);
            }

            var improperNodes = parsedNodes.Values
                .Where(info => info.type == NodeType.VariableRef)
                .Select(info => (info, parsedNodes[info.prev.First()].name))
                .Where(pair => !ClassHasVariable(pair.Item2, pair.Item1.name));

            foreach (var (nodeInfo, className) in improperNodes) {
                var message = $"{nodeInfo.ToNameAndType()} should refer to an existing variable\nof class {className}";

                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }

        public List<Action<Dictionary<string, ParsedNodeInfo>, Dictionary<string, string>, TemporaryInstantiator>> GetCheckerMethods() {
            return new List<Action<Dictionary<string, ParsedNodeInfo>, Dictionary<string, string>, TemporaryInstantiator>>{
                (parsedNodes, idToFile, instantiator) => CheckNext(parsedNodes, idToFile),
                (parsedNodes, idToFile, instantiator) => CheckPrev(parsedNodes, idToFile),
                (parsedNodes, idToFile, instantiator) => CheckParents(parsedNodes, idToFile),
                (parsedNodes, idToFile, instantiator) => CheckChildren(parsedNodes, idToFile),
                CheckName,
            };
        }
    }
}
