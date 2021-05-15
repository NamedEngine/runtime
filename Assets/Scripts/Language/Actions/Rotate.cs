using System.Collections;
using Language.Variables;
using UnityEngine;

namespace Language.Actions {
    public class Rotate : Action {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<float>()},
        };
        
        public Rotate(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, values, constraintReference) { }
        
        protected override IEnumerator ActionLogic() {
            var deltaRotation = ((Value<float>) Arguments[0]).Get();
            var rotation = (Variable<float>) VariableDict[nameof(Rotation)];
            
            rotation.Set(rotation + deltaRotation);

            return null;
        }
    }
}