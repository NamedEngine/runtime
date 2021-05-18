using System;
using UnityEngine;

namespace Language.Operators {
    public class MaxF : Operator<float> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<float>()},
            new IValue[] {new Value<float>()},
        };

        public MaxF(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, DictionaryWrapper<string, IVariable> variables, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, variables, values, constraintReference) { }

        protected override float InternalGet() {
            return Math.Max((Value<float>) Arguments[0], (Value<float>) Arguments[1]);
        }
    }
}
