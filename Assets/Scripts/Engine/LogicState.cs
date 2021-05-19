using System;
using System.Linq;
using UnityEngine;

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

    public void Start(LogicObject thisObject, BaseContext baseContext) {
        foreach (var logicChain in _logicChains) {
            logicChain.ResetChain(thisObject, baseContext);
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

    public LogicState Clone(LogicObject newObject, BaseContext baseContext, GameObject objectToAttachTo) {
        var clonedChains = _logicChains
            .Select(chain => chain.Clone(newObject, baseContext, objectToAttachTo))
            .ToArray();
        return new LogicState(clonedChains);
    }
}
