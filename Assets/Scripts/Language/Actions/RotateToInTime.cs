using System;
using System.Collections;
using Language.Variables;
using UnityEngine;

namespace Language.Actions {
    public class RotateToInTime : Action {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<float>()},
            new IValue[] {new Value<float>()},
        };

        const float DeltaTime = 0.02f;
        
        public RotateToInTime(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }
        
        protected override IEnumerator ActionLogic() {
            var newRotation = ((Value<float>) Context.Arguments[0]).Get();
            var time = ((Value<float>) Context.Arguments[1]).Get();
            var rotation = (Variable<float>) Context.Base.VariableDict[nameof(Rotation)];
            
            var iterations = Convert.ToInt32(time / DeltaTime);
            var deltaRotationPortion = (newRotation - rotation) / iterations;
            for (int i = 0; i < iterations; i++) {
                rotation.Set(rotation + deltaRotationPortion);
                yield return new WaitForSeconds(DeltaTime);
            }
            
            rotation.Set(newRotation);
        }
    }
}
