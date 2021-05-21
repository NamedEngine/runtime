using System;

namespace Language.Operators {
    public class ToInt : Operator<int> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {
                new Value<int>(),
                new Value<float>(),
                new Value<bool>(),
                new Value<string>(),
            },
        };

        public ToInt(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override int InternalGet() {
            switch (Context.Arguments[0]) {
                case Value<int> intVal:
                    return intVal;
                case Value<float> floatVal:
                    return Convert.ToInt32(floatVal);
                case Value<bool> boolVal:
                    return Convert.ToInt32(boolVal);
                case Value<string> strVal:
                    return Convert.ToInt32(strVal);
                default:
                    throw new Exception("This should not be possible!");
            }
        }
    }
}
