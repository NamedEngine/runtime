using System;

namespace Language.Operators {
    public class ToBool : Operator<bool> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {
                new Value<bool>(),
                new Value<int>(),
                new Value<float>(),
                new Value<string>(),
            },
        };

        public ToBool(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override bool InternalGet() {
            switch (Context.Arguments[0]) {
                case Value<int> intVal:
                    return Convert.ToBoolean(intVal);
                case Value<float> floatVal:
                    return Convert.ToBoolean(floatVal);;
                case Value<bool> boolVal:
                    return boolVal;
                case Value<string> strVal:
                    return Convert.ToBoolean(strVal);
                default:
                    throw new Exception("This should not be possible!");
            }
        }
    }
}
