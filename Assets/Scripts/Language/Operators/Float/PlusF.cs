﻿using UnityEngine;

namespace Language.Operators {
    public class PlusF : Operator<float> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<float>()},
            new IValue[] {new Value<float>()},
        };

        public PlusF(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, values, constraintReference) { }

        protected override float InternalGet() {
            return (Value<float>) Arguments[0] + (Value<float>) Arguments[1];
        }
    }
}