using UnityEngine;

namespace Language.Variables {
    public class Collidable : SpecialVariable<bool> {
        BoxCollider2D _collider;
        Size _size;

        void SetCollider() {
            _collider = BoundGameObject.AddComponent<BoxCollider2D>();
            _collider.enabled = false;
            
            _size = BoundGameObject.GetComponent<Size>();
            _size.RegisterCollider(_collider);
            _collider.size = _size.Value;
            _collider.offset = _size.Value / 2;
        }

        readonly System.Action _setColliderOnce;

        protected override bool SpecialGet() {
            _setColliderOnce();

            return _collider.enabled;
        }

        protected override void SpecialSet(bool value) {
            _setColliderOnce();

            _collider.enabled = value;
        }

        public Collidable(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI) : base(gameObject, engineAPI) {
            _setColliderOnce = ((System.Action) SetCollider).Once();
        }
    }
}