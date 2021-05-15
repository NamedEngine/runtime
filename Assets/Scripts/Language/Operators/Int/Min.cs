﻿using System;
using UnityEngine;

namespace Language.Operators {
    public class Min : Operator<int> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<int>()},
            new IValue[] {new Value<int>()},
        };

        public Min(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, values, constraintReference) { }

        protected override int InternalGet() {
            return Math.Min((Value<int>) Arguments[0], (Value<int>) Arguments[1]);
        }
    }
}