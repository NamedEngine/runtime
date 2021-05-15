using System.Collections;
using UnityEngine;

namespace Language.Actions {
    public class CreateObject : Action {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<string>()},
            new IValue[] {new ClassRef()},
        };

        public CreateObject(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, values, constraintReference) { }
        
        protected override IEnumerator ActionLogic() {
            EngineAPI.CreateObject((Value<string>) Arguments[0], ((ClassRef) Arguments[1]).ClassName);

            return null;
        }
    }
}