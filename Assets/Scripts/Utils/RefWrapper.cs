public class RefWrapper<T> {
    public T Value { get; set; }

    public RefWrapper(T value = default) {
        Value = value;
    }

    public static implicit operator T(RefWrapper<T> rw) => rw.Value;
}
