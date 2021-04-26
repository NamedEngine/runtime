public abstract class LogicOperator<T> : Value<T> {
    protected IValue[] Arguments;
    readonly LogicTypeConstraints _constraints;
    public IValue[][] TypeConstraints => _constraints.ArgTypes;

    protected LogicOperator(IValue[][] argTypes, IValue[] arguments) {
        _constraints = new LogicTypeConstraints(argTypes);
        _constraints.CheckArgs(arguments, this);
        
        Arguments = arguments;
    }
}