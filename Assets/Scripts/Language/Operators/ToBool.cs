using System;
using UnityEngine;

namespace Language.Operators {
    public class ToBool : Operator<bool> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {
                new Value<int>(),
                new Value<float>(),
                new Value<bool>(),
                new Value<string>(),
            },
        };

        public ToBool(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, values, constraintReference) { }

        protected override bool InternalGet() {
            switch (Arguments[0]) {
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