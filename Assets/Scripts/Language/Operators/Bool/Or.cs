namespace Language.Operators {
    public class Or : Operator<bool> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<bool>()},
            new IValue[] {new Value<bool>()},
        };

        public Or(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override bool InternalGet() {
            return (Value<bool>) Context.Arguments[0] || (Value<bool>) Context.Arguments[1];
        }
    }
}
