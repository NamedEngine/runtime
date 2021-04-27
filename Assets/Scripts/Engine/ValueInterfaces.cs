public interface IValue {
    bool Cast(IValue value);

    bool IsType(ValueType type);
}

public interface IVariable : IValue {
    IVariable Clone();
}
