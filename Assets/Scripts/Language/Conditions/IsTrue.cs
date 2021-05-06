using UnityEngine;

namespace Language.Conditions {
    public class IsTrue : Condition {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<bool>()}
        };
        
        public IsTrue(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, values, constraintReference) { }
        
        protected override bool ConditionLogic() {
            return Arguments[0] as Value<bool>;
        }
    }
}