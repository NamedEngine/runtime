using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class LogicOperator<T> : LogicValue<T> {
    protected IValue[] Arguments;
    readonly LogicTypeConstraints _constraints;
    public IValue[][] TypeConstraints => _constraints.ArgTypes;

    protected LogicOperator(IValue[][] argTypes, IValue[] arguments) {
        _constraints = new LogicTypeConstraints(argTypes);
        _constraints.CheckArgs(arguments);
        
        Arguments = arguments;
    }
}