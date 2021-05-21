namespace Language.Operators {
    public class Xor : Operator<bool> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<bool>()},
            new IValue[] {new Value<bool>()},
        };

        public Xor(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override bool InternalGet() {
            var arg1 = (Value<bool>) Context.Arguments[0];
            var arg2 = (Value<bool>) Context.Arguments[1];
            return !arg1 && arg2 || arg1 && !arg2;
        }
    }
}
