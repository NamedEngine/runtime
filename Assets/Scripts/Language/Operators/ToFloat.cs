using System;

namespace Language.Operators {
    public class ToFloat : Operator<float> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {
                new Value<float>(),
                new Value<int>(),
                new Value<bool>(),
                new Value<string>(),
            },
        };

        public ToFloat(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override float InternalGet() {
            switch (Context.Arguments[0]) {
                case Value<int> intVal:
                    return Convert.ToSingle(intVal);
                case Value<float> floatVal:
                    return floatVal;
                case Value<bool> boolVal:
                    return Convert.ToSingle(boolVal);
                case Value<string> strVal:
                    return Convert.ToSingle(strVal);
                default:
                    throw new Exception("This should not be possible!");
            }
        }
    }
}
