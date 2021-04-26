public class Value<T> : IValue {
    public T Get() {
        var val = InternalGet();
        // Debug.Log("Getting value of type " + GetType() + ": " + val);
        return val;
    }

    protected virtual T InternalGet() {
        return default;
    }

    public virtual bool Cast(IValue value) {
        return value is Value<T>;
    }

    public static implicit operator T(Value<T> v) => v.Get();
    
    public override string ToString() {
        return Get().ToString();
    }
}