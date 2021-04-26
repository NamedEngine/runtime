using System;
using System.Collections;
using System.Collections.Generic;
using CoroRunner = System.Action<System.Collections.IEnumerator>;

public abstract class Chainable {
    protected readonly IValue[] Arguments;
    readonly LogicTypeConstraints _constraints;
    public IValue[][] TypeConstraints => _constraints.ArgTypes;

    protected Chainable(IValue[][] argTypes, IValue[] arguments) {
        _constraints = new LogicTypeConstraints(argTypes);
        _constraints.CheckArgs(arguments, this);
        
        Arguments = arguments;
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
        _notifiables.Add(chainable.GetNotified);
        chainable.AddParent();
    }

    public void Execute(CoroRunner runner) {
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