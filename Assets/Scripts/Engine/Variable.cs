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
        return value is Variable<T>;
    }

    public virtual IVariable Clone(GameObject objectToAttachTo) {
        return new Variable<T>(_value);
    }

    public bool TryTransferValueTo(IVariable other) {
        if (!(other is Variable<T> castedOther)) {
            return false;
        }
        
        castedOther.Set(Get());
        return true;
    }
}