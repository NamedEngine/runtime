using System;
using UnityEngine;

public abstract class SpecialVariable<T> : Variable<T>, ISpecialVariable {
    protected GameObject BoundGameObject;
    protected LogicEngine.LogicEngineAPI EngineAPI;
    T _tempValue;
    
    public SpecialVariable(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI) {
        BoundGameObject = gameObject;
        EngineAPI = engineAPI;
    }

    // if false -> is used in class, not object
    bool IsSet() {
        return EngineAPI != null && BoundGameObject != null;
    }

    protected override T InternalGet() {
        return IsSet() ? SpecialGet() : _tempValue;
    }

    public override void Set(T value) {
        if (IsSet()) {
            SpecialSet(value);
        } else {
            _tempValue = value;
        }
    }

    protected abstract T SpecialGet();
    protected abstract void SpecialSet(T value);

    public override IVariable Clone(GameObject objectToAttachTo, LogicEngine.LogicEngineAPI engineAPI) {
        var newVariableInstantiator = LogicUtils.GetSpecialVariableInstantiator(GetType());
        var newVariable = newVariableInstantiator(objectToAttachTo, engineAPI) as SpecialVariable<T>;
        
        var res = TryTransferValueTo(newVariable);
        if (!res) {
            throw new ApplicationException();
        }
        return newVariable;
    }
}