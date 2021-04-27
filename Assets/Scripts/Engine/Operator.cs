using System;

public abstract class Operator<T> : Value<T>, IConstrainable {
    protected IValue[] Arguments;
    // most likely this (this bool) and everything related is a REALLY bad solution but I don't have templates nor complex inheritance and want to get this done
    bool _constraintReference;
    readonly LogicTypeConstraints _constraints;
    public IValue[][] GetConstraints() {
        return _constraints.ArgTypes;
    }

    protected Operator(IValue[][] argTypes, IValue[] arguments, bool constraintReference) {
        _constraintReference = constraintReference;
        
        _constraints = new LogicTypeConstraints(argTypes);
        if (!_constraintReference) {
            _constraints.CheckArgs(arguments, this);
        }
        
        Arguments = arguments;
    }

    protected override T InternalGet() {
        if (_constraintReference) {
            throw new ApplicationException("This object is only a constraints reference");
        }
        return base.InternalGet();
    }
}