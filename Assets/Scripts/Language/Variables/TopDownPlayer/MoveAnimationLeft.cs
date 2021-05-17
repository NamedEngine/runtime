using Player;
using UnityEngine;

namespace Language.Variables.TopDownPlayer {
    public class MoveAnimationLeft : SpecialVariable<string> {
        PlayerTopDownAnimation _playerAnimation;

        void SetPlayerAnimation() {
            _playerAnimation = BoundGameObject.GetComponent<PlayerTopDownAnimation>();
        }

        readonly System.Action _setPlayerAnimationOnce;
        
        protected override string SpecialGet() {
            _setPlayerAnimationOnce();

            return _playerAnimation.MoveAnimationLeft;
        }

        protected override void SpecialSet(string value) {
            _setPlayerAnimationOnce();

            _playerAnimation.MoveAnimationLeft.Value = value;
        }

        public MoveAnimationLeft(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI) : base(gameObject, engineAPI) {
            _setPlayerAnimationOnce = ((System.Action) SetPlayerAnimation).Once();
        }
    }
}
