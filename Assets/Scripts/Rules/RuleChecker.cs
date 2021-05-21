using System.Collections.Generic;
using Rules.Logic;
using Rules.Parsing;

namespace Rules {
    public static class RuleChecker {
        public static void CheckParsing<TParsingChecker, TSource>(TSource source, string file) where TParsingChecker : IParsingChecker<TSource>, new() {
            new TParsingChecker().GetCheckerMethods().ForEach(method => method(source, file));
        } 
        
        public static void CheckLogic(Dictionary<string, ParsedNodeInfo> parsedNodes, Dictionary<string, string> idToFile, TemporaryInstantiator instantiator) {
            var logicCheckers = new List<ILogicChecker> {
                new NodeGroup(),
                
                new ClassNode(),
                new StateNode(),
                new ClassRefNode(),
                new VariableNode(),
                new VariableRefNode(),
                new OperatorNode(),
                new ChainableNode(),
                new ParameterNode(),
            };
            
            logicCheckers.ForEach(lc => lc.GetCheckerMethods().ForEach(method => {
                method(parsedNodes, idToFile, instantiator);
                instantiator.Clear();
            }));
        }
    }
}