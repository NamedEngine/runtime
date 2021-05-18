using System;
using UnityEngine;

namespace Language.Operators {
    public class EqualF : Operator<bool> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<float>()},
            new IValue[] {new Value<float>()},
            new IValue[] {new Value<float>(), new NullValue()},
        };

        public EqualF(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, DictionaryWrapper<string, IVariable> variables, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, variables, values, constraintReference) { }

        protected override bool InternalGet() {
            var delta = Arguments[2] as Value<float> ?? 1e-4f;
            return Math.Abs(((Value<float>) Arguments[0]).Get() - ((Value<float>) Arguments[1]).Get()) < delta;
        }
    }
}
