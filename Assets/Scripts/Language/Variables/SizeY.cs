using UnityEngine;

namespace Language.Variables {
    public class SizeY : SpecialVariable<float> {
        Size _size;

        void SetSize() {
            _size = BoundGameObject.GetComponent<Size>();
        }

        readonly System.Action _setSizeOnce;
        
        protected override float SpecialGet() {
            _setSizeOnce();

            return (_size.Value / EngineAPI.GetSizePosConverter().SizeM2U).y;
        }

        protected override void SpecialSet(float value) {
            _setSizeOnce();

            var y = (new Vector2(0, value) * EngineAPI.GetSizePosConverter().SizeM2U).y;
            _size.Value = new Vector2(_size.Value.x, y);
        }

        public SizeY(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI) : base(gameObject, engineAPI) {
            _setSizeOnce = ((System.Action) SetSize).Once();
        }
    }
}