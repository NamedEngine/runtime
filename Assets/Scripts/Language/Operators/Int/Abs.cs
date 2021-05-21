using System;

namespace Language.Operators {
    public class Abs : Operator<int> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<int>()},
        };

        public Abs(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override int InternalGet() {
            return Math.Abs((Value<int>) Context.Arguments[0]);
        }
    }
}
