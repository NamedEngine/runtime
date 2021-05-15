using UnityEngine;

public interface IValue {
    bool Cast(IValue value);
    IValue PrepareForCast();
    
    bool IsType(ValueType type);
    ValueType GetValueType();

    bool TryTransferValueTo(IVariable other);
}

public interface IVariable : IValue {
    IVariable Clone(GameObject objectToAttachTo, LogicEngine.LogicEngineAPI engineAPI);
}
