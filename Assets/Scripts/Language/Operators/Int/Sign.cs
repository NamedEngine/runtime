using System;

namespace Language.Operators {
    public class Sign : Operator<int> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<int>()},
        };

        public Sign(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override int InternalGet() {
            return Math.Sign((Value<int>) Context.Arguments[0]);
        }
    }
}
