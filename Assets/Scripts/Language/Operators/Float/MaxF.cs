using System;

namespace Language.Operators {
    public class MaxF : Operator<float> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<float>()},
            new IValue[] {new Value<float>()},
        };

        public MaxF(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override float InternalGet() {
            return Math.Max((Value<float>) Context.Arguments[0], (Value<float>) Context.Arguments[1]);
        }
    }
}
