using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CoroRunner = System.Action<System.Collections.IEnumerator>;

public abstract class Chainable : IConstrainable {
    protected readonly ConstrainableContext Context;
        // most likely this (this bool) and everything related is a REALLY bad solution but I don't have templates nor complex inheritance and want to get this done
    readonly bool _constraintReference;
    readonly LogicTypeConstraints _constraints;
    public IValue[][] GetConstraints() {
        return _constraints.ArgTypes;
    }

    protected Chainable(IValue[][] argTypes, ConstrainableContext context, bool constraintReference) {
        _constraintReference = constraintReference;
        _constraints = new LogicTypeConstraints(argTypes);

        if (_constraintReference) {
            return;
        }
        
        var arguments = _constraints.CheckArgs(context.Arguments, this);
        Context = context.UpdateArguments(arguments);
    }
    
    int _parents;

    void AddParent() {
        _parents++;
    }

    int _notifications;
    bool IsReady => _notifications == _parents;

    void GetNotified() {
        _notifications++;
    }

    public void ResetNotifications() {
        _notifications = 0;
    }
    
    readonly List<Chainable> _children = new List<Chainable>();

    void Notify() {
        // Debug.Log(GetType()+ ": notifying!");
        foreach (var child in _children) {
            if (Context.Base.EngineAPI.LevelChanged) {
                return;
            }
            child.GetNotified();
        }
    }

    public void AddChild(Chainable chainable) {
        if (_constraintReference) {
            throw new ApplicationException("This object is only a constraints reference");
        }
        
        _children.Add(chainable);
        chainable.AddParent();
    }

    public void Execute(CoroRunner runner, Action<IEnumerable<Chainable>> callback) {
        if (_constraintReference) {
            throw new ApplicationException("This object is only a constraints reference");
        }
        
        void ContinueCallChain(bool notify) {
            if (notify) {
                Notify();
            }
            callback(_children.Where(child => child.IsReady));
        }

        var logic = InternalLogic(out var shouldNotify);
        if (logic != null) {
            // Debug.Log(GetType()+ ": my logic is Async!");
            IEnumerator Wrapper() {
                yield return logic;
                
                ContinueCallChain(shouldNotify);
            }

            runner(Wrapper());
        } else {
            // Debug.Log(GetType()+ ": my logic is Sync!");
            ContinueCallChain(shouldNotify);
        }
    }

    protected abstract IEnumerator InternalLogic(out bool shouldNotify);
}
