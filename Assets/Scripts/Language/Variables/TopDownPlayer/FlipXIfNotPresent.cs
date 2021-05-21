using Player;
using UnityEngine;

namespace Language.Variables.TopDownPlayer {
    public class FlipXIfNotPresent : SpecialVariable<bool> {
        PlayerTopDownAnimation _playerAnimation;

        void SetPlayerAnimation() {
            _playerAnimation = BoundGameObject.GetComponent<PlayerTopDownAnimation>();
        }

        readonly System.Action _setPlayerAnimationOnce;
        
        protected override bool SpecialGet() {
            _setPlayerAnimationOnce();

            return _playerAnimation.FlipXIfNotPresent;
        }

        protected override void SpecialSet(bool value) {
            _setPlayerAnimationOnce();

            _playerAnimation.FlipXIfNotPresent.Value = value;
        }

        public FlipXIfNotPresent(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI) : base(gameObject, engineAPI) {
            _setPlayerAnimationOnce = ((System.Action) SetPlayerAnimation).Once();
        }
    }
}
