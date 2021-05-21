namespace Language.Operators {
    public class Less : Operator<bool> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<int>()},
            new IValue[] {new Value<int>()},
        };

        public Less(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override bool InternalGet() {
            return (Value<int>) Context.Arguments[0] < (Value<int>) Context.Arguments[1];
        }
    }
}
