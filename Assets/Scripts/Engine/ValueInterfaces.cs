using UnityEngine;

public interface IValue {
    bool Cast(IValue value);
    IValue PrepareForCast();
    
    bool IsType(ValueType type);
    ValueType GetValueType();
}

public interface IVariable : IValue {
    IVariable Clone(GameObject objectToAttachTo);

    bool TryTransferValueTo(IVariable other);
}
