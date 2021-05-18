using System;
using UnityEngine;

public abstract class Operator<T> : Value<T>, IConstrainable {
    protected readonly GameObject BoundGameObject;
    protected readonly DictionaryWrapper<string, IVariable> VariableDict;
    protected readonly LogicEngine.LogicEngineAPI EngineAPI;
    protected readonly IValue[] Arguments;
    // most likely this (this bool) and everything related is a REALLY bad solution but I don't have templates nor complex inheritance and want to get this done
    protected readonly bool ConstraintReference;
    readonly LogicTypeConstraints _constraints;
    public IValue[][] GetConstraints() {
        return _constraints.ArgTypes;
    }

    protected Operator(IValue[][] argTypes, GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI,
        DictionaryWrapper<string, IVariable> variables, IValue[] arguments, bool constraintReference) {
        ConstraintReference = constraintReference;
        BoundGameObject = gameObject;
        VariableDict = variables;
        EngineAPI = engineAPI;

        _constraints = new LogicTypeConstraints(argTypes);
        if (!ConstraintReference) {
            Arguments = _constraints.CheckArgs(arguments, this);
        }
    }

    public override T Get() {
        if (ConstraintReference) {
            throw new ApplicationException("This object is only a constraints reference");
        }

        return InternalGet();
    }
}
