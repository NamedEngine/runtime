namespace Language.Operators {
    public class Equal : Operator<bool> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<int>()},
            new IValue[] {new Value<int>()},
        };

        public Equal(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override bool InternalGet() {
            return ((Value<int>) Context.Arguments[0]).Get() == ((Value<int>) Context.Arguments[1]).Get();
        }
    }
}
