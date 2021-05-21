using UnityEngine;

namespace Language.Variables {
    public class VelocityX : SpecialVariable<float> {
        Rigidbody2D _rb;

        void SetRigidBody() {
            _rb = BoundGameObject.GetComponent<Rigidbody2D>();
        }

        readonly System.Action _setRigidBodyOnce;
        
        protected override float SpecialGet() {
            _setRigidBodyOnce();

            return EngineAPI.GetSizePosConverter().DirectionU2M(_rb.velocity).x;
        }

        protected override void SpecialSet(float value) {
            _setRigidBodyOnce();

            var vel = _rb.velocity;
            var x = EngineAPI.GetSizePosConverter().DirectionM2U(new Vector2(value, 0)).x;
            _rb.velocity = new Vector2(x, vel.y);
        }

        public VelocityX(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI) : base(gameObject, engineAPI) {
            _setRigidBodyOnce = ((System.Action) SetRigidBody).Once();
        }
    }
}