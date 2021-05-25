using System;
using System.Collections.Generic;
using System.Linq;

namespace Rules.Logic {
    public class NodeGroup : ILogicChecker {
        static bool IsChainable(ParsedNodeInfo info) {
            return info.type == NodeType.Condition
                   || info.type == NodeType.Action;
        }

        static void CycleCheck(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            bool IsFirstChainable(string id) {
                var info = parsedNodes[id];
                return IsChainable(info)
                       && info.prev.Length == 1
                       && !IsChainable(parsedNodes[info.prev.First()]);
            }

            var cycles = parsedNodes
                .Select(pair => pair.Key)
                        // not IsFirstOperator cause if there are none rules won't prohibit it (unlike with chainables)
                .Where(id => IsFirstChainable(id) || parsedNodes[id].type == NodeType.Operator)  
                .Select(startId => {
                    // Debug.Log($"First chainable or operator: {parsedNodes[startId].name}");
                    string[] GetNext(string id) {
                        var info = parsedNodes[id];
                        if (IsChainable(info)) {
                            return info.next
                                .Where(nextId => IsChainable(parsedNodes[nextId]))
                                .ToArray();
                        }

                        // operator
                        return info.next
                            .Select(nextId => parsedNodes[nextId].parent)
                            .Where(parentId => parentId != null
                                               && parsedNodes[parentId].type == NodeType.Operator)
                            .ToArray();
                    }

                    List<string> HasCycleInNext(string id, List<string> visitedIds) {
                        if (visitedIds.Count > 10) {
                            return null;
                        }
                        var index = visitedIds.IndexOf(id);
                        if (index != -1) {
                            return visitedIds.GetRange(index, visitedIds.Count - index);
                        }

                        var next = GetNext(id);
                        if (next.Length == 0) {
                            return null;
                        }

                        var newVisitedIds = visitedIds.Append(id).ToList();
                        return next
                            .Select(nextId => HasCycleInNext(nextId, newVisitedIds))
                            .FirstOrDefault(cycleList => cycleList != null);
                    }

                    return HasCycleInNext(startId, new List<string>());
                })
                .Where(cycle => cycle != null);

            foreach (var cycle in cycles) {
                throw new LogicParseException(idToFile[cycle.First()],
                    "File contains a cycle that includes following nodes:\n" +
                    string.Join("\n", cycle.Select(id => parsedNodes[id].ToNameAndType())));
            }
        }
        
        static void OneRelatedClassCheck(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            var nodeClasses = new Dictionary<string, string>();
            
            idToFile
                .GroupBy(pair => pair.Value)
                .ToDictionary(group => group.Key,
                    group => group
                        .Select(pair => pair.Key)
                        .Where(id => parsedNodes[id].type != NodeType.Parameter)
                        .ToArray())
                .ToList()
                .ForEach(pair => {
                    var (file, ids) = pair;
                    
                    string GetRelatedClass(string id, string[] visitedNodes) {
                        if (visitedNodes.Contains(id)) {
                            return null;
                        }
                        visitedNodes = visitedNodes.Append(id).ToArray();
                        
                        if (nodeClasses.ContainsKey(id)) {
                            return nodeClasses[id];
                        }
                        
                        var node = parsedNodes[id];
                        if (node.type == NodeType.ClassRef) {
                            return null;
                        }
                        
                        var possibleClasses = node.prev
                            .Select(prevId => parsedNodes[prevId])
                            .Where(prevNode => prevNode.type == NodeType.Class)
                            .Select(prevNode => prevNode.name)
                            .ToList();

                        var prev = node.prev
                            .Where(prevId => parsedNodes[prevId].type != NodeType.Class)
                            .ToList();
                        var paramPrev = node.parameters
                            .Select(paramId => parsedNodes[paramId].prev
                                .Where(paramPrevId => parsedNodes[paramPrevId].type != NodeType.Class
                                                      && parsedNodes[paramPrevId].type != NodeType.ClassRef
                                                      && parsedNodes[paramPrevId].type != NodeType.VariableRef))
                            .SelectMany(paramPrevId => paramPrevId);
                        
                        prev.AddRange(paramPrev);

                        var possibleClasseFromPrev = prev
                            .Select(prevId => GetRelatedClass(prevId, visitedNodes))
                            .Where(possibleClass => possibleClass != null);
                        
                        possibleClasses.AddRange(possibleClasseFromPrev);

                        if (possibleClasses.Count == 0) {
                            return null;
                        }

                        // if (possibleClasses.Any(possibleClass => possibleClass != possibleClasses.First())) {
                        if (possibleClasses.ToHashSet().Count != 1) {
                            var message = "File contains a block that is related to multiple classes:" +
                                          $"\n{node.ToNameAndType()} has these related classes:\n";
                            message += string.Join(", ",
                                possibleClasses.ToHashSet());
                            throw new LogicParseException(file, message);
                        }

                        var relatedClass = possibleClasses.First();
                        nodeClasses[id] = relatedClass;

                        return relatedClass;
                    }

                    foreach (var startId in ids) {
                        GetRelatedClass(startId, new string[] { });
                    }
                });
        }

        static void OneRelatedChainCheck(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile) {
            HashSet<string> GetChainStartIds(string id, string nextId) {
                var node = parsedNodes[id];
                if (!IsChainable(node)) {
                    var res = new HashSet<string>();
                    if (nextId != null) {
                        res.Add(nextId);
                    }
                    return res;
                }

                return node.prev
                    .Select(prevId => GetChainStartIds(prevId, id))
                    .Aggregate(new HashSet<string>(), (acc, val) => {
                        acc.UnionWith(val);
                        return acc;
                    });
            }

            var tooManyRelatedChainsNodes = parsedNodes.Values
                .Where(IsChainable)
                .Select(info => (info, GetChainStartIds(info.id, null)))
                .Where(pair => pair.Item2.Count > 1);

            foreach (var (nodeInfo, relatedChainStarts) in tooManyRelatedChainsNodes) {
                var message = nodeInfo.ToNameAndType() + " has multiple related chains\nwhich start from following blocks:\n";
                message += string.Join("\n", relatedChainStarts.Select(id => parsedNodes[id].ToNameAndType()));
                throw new LogicParseException(idToFile[nodeInfo.id], message);
            }
        }

        public List<Action<Dictionary<string, ParsedNodeInfo>, Dictionary<string, string>, TemporaryInstantiator>> GetCheckerMethods() {
            return new List<Action<Dictionary<string, ParsedNodeInfo>, Dictionary<string, string>, TemporaryInstantiator>>{
                (parsedNodes, idToFile, instantiator) => CycleCheck(parsedNodes, idToFile),
                (parsedNodes, idToFile, instantiator) => OneRelatedClassCheck(parsedNodes, idToFile),
                (parsedNodes, idToFile, instantiator) => OneRelatedChainCheck(parsedNodes, idToFile),
            };
        } 
    }
}