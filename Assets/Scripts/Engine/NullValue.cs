namespace Engine {
    public class NullValue : IValue {
        public bool Cast(IValue value) {
            return value == null;
        }
    }
}