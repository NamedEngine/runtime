public class ClassRef : IValue {
    public readonly string ClassName;

    public ClassRef() { }  // for type constraints
    public ClassRef(string className) {
        ClassName = className;
    }
    public bool Cast(IValue value) {
        return value is ClassRef;
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