using System;

namespace Language.Operators {
    public class Min : Operator<int> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<int>()},
            new IValue[] {new Value<int>()},
        };

        public Min(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override int InternalGet() {
            return Math.Min((Value<int>) Context.Arguments[0], (Value<int>) Context.Arguments[1]);
        }
    }
}
