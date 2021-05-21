using Player;
using UnityEngine;

namespace Language.Variables.TopDownPlayer {
    public class AnimationTime : SpecialVariable<float> {
        PlayerTopDownAnimation _playerAnimation;

        void SetPlayerAnimation() {
            _playerAnimation = BoundGameObject.GetComponent<PlayerTopDownAnimation>();
        }

        readonly System.Action _setPlayerAnimationOnce;
        
        protected override float SpecialGet() {
            _setPlayerAnimationOnce();

            return _playerAnimation.animationTime;
        }

        protected override void SpecialSet(float value) {
            _setPlayerAnimationOnce();

            _playerAnimation.animationTime = value;
        }

        public AnimationTime(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI) : base(gameObject, engineAPI) {
            _setPlayerAnimationOnce = ((System.Action) SetPlayerAnimation).Once();
        }
    }
}
