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
        
        public Move(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }
        
        protected override IEnumerator ActionLogic() {
            var deltaX = ((Value<float>) Context.Arguments[0]).Get();
            var deltaY = ((Value<float>) Context.Arguments[1]).Get();
            var x = (Variable<float>) Context.Base.VariableDict[nameof(CenterX)];
            var y = (Variable<float>) Context.Base.VariableDict[nameof(CenterY)];
            var relative = (Context.Arguments[2] as Value<bool>)?.Get() ?? false;
            
            if (!relative) {
                x.Set(x + deltaX);
                y.Set(y + deltaY);
                return null;
            }

            var mapDirection = new Vector2(deltaX, deltaY);
            var unityDirection = Context.Base.EngineAPI.GetSizePosConverter().DirectionM2U(mapDirection);
            var newUnityDirection = Context.BoundGameObject.transform.right * unityDirection.x + Context.BoundGameObject.transform.forward * unityDirection.y;
            var newMapDirection = Context.Base.EngineAPI.GetSizePosConverter().DirectionU2M(newUnityDirection);
            
            x.Set(x + newMapDirection.x);
            y.Set(y + newMapDirection.y);

            return null;
        }
    }
}
