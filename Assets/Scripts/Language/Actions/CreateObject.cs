using System;
using System.Collections;
using UnityEngine;

namespace Language.Actions {
    public class CreateObject : Action {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<string>()},
            new IValue[] {new ClassRef()},
        };

        public CreateObject(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, DictionaryWrapper<string, IVariable> variables, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, variables, values, constraintReference) { }
        
        protected override IEnumerator ActionLogic() {
            var objName = ((Value<string>) Arguments[0]).Get();
            var res = EngineAPI.CreateObject(objName, ((ClassRef) Arguments[1]).ClassName);
            if (res == null) {
                throw new ArgumentException($"Object with name \"{objName}\" already exists!");
            }

            return null;
        }
    }
}
