using System;

namespace Language.Operators {
    public class EqualF : Operator<bool> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<float>()},
            new IValue[] {new Value<float>()},
            new IValue[] {new Value<float>(), new NullValue()},
        };

        public EqualF(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override bool InternalGet() {
            var delta = Context.Arguments[2] as Value<float> ?? 1e-4f;
            return Math.Abs(((Value<float>) Context.Arguments[0]).Get() - ((Value<float>) Context.Arguments[1]).Get()) < delta;
        }
    }
}
