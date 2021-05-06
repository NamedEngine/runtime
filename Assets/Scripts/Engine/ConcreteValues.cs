public class ConcreteValue<T> : Value<T> {
    readonly T _value;
    
    public ConcreteValue(T value = default) {
        _value = value;
    }
    protected override T InternalGet() {
        return _value;
    }
}

public class NullValue : IValue {
    public bool Cast(IValue value) {
        return value == null;
    }

    public IValue PrepareForCast() {
        return this;
    }

    public bool IsType(ValueType type) {
        return false;
    }

    public ValueType GetValueType() {
        return ValueType.Null;
    }

    public bool TryTransferValueTo(IVariable other) {
        return false;
    }
}