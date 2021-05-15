using System.Collections;
using Language.Variables;
using UnityEngine;

namespace Language.Actions {
    public class RotateTo : Action {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<float>()},
        };
        
        public RotateTo(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, values, constraintReference) { }
        
        protected override IEnumerator ActionLogic() {
            ((Value<float>) Arguments[0]).TryTransferValueTo(VariableDict[nameof(Rotation)]);
            
            return null;
        }
    }
}