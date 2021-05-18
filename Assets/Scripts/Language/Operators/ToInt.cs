using System;
using UnityEngine;

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

        public ToInt(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, DictionaryWrapper<string, IVariable> variables, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, variables, values, constraintReference) { }

        protected override int InternalGet() {
            switch (Arguments[0]) {
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
