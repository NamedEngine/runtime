using System;

namespace Language.Operators {
    public class Power : Operator<int> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<int>()},
            new IValue[] {new Value<int>()},
        };

        public Power(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override int InternalGet() {
            return Convert.ToInt32(Math.Pow((Value<int>) Context.Arguments[0], (Value<int>) Context.Arguments[1]));
        }
    }
}
