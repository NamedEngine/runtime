using System;
using UnityEngine;

namespace Language.Operators {
    public class ToFloat : Operator<float> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {
                new Value<int>(),
                new Value<float>(),
                new Value<bool>(),
                new Value<string>(),
            },
        };

        public ToFloat(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, DictionaryWrapper<string, IVariable> variables, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, variables, values, constraintReference) { }

        protected override float InternalGet() {
            switch (Arguments[0]) {
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
