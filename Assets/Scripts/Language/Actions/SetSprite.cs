using System.Collections;
using UnityEngine;

namespace Language.Actions {
    public class SetSprite : Action {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<string>()},
        };

        readonly SpriteAnimator _animation;

        public SetSprite(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, DictionaryWrapper<string, IVariable> variables, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, variables, values, constraintReference) {
            _animation = gameObject?.GetComponent<SpriteAnimator>();
        }
        
        protected override IEnumerator ActionLogic() {
            _animation.SetSprite((Value<string>) Arguments[0]);

            return null;
        }
    }
}
