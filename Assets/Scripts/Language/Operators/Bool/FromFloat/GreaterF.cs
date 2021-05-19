namespace Language.Operators {
    public class GreaterF : Operator<bool> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<float>()},
            new IValue[] {new Value<float>()},
        };

        public GreaterF(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override bool InternalGet() {
            return (Value<float>) Context.Arguments[0] > (Value<float>) Context.Arguments[1];
        }
    }
}
