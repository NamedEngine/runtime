using System;
using System.Collections;
using UnityEngine;

namespace Language.Actions {
    public class Wait : Action {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {
                new Value<int>(),
                new Value<float>()
            },
        };
        
        public Wait(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, DictionaryWrapper<string, IVariable> variables, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, variables, values, constraintReference) { }
        protected override IEnumerator ActionLogic() {
            switch (Arguments[0]) {
                case Value<int> intVal:
                    yield return new WaitForSeconds(intVal);
                    break;
                case Value<float> floatVal:
                    yield return new WaitForSeconds(floatVal);
                    break;
                default:
                    throw new Exception("This should not be possible!");
            }
        }
    }
}
