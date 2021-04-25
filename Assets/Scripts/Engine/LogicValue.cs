using UnityEngine;

public abstract class LogicValue<T> : IValue {
    public T Get() {
        var val = InternalGet();
        // Debug.Log("Getting value of type " + GetType() + ": " + val);
        return val;
    }
    abstract protected T InternalGet();

    public virtual bool Cast(IValue value) {
        return value is LogicValue<T>;
    }

    public static implicit operator T(LogicValue<T> v) => v.Get();
    
    public override string ToString() {
        return Get().ToString();
    }
}