public class LogicVariable<T> : LogicValue<T>, IVariable {
    T _value;

    public LogicVariable(T value = default) {
        _value = value;
    }

    protected override T InternalGet() {
        return _value;
    }

    public void Set(T value) {
        _value = value;
    }

    public override bool Cast(IValue value) {
        return value is LogicVariable<T>;
    }

    public IVariable Clone() {
        return new LogicVariable<T>(_value);
    }
}