public class Value<T> : IValue {
    public virtual T Get() {
        var val = InternalGet();
        // Debug.Log("Getting value of type " + GetType() + ": " + val);
        return val;
    }

    protected virtual T InternalGet() {
        return default;
    }

    public virtual bool Cast(IValue value) {
        return value?.PrepareForCast() is Value<T>;
    }

    public virtual IValue PrepareForCast() {
        return this;
    }

    public bool IsType(ValueType type) {
        return typeof(T) == ValueTypeConverter.GetType(type);
    }

    public ValueType GetValueType() {
        return ValueTypeConverter.GetValueType(typeof(T));
    }

    public virtual bool TryTransferValueTo(IVariable other) {
        if (!(other.PrepareForCast() is Variable<T> castedOther)) {
            return false;
        }
        
        castedOther.Set(Get());
        return true;
    }

    public static implicit operator T(Value<T> v) => v.Get();
    
    public override string ToString() {
        return Get().ToString();
    }
}