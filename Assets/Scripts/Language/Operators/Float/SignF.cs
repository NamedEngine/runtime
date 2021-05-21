using System;

namespace Language.Operators {
    public class SignF : Operator<float> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<float>()},
        };

        public SignF(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override float InternalGet() {
            return Math.Sign((Value<float>) Context.Arguments[0]);
        }
    }
}
