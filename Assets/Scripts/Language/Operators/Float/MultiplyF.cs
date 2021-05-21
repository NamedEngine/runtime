namespace Language.Operators {
    public class MultiplyF : Operator<float> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<float>()},
            new IValue[] {new Value<float>()},
        };

        public MultiplyF(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override float InternalGet() {
            return (Value<float>) Context.Arguments[0] * (Value<float>) Context.Arguments[1];
        }
    }
}
