using System;

namespace Language.Operators {
    public class RandFloat : Operator<float> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<float>(), new NullValue()},
            new IValue[] {new Value<float>(), new NullValue()},
        };
        readonly Random _random = new Random();
        
        public RandFloat(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override float InternalGet() {
            var minValue = Context.Arguments[0] as Value<float> ?? float.MinValue;
            var maxValue = Context.Arguments[1] as Value<float> ?? float.MaxValue;
            return minValue + Convert.ToSingle(_random.NextDouble() * Convert.ToDouble(maxValue - minValue));
        }
    }
}
