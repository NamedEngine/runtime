using System;
using System.Collections;
using UnityEngine;

namespace Language.Actions {
    public class DestroyObject : Action {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<string>()},
            new IValue[] {new ClassRef()},
        };

        public DestroyObject(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, DictionaryWrapper<string, IVariable> variables, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, variables, values, constraintReference) { }
        
        protected override IEnumerator ActionLogic() {
            var objName = ((Value<string>) Arguments[0]).Get();
            var className = ((ClassRef) Arguments[1]).ClassName;
            var destroyed = EngineAPI.DestroyObject(objName, className);
            if (!destroyed) {
                throw new LogicException(nameof(DestroyObject), 
                    $"Could not find object with name \"{objName}\" and type \"{className}\"");
            }

            return null;
        }
    }
}
