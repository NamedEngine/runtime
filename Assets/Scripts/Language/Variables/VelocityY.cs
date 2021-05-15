using UnityEngine;

namespace Language.Variables {
    public class VelocityY : SpecialVariable<float> {
        Rigidbody2D _rb;

        void SetRigidBody() {
            _rb = BoundGameObject.GetComponent<Rigidbody2D>();
        }

        readonly System.Action _setRigidBodyOnce;
        
        protected override float InternalGet() {
            _setRigidBodyOnce();

            return EngineAPI.GetSizePosConverter().DirectionU2M(_rb.velocity).y;
        }

        public override void Set(float value) {
            _setRigidBodyOnce();

            var vel = _rb.velocity;
            var y = EngineAPI.GetSizePosConverter().DirectionM2U(new Vector2(0, value)).y;
            _rb.velocity = new Vector2(vel.x, y);
        }

        public VelocityY(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI) : base(gameObject, engineAPI) {
            _setRigidBodyOnce = ((System.Action) SetRigidBody).Once();
        }
    }
}