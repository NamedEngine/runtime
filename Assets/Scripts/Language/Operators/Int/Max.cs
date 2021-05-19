using System;

namespace Language.Operators {
    public class Max : Operator<int> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<int>()},
            new IValue[] {new Value<int>()},
        };

        public Max(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override int InternalGet() {
            return Math.Max((Value<int>) Context.Arguments[0], (Value<int>) Context.Arguments[1]);
        }
    }
}
