using UnityEngine;

namespace Language.Variables {
    public class Collidable : SpecialVariable<bool> {
        BoxCollider2D _collider;

        void SetCollider() {
            _collider = BoundGameObject.AddComponent<BoxCollider2D>();
            var size = BoundGameObject.GetComponent<Size>();
            size.RegisterCollider(_collider);
            _collider.size = size.Value;
            _collider.enabled = false;
        }

        readonly System.Action _setColliderOnce;

        protected override bool InternalGet() {
            _setColliderOnce();

            return _collider.enabled;
        }

        public override void Set(bool value) {
            _setColliderOnce();
            
            // TODO: also set Interactable to false if Collidable is being set to true
            
            _collider.enabled = value;
        }

        public Collidable(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI) : base(gameObject, engineAPI) {
            _setColliderOnce = ((System.Action) SetCollider).Once();
        }
    }
}