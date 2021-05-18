using System;
using UnityEngine;

namespace Language.Operators {
    public class Power : Operator<int> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<int>()},
            new IValue[] {new Value<int>()},
        };

        public Power(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, DictionaryWrapper<string, IVariable> variables, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, variables, values, constraintReference) { }

        protected override int InternalGet() {
            return Convert.ToInt32(Math.Pow((Value<int>) Arguments[0], (Value<int>) Arguments[1]));
        }
    }
}
