﻿using UnityEngine;

namespace Language.Operators {
    public class Minus : Operator<int> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<int>()},
            new IValue[] {new Value<int>()},
        };

        public Minus(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, values, constraintReference) { }

        protected override int InternalGet() {
            return (Value<int>) Arguments[0] - (Value<int>) Arguments[1];
        }
    }
}