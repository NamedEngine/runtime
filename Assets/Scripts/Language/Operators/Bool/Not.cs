namespace Language.Operators {
    public class Not : Operator<bool> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<bool>()},
        };

        public Not(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override bool InternalGet() {
            return !(Value<bool>) Context.Arguments[0];
        }
    }
}
