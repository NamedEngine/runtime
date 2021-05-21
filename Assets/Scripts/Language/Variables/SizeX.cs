using UnityEngine;

namespace Language.Variables {
    public class SizeX : SpecialVariable<float> {
        Size _size;

        void SetSize() {
            _size = BoundGameObject.GetComponent<Size>();
        }

        readonly System.Action _setSizeOnce;
        
        protected override float SpecialGet() {
            _setSizeOnce();

            return (_size.Value / EngineAPI.GetSizePosConverter().SizeM2U).x;
        }

        protected override void SpecialSet(float value) {
            _setSizeOnce();

            var x = (new Vector2(value, 0) * EngineAPI.GetSizePosConverter().SizeM2U).x;
            _size.Value = new Vector2(x, _size.Value.y);
        }

        public SizeX(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI) : base(gameObject, engineAPI) {
            _setSizeOnce = ((System.Action) SetSize).Once();
        }
    }
}