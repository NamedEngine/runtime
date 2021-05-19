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
        
        public Wait(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }
        protected override IEnumerator ActionLogic() {
            switch (Context.Arguments[0]) {
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
