public class VariableRef : IValue {
    public readonly ClassRef ClassRef;
    public readonly string Name;
    public readonly ValueType Type;

    public VariableRef() { }  // for type constraints
    public VariableRef(ClassRef classRef, string name, ValueType type) {
        ClassRef = classRef;
        Name = name;
        Type = type;
    }
    
    public bool Cast(IValue value) {
        return value is VariableRef;
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
}