using UnityEngine;

namespace Language.Operators {
    public class GreaterOrEqual : Operator<bool> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<int>()},
            new IValue[] {new Value<int>()},
        };

        public GreaterOrEqual(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, DictionaryWrapper<string, IVariable> variables, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, variables, values, constraintReference) { }

        protected override bool InternalGet() {
            return (Value<int>) Arguments[0] >= (Value<int>) Arguments[1];
        }
    }
}
