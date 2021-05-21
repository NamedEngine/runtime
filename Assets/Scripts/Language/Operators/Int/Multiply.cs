namespace Language.Operators {
    public class Multiply : Operator<int> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<int>()},
            new IValue[] {new Value<int>()},
        };

        public Multiply(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override int InternalGet() {
            return (Value<int>) Context.Arguments[0] * (Value<int>) Context.Arguments[1];
        }
    }
}
