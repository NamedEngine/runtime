using System;
using System.Collections;
using Language.Variables;
using UnityEngine;

namespace Language.Actions {
    public class RotateInTime : Action {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<float>()},
            new IValue[] {new Value<float>()},
        };

        const float DeltaTime = 0.02f;
        
        public RotateInTime(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, values, constraintReference) { }
        
        protected override IEnumerator ActionLogic() {
            var deltaRotation = ((Value<float>) Arguments[0]).Get();
            var time = ((Value<float>) Arguments[1]).Get();
            var rotation = (Variable<float>) VariableDict[nameof(Rotation)];
            
            var resultingRotation = rotation + deltaRotation;

            var iterations = Convert.ToInt32(time / DeltaTime);
            var deltaRotationPortion = deltaRotation / iterations;
            for (int i = 0; i < iterations; i++) {
                rotation.Set(rotation + deltaRotationPortion);
                yield return new WaitForSeconds(DeltaTime);
            }
            
            rotation.Set(resultingRotation);
        }
    }
}