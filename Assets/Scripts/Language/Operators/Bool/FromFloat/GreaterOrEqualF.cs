namespace Language.Operators {
    public class GreaterOrEqualF : Operator<bool> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<float>()},
            new IValue[] {new Value<float>()},
        };

        public GreaterOrEqualF(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override bool InternalGet() {
            return (Value<float>) Context.Arguments[0] >= (Value<float>) Context.Arguments[1];
        }
    }
}
