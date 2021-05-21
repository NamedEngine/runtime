using System.Collections;

namespace Language.Actions {
    public class SetSprite : Action {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<string>()},
        };

        SpriteAnimator _animator;
        
        void SetAnimator() {
            _animator = Context.BoundGameObject.GetComponent<SpriteAnimator>();
        }

        readonly System.Action _setAnimatorOnce;

        public SetSprite(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context,
            constraintReference) {
            _setAnimatorOnce = ((System.Action) SetAnimator).Once();
        }

        protected override IEnumerator ActionLogic() {
            _setAnimatorOnce();
            
            _animator.SetSprite((Value<string>) Context.Arguments[0]);

            return null;
        }
    }
}
