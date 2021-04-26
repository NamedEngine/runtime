public abstract class Operator<T> : Value<T> {
    protected IValue[] Arguments;
    readonly LogicTypeConstraints _constraints;
    public IValue[][] TypeConstraints => _constraints.ArgTypes;

    protected Operator(IValue[][] argTypes, IValue[] arguments) {
        _constraints = new LogicTypeConstraints(argTypes);
        _constraints.CheckArgs(arguments, this);
        
        Arguments = arguments;
    }
}