public interface IValue {
    bool Cast(IValue value);
}

public interface IVariable : IValue, IClonable<IVariable> { }
