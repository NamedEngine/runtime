using System;

namespace Language.Operators {
    public class AbsF : Operator<float> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<float>()},
        };

        public AbsF(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override float InternalGet() {
            return Math.Abs((Value<float>) Context.Arguments[0]);
        }
    }
}
