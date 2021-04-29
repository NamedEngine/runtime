using UnityEngine;

namespace Variables {
    public class DummyVisible : SpecialVariable<bool> {
        SpriteRenderer _renderer;

        void SetRenderer() {
            _renderer = BoundGameObject.GetComponent<SpriteRenderer>();
        }
        
        protected override bool InternalGet() {
            if (!_renderer) {
                SetRenderer();
            }

            return _renderer.enabled;
        }

        public override void Set(bool value) {
            if (!_renderer) {
                SetRenderer();
            }

            _renderer.enabled = value;
        }

        public DummyVisible(GameObject gameObject) : base(gameObject) { }
    }
}
