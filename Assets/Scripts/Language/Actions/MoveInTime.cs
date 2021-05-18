using System.Collections;
using Language.Variables;
using UnityEngine;

namespace Language.Actions {
    public class MoveInTime : Action {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<float>()},
            new IValue[] {new Value<float>()},
            new IValue[] {new Value<float>()},
            new IValue[] {new Value<bool>(), new NullValue()},  // relative to direction
        };
        
        public MoveInTime(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, DictionaryWrapper<string, IVariable> variables, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, variables, values, constraintReference) { }
        
        protected override IEnumerator ActionLogic() {
            var deltaX = ((Value<float>) Arguments[0]).Get();
            var deltaY = ((Value<float>) Arguments[1]).Get();
            var x = (Variable<float>) VariableDict[nameof(CenterX)];
            var y = (Variable<float>) VariableDict[nameof(CenterY)];

            var velocityX = (Variable<float>) VariableDict[nameof(VelocityX)];
            var velocityY = (Variable<float>) VariableDict[nameof(VelocityY)];
            
            var time = ((Value<float>) Arguments[2]).Get();
            var relative = (Arguments[3] as Value<bool>)?.Get() ?? false;
            
            if (!relative) {
                var initialX = x.Get();
                var initialY = y.Get();
                
                var newVelocityX = deltaX / time;
                var newVelocityY = deltaY / time;
                
                velocityX.Set(newVelocityX);
                velocityY.Set(newVelocityY);

                yield return new WaitForSeconds(time);
                
                x.Set(initialX + deltaX);
                y.Set(initialY + deltaY);
            } else {
                var mapDirection = new Vector2(deltaX, deltaY);
                var unityDirection = EngineAPI.GetSizePosConverter().DirectionM2U(mapDirection);
                var newUnityDirection = BoundGameObject.transform.right * unityDirection.x +
                                        BoundGameObject.transform.forward * unityDirection.y;
                var newMapDirection = EngineAPI.GetSizePosConverter().DirectionU2M(newUnityDirection);

                var resultingX = x + newMapDirection.x;
                var resultingY = y + newMapDirection.y;
                
                var newVelocityX = newMapDirection.x / time;
                var newVelocityY = newMapDirection.y / time;
                
                velocityX.Set(newVelocityX);
                velocityY.Set(newVelocityY);

                yield return new WaitForSeconds(time);
                
                x.Set(resultingX);
                y.Set(resultingY);
            }
            
            velocityX.Set(0);
            velocityY.Set(0);
        }
    }
}
