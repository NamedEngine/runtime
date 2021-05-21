using System;

namespace Language.Operators {
    public class MinF : Operator<float> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<float>()},
            new IValue[] {new Value<float>()},
        };

        public MinF(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override float InternalGet() {
            return Math.Min((Value<float>) Context.Arguments[0], (Value<float>) Context.Arguments[1]);
        }
    }
}
