using UnityEngine;

namespace Variables {
    public class DummyVisible : SpecialVariable<bool> {
        SpriteRenderer _renderer;

        void SetRenderer() {
            _renderer = BoundGameObject.GetComponent<SpriteRenderer>();
        }

        readonly System.Action _setRendererOnce;
        
        protected override bool InternalGet() {
            _setRendererOnce();

            return _renderer.enabled;
        }

        public override void Set(bool value) {
            _setRendererOnce();

            _renderer.enabled = value;
        }

        public DummyVisible(GameObject gameObject) : base(gameObject) {
            _setRendererOnce = ((System.Action) SetRenderer).Once();
        }
    }
}
