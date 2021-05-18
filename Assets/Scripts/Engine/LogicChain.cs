using System;
using System.Collections;
using System.Linq;
using UnityEngine;

using OperatorInstantiator = System.Func<LogicObject, LogicEngine.LogicEngineAPI, DictionaryWrapper<string, IVariable>, IValue[], IValue>;
using ChainableInstantiator = System.Func<LogicObject, LogicEngine.LogicEngineAPI, DictionaryWrapper<string, IVariable>, IValue[], Chainable>;

public class LogicChain : MonoBehaviour {
    int _coroCount;
    bool IsRunning => _coroCount > 0;

    Chainable _root;

    LogicChainInfo _info;

    public void ResetChain(LogicObject thisObject, LogicEngine.LogicEngineAPI engineAPI, DictionaryWrapper<string, IVariable> variableDictionary) {
        SetupChain(thisObject, engineAPI, variableDictionary, _info);    
    }
    
    public void SetupChain(LogicObject thisObject, LogicEngine.LogicEngineAPI engineAPI, DictionaryWrapper<string, IVariable> variableDictionary, LogicChainInfo info) { // TODO: thb this stinks
        if (IsRunning) {
            throw new Exception("Should not setup chain when running");
        }
        
        // TODO maybe remove "Need sorted value instantiators" constraint
        var preparedOperators = new IValue[info.OperatorInstantiators.Length];
        for (int i = 0; i < preparedOperators.Length; i++) {
            preparedOperators[i] = info.OperatorInstantiators[i](thisObject, engineAPI, variableDictionary, preparedOperators);
        }

        var preparedChainables = info.ChainableInstantiators
            .Select(chInst => chInst(thisObject, engineAPI, variableDictionary, preparedOperators))
            .ToArray();

        for (int parent = 0; parent < preparedChainables.Length; parent++) {
            var children = info.ChainableRelations
                .Where(pair => pair.Item1 == parent)
                .Select(pair => pair.Item2)
                .ToArray();

            foreach (var child in children) {
                preparedChainables[parent].AddChild(preparedChainables[child]);
            }
        }

        var parentSet = info.ChainableRelations
            .Select(pair => pair.Item1)
            .ToHashSet();
        var childSet = info.ChainableRelations
            .Select(pair => pair.Item2)
            .ToHashSet();
        
        parentSet.ExceptWith(childSet);
        if (parentSet.Count != 1) {
            throw new ArgumentException("A chain should only have one root");
        }

        var rootIndex = parentSet.ToArray()[0];

        _root = preparedChainables[rootIndex];

        _info = info;
    }

    public void Execute() {
        if (IsRunning) {
            return;
        }

        _coroCount++;
        _root.Execute(RunCoro);
        _coroCount--;
    }

    void RunCoro(IEnumerator coro) {
        IEnumerator Runner() {
            _coroCount++;
            yield return coro;
            _coroCount--;
            // Debug.Log("FINISHED RUNNING CORO; COROS LEFT: " + _coroCount);
        }
        // Debug.Log("Running new coro!");
        StartCoroutine(Runner());
    }

    public void Finish() {
        StopAllCoroutines();
        _coroCount = 0;
    }
    
    public LogicChain Clone(LogicObject newObject, LogicEngine.LogicEngineAPI engineAPI, DictionaryWrapper<string, IVariable> variableDictionary, GameObject objectToAttachTo) {
        var newChain = objectToAttachTo.AddComponent<LogicChain>();
        newChain.SetupChain(newObject, engineAPI, variableDictionary, _info);
        return newChain;
    }
}
