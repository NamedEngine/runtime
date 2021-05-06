using System;
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

    public void Start(LogicObject thisObject, LogicEngine.LogicEngineAPI engineAPI, VariableDictionary variableDictionary) {
        foreach (var logicChain in _logicChains) {
            logicChain.ResetChain(thisObject, engineAPI, variableDictionary);
        }
    }

    public void Finish() {
        foreach (var logicChain in _logicChains) {
            logicChain.Finish();
        }
    }
    
    public void Destroy(Action<UnityEngine.Object> destroyer) {
        foreach (var logicChain in _logicChains) {
            destroyer(logicChain);
        }
    }

    public LogicState Clone(LogicObject newObject, LogicEngine.LogicEngineAPI engineAPI, VariableDictionary variableDictionary, GameObject objectToAttachTo) {
        var clonedChains = _logicChains
            .Select(chain => chain.Clone(newObject, engineAPI, variableDictionary, objectToAttachTo))
            .ToArray();
        return new LogicState(clonedChains);
    }
}