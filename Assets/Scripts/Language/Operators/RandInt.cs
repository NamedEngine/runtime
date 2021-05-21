using System;

namespace Language.Operators {
    public class RandInt : Operator<int> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<int>(), new NullValue()},
            new IValue[] {new Value<int>(), new NullValue()},
        };
        readonly Random _random = new Random();
        
        public RandInt(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override int InternalGet() {
            var minValue = Context.Arguments[0] as Value<int> ?? int.MinValue;
            var maxValue = Context.Arguments[1] as Value<int> ?? int.MaxValue;
            return _random.Next(minValue, maxValue);
        }
    }
}
