﻿using UnityEngine;

namespace Language.Operators {
    public class And : Operator<bool> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<bool>()},
            new IValue[] {new Value<bool>()},
        };

        public And(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, DictionaryWrapper<string, IVariable> variables, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, variables, values, constraintReference) { }

        protected override bool InternalGet() {
            return (Value<bool>) Arguments[0] && (Value<bool>) Arguments[1];
        }
    }
}
