
public class SharedVal<T> {
    public T Value;
    
    public SharedVal(T value) {
        Value = value;
    }
    
    public static implicit operator SharedVal<T>(T value) => new SharedVal<T>(value);
}