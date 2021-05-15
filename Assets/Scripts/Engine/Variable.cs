using UnityEngine;

public class Variable<T> : Value<T>, IVariable {
    T _value;

    public Variable(T value = default) {
        _value = value;
    }

    protected override T InternalGet() {
        return _value;
    }

    public virtual void Set(T value) {
        _value = value;
    }

    public override bool Cast(IValue value) {
        return value?.PrepareForCast() is Variable<T>;
    }

    public virtual IVariable Clone(GameObject objectToAttachTo, LogicEngine.LogicEngineAPI engineAPI) {
        return new Variable<T>(_value);
    }
}