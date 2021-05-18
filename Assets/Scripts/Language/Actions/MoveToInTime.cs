using System.Collections;
using Language.Variables;
using UnityEngine;

namespace Language.Actions {
    public class MoveToInTime : Action {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<float>()},
            new IValue[] {new Value<float>()},
            new IValue[] {new Value<float>()},
        };
        
        public MoveToInTime(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, values, constraintReference) { }
        
        protected override IEnumerator ActionLogic() {
            var newX = ((Value<float>) Arguments[0]).Get();
            var newY = ((Value<float>) Arguments[1]).Get();
            var x = (Variable<float>) VariableDict[nameof(CenterX)];
            var y = (Variable<float>) VariableDict[nameof(CenterY)];

            var velocityX = (Variable<float>) VariableDict[nameof(VelocityX)];
            var velocityY = (Variable<float>) VariableDict[nameof(VelocityY)];
            
            var time = ((Value<float>) Arguments[2]).Get();

            var newVelocityX = (newX - x) / time;
            var newVelocityY = (newY - y) / time;
                
            velocityX.Set(newVelocityX);
            velocityY.Set(newVelocityY);

            yield return new WaitForSeconds(time);
                
            x.Set(newX);
            y.Set(newY);
            
            velocityX.Set(0);
            velocityY.Set(0);
        }
    }
}
