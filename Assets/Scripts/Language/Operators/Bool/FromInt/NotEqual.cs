using UnityEngine;

namespace Language.Operators {
    public class NotEqual : Operator<bool> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<int>()},
            new IValue[] {new Value<int>()},
        };

        public NotEqual(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, values, constraintReference) { }

        protected override bool InternalGet() {
            return ((Value<int>) Arguments[0]).Get() != ((Value<int>) Arguments[1]).Get();
        }
    }
}