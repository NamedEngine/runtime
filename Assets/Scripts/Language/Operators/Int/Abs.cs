using System;
using UnityEngine;

namespace Language.Operators {
    public class Abs : Operator<int> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<int>()},
        };

        public Abs(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, DictionaryWrapper<string, IVariable> variables, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, variables, values, constraintReference) { }

        protected override int InternalGet() {
            return Math.Abs((Value<int>) Arguments[0]);
        }
    }
}
