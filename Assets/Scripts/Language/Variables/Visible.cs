using UnityEngine;

namespace Language.Variables {
    public class Visible : SpecialVariable<bool> {
        SpriteRenderer _renderer;

        void SetRenderer() {
            _renderer = BoundGameObject.GetComponent<SpriteRenderer>();
        }

        readonly System.Action _setRendererOnce;
        
        protected override bool SpecialGet() {
            _setRendererOnce();

            return _renderer.enabled;
        }

        protected override void SpecialSet(bool value) {
            _setRendererOnce();

            _renderer.enabled = value;
        }

        public Visible(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI) : base(gameObject, engineAPI) {
            _setRendererOnce = ((System.Action) SetRenderer).Once();
        }
    }
}
