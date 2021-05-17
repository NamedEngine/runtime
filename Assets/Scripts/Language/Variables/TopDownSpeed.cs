using Player;
using UnityEngine;

namespace Language.Variables {
    public class TopDownSpeed : SpecialVariable<float> {
        PlayerTopDownMove _playerMove;

        void SetPlayerMove() {
            _playerMove = BoundGameObject.GetComponent<PlayerTopDownMove>();
        }

        readonly System.Action _setPlayerMoveOnce;
        
        protected override float SpecialGet() {
            _setPlayerMoveOnce();

            return _playerMove.speed / EngineAPI.GetSizePosConverter().SizeM2U;
        }

        protected override void SpecialSet(float value) {
            _setPlayerMoveOnce();

            _playerMove.speed = value * EngineAPI.GetSizePosConverter().SizeM2U;
        }

        public TopDownSpeed(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI) : base(gameObject, engineAPI) {
            _setPlayerMoveOnce = ((System.Action) SetPlayerMove).Once();
        }
    }
}
