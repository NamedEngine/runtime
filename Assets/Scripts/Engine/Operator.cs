using System;
using UnityEngine;

public abstract class Operator<T> : Value<T>, IConstrainable {
    protected readonly GameObject ThisGameObject;
    protected LogicEngine.LogicEngineAPI EngineAPI;
    protected IValue[] Arguments;
    // most likely this (this bool) and everything related is a REALLY bad solution but I don't have templates nor complex inheritance and want to get this done
    bool _constraintReference;
    readonly LogicTypeConstraints _constraints;
    public IValue[][] GetConstraints() {
        return _constraints.ArgTypes;
    }

    protected Operator(IValue[][] argTypes, GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, IValue[] arguments, bool constraintReference) {
        _constraintReference = constraintReference;
        ThisGameObject = gameObject;
        EngineAPI = engineAPI;

        _constraints = new LogicTypeConstraints(argTypes);
        if (!_constraintReference) {
            Arguments = _constraints.CheckArgs(arguments, this);
        }
    }

    public override T Get() {
        if (_constraintReference) {
            throw new ApplicationException("This object is only a constraints reference");
        }

        return InternalGet();
    }
}