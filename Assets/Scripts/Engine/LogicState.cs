using System.Linq;
using UnityEngine;
using VariableDictionary = System.Collections.Generic.Dictionary<string, IVariable>;
using OperatorInstantiator = System.Func<System.Collections.Generic.Dictionary<string, IVariable>, IValue[], IValue>;
using ChainableInstantiator = System.Func<LogicObject, System.Collections.Generic.Dictionary<string, IVariable>, IValue[], Chainable>;

public class LogicState {
    readonly LogicChain[] _logicChains;
    
    public LogicState(LogicChain[] logicChains) {
        _logicChains = logicChains;
    }

    public void ProcessLogic() {
        foreach (var logicChain in _logicChains) {
            logicChain.Execute();
        }
    }

    public void Start(LogicObject thisObject, VariableDictionary variableDictionary) {
        foreach (var logicChain in _logicChains) {
            logicChain.ResetChain(thisObject, variableDictionary);
        }
    }

    public void Finish() {
        foreach (var logicChain in _logicChains) {
            logicChain.Finish();
        }
    }

    public LogicState Clone(LogicObject newObject, VariableDictionary variableDictionary, GameObject objectToAttachTo) {
        var clonedChains = _logicChains
            .Select(chain => chain.Clone(newObject, variableDictionary, objectToAttachTo))
            .ToArray();
        return new LogicState(clonedChains);
    }
}