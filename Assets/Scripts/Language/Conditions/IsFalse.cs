using UnityEngine;

namespace Language.Conditions {
    public class IsFalse : Condition {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<bool>()}
        };
        
        public IsFalse(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, DictionaryWrapper<string, IVariable> variables, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, variables, values, constraintReference) { }
        
        protected override bool ConditionLogic() {
            return !(Arguments[0] as Value<bool>);
        }
    }
}
