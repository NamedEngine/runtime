using System;
using UnityEngine;

namespace Language.Operators {
    public class AbsF : Operator<float> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<float>()},
        };

        public AbsF(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, values, constraintReference) { }

        protected override float InternalGet() {
            return Math.Abs((Value<float>) Arguments[0]);
        }
    }
}