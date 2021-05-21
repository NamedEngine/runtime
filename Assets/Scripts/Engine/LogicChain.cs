using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using OperatorInstantiator = System.Func<LogicObject, LogicEngine.LogicEngineAPI, DictionaryWrapper<string, IVariable>, IValue[], IValue>;
using ChainableInstantiator = System.Func<LogicObject, LogicEngine.LogicEngineAPI, DictionaryWrapper<string, IVariable>, IValue[], Chainable>;

public class LogicChain : MonoBehaviour {
    int _coroCount;
    bool IsRunning => _coroCount > 0;

    Chainable _root;

    LogicChainInfo _info;

    public void ResetChain(LogicObject thisObject, BaseContext baseContex) {
        SetupChain(thisObject, baseContex, _info);    
    }

    readonly List<System.Action> _chainableNotificationResetters = new List<System.Action>();
    
    public void SetupChain(LogicObject thisObject, BaseContext baseContex, LogicChainInfo info) {
        if (IsRunning) {
            throw new Exception("Should not setup chain when running");
        }
        
        // TODO maybe remove "Need sorted value instantiators" constraint
        var argLocContext =
            new ArgumentLocationContext(baseContex, thisObject, new IValue[info.OperatorInstantiators.Length]);
        
        for (var i = 0; i < argLocContext.PreparedOperators.Length; i++) {
            argLocContext.PreparedOperators[i] = info.OperatorInstantiators[i](argLocContext);
        }

        var preparedChainables = info.ChainableInstantiators
            .Select(chInst => chInst(argLocContext))
            .ToList();
        
        _chainableNotificationResetters.Clear();
        preparedChainables.ForEach(chainable => _chainableNotificationResetters.Add(chainable.ResetNotifications));

        for (var parent = 0; parent < preparedChainables.Count; parent++) {
            var children = info.ChainableRelations
                .Where(pair => pair.Item1 == parent)
                .Select(pair => pair.Item2)
                .ToArray();

            foreach (var child in children) {
                // Debug.Log($"{preparedChainables[parent].GetType()} now has child {preparedChainables[child]}");
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
            throw new ArgumentException("A chain should only have one root, but it has several: " +
                                        string.Join(", ", parentSet));
        }

        var rootIndex = parentSet.ToArray()[0];

        _root = preparedChainables[rootIndex];

        _info = info;
    }
    public void Execute() {
        if (IsRunning) {
            return;
        }

        _chainableNotificationResetters.ForEach(reset => reset());

        _coroCount++;
        _root.Execute(RunCoro, ExecutionCallback);
        _coroCount--;
    }

    readonly Queue<Chainable> _executionQueue = new Queue<Chainable>();

    void ExecutionCallback(IEnumerable<Chainable> nextChainables) {
        foreach (var next in nextChainables) {
            _executionQueue.Enqueue(next);
        }
        
        if (_executionQueue.Count > 0) {
            _executionQueue.Dequeue().Execute(RunCoro, ExecutionCallback);
        }
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
    
    public LogicChain Clone(LogicObject newObject, BaseContext baseContext, GameObject objectToAttachTo) {
        var newChain = objectToAttachTo.AddComponent<LogicChain>();
        newChain.SetupChain(newObject, baseContext, _info);
        return newChain;
    }
}
