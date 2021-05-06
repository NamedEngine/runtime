using System.Collections;
using UnityEngine;

namespace Language.Actions {
    public class SetAnimation : Action {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<string>()},
            new IValue[] {new Value<float>()},
            new IValue[] {new Value<int>()},
        };

        readonly SpriteAnimator _animation;

        public SetAnimation(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, values, constraintReference) {
            _animation = gameObject?.GetComponent<SpriteAnimator>();
        }
        
        protected override IEnumerator ActionLogic() {
            _animation.SetAnimation(
                (Value<string>) Arguments[0],
                (Value<float>) Arguments[1],
                (Value<int>) Arguments[2]
            );

            return null;
        }
    }
}