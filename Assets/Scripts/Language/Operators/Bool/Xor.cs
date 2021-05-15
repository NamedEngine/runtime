using UnityEngine;

namespace Language.Operators {
    public class Xor : Operator<bool> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<bool>()},
            new IValue[] {new Value<bool>()},
        };

        public Xor(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, values, constraintReference) { }

        protected override bool InternalGet() {
            var arg1 = (Value<bool>) Arguments[0];
            var arg2 = (Value<bool>) Arguments[1];
            return !arg1 && arg2 || arg1 && !arg2;
        }
    }
}