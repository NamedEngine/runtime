using System;

public abstract class Operator<T> : Value<T>, IConstrainable {
    protected readonly ConstrainableContext Context;
    // most likely this (this bool) and everything related is a REALLY bad solution but I don't have templates nor complex inheritance and want to get this done
    protected readonly bool ConstraintReference;
    readonly LogicTypeConstraints _constraints;
    public IValue[][] GetConstraints() {
        return _constraints.ArgTypes;
    }

    protected Operator(IValue[][] argTypes, ConstrainableContext context, bool constraintReference) {
        ConstraintReference = constraintReference;
        _constraints = new LogicTypeConstraints(argTypes);

        if (ConstraintReference) {
            return;
        }
        
        var arguments = _constraints.CheckArgs(context.Arguments, this);
        Context = new ConstrainableContext(context.Base, context.BoundGameObject, arguments);
    }

    public override T Get() {
        if (ConstraintReference) {
            throw new ApplicationException("This object is only a constraints reference");
        }

        return InternalGet();
    }
}
