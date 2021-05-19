using System;

namespace Language.Operators {
    public class ToString : Operator<string> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {
                new Value<int>(),
                new Value<float>(),
                new Value<bool>(),
                new Value<string>(),
            },
        };

        public ToString(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override string InternalGet() {
            switch (Context.Arguments[0]) {
                case Value<int> intVal:
                    return intVal.ToString();
                case Value<float> floatVal:
                    return floatVal.ToString();
                case Value<bool> boolVal:
                    return boolVal.ToString();
                case Value<string> strVal:
                    return strVal;
                default:
                    throw new Exception("This should not be possible!");
            }
        }
    }
}
