using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoroRunner = System.Action<System.Collections.IEnumerator>;

public abstract class Chainable : IConstrainable {
    protected readonly GameObject BoundGameObject;
    protected readonly DictionaryWrapper<string, IVariable> VariableDict;
    protected readonly LogicEngine.LogicEngineAPI EngineAPI;
    protected readonly IValue[] Arguments;
    // most likely this (this bool) and everything related is a REALLY bad solution but I don't have templates nor complex inheritance and want to get this done
    readonly bool _constraintReference;
    readonly LogicTypeConstraints _constraints;
    public IValue[][] GetConstraints() {
        return _constraints.ArgTypes;
    }

    protected Chainable(IValue[][] argTypes, GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI,
        DictionaryWrapper<string, IVariable> variables, IValue[] arguments, bool constraintReference) {
        _constraintReference = constraintReference;
        BoundGameObject = gameObject;
        VariableDict = variables;
        EngineAPI = engineAPI;

        _constraints = new LogicTypeConstraints(argTypes);
        if (!_constraintReference) {
            Arguments = _constraints.CheckArgs(arguments, this);
        }
    }
    
    int _parents;

    void AddParent() {
        _parents++;
    }

    int _notifications;
    void GetNotified(CoroRunner runner) {
        _notifications++;
        // Debug.Log(GetType()+ ": getting notified!");
        if (_notifications >= _parents) {
            _notifications = 0;
            // Debug.Log(GetType()+ ": executing!");
            Execute(runner);
        }
    }
    
    readonly List<Action<CoroRunner>> _notifiables = new List<Action<CoroRunner>>();

    void Notify(CoroRunner runner) {
        // Debug.Log(GetType()+ ": notifying!");
        foreach (var notifiable in _notifiables) {
            notifiable(runner);
        }
    }
    public void AddChild(Chainable chainable) {
        if (_constraintReference) {
            throw new ApplicationException("This object is only a constraints reference");
        }
        
        _notifiables.Add(chainable.GetNotified);
        chainable.AddParent();
    }

    public void Execute(CoroRunner runner) {
        if (_constraintReference) {
            throw new ApplicationException("This object is only a constraints reference");
        }
        
        var logic = InternalLogic(out var shouldNotify);
        if (logic != null) {
            // Debug.Log(GetType()+ ": my logic is Async!");
            IEnumerator Wrapper() {
                yield return logic;
                
                if (shouldNotify) {
                    Notify(runner);
                }
            }

            runner(Wrapper());
        } else {
            // Debug.Log(GetType()+ ": my logic is Sync!");
            if (shouldNotify) {
                Notify(runner);
            }
        }
    }

    protected abstract IEnumerator InternalLogic(out bool shouldNotify);
}
