using System.Collections;
using Language.Variables;
using UnityEngine;

namespace Language.Actions {
    public class Move : Action {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<float>()},
            new IValue[] {new Value<float>()},
            new IValue[] {new Value<bool>(), new NullValue()},  // relative to direction
        };
        
        public Move(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, values, constraintReference) { }
        
        protected override IEnumerator ActionLogic() {
            var deltaX = ((Value<float>) Arguments[0]).Get();
            var deltaY = ((Value<float>) Arguments[1]).Get();
            var x = (Variable<float>) VariableDict[nameof(X)];
            var y = (Variable<float>) VariableDict[nameof(Y)];
            var relative = (Arguments[2] as Value<bool>)?.Get() ?? false;
            
            if (!relative) {
                x.Set(x + deltaX);
                y.Set(y + deltaY);
                return null;
            }

            var mapDirection = new Vector2(deltaX, deltaY);
            var unityDirection = EngineAPI.GetSizePosConverter().DirectionM2U(mapDirection);
            var newUnityDirection = BoundGameObject.transform.right * unityDirection.x + BoundGameObject.transform.forward * unityDirection.y;
            var newMapDirection = EngineAPI.GetSizePosConverter().DirectionU2M(newUnityDirection);
            
            x.Set(x + newMapDirection.x);
            y.Set(y + newMapDirection.y);

            return null;
        }
    }
}