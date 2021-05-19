using System.Collections;

namespace Language.Actions {
    public class SetAnimation : Action {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<string>()},
            new IValue[] {new Value<float>()},
            new IValue[] {new Value<int>()},
        };

        SpriteAnimator _animator;
        
        void SetAnimator() {
            _animator = Context.BoundGameObject.GetComponent<SpriteAnimator>();
        }

        readonly System.Action _setAnimatorOnce;

        public SetAnimation(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context,
            constraintReference) {
            _setAnimatorOnce = ((System.Action) SetAnimator).Once();
        }

        protected override IEnumerator ActionLogic() {
            _setAnimatorOnce();
            
            _animator.SetAnimation(
                (Value<string>) Context.Arguments[0],
                (Value<float>) Context.Arguments[1],
                (Value<int>) Context.Arguments[2]
            );

            return null;
        }
    }
}
