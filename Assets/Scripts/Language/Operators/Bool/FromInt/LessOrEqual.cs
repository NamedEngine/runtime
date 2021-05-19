namespace Language.Operators {
    public class LessOrEqual : Operator<bool> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<int>()},
            new IValue[] {new Value<int>()},
        };

        public LessOrEqual(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override bool InternalGet() {
            return (Value<int>) Context.Arguments[0] <= (Value<int>) Context.Arguments[1];
        }
    }
}
