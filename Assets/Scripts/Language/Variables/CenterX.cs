using UnityEngine;

namespace Language.Variables {
    public class CenterX : SpecialVariable<float> {
        Size _size;

        void SetSize() {
            _size = BoundGameObject.GetComponent<Size>();
        }

        readonly System.Action _setSizeOnce;
        
        protected override float SpecialGet() {
            _setSizeOnce();

            return EngineAPI.GetSizePosConverter().PositionU2M(BoundGameObject.transform.position).x;
        }

        protected override void SpecialSet(float value) {
            _setSizeOnce();

            var x = EngineAPI.GetSizePosConverter().PositionM2U(new Vector2(value, 0)).x;
            var pos = BoundGameObject.transform.position;
            BoundGameObject.transform.position = new Vector3(x, pos.y, pos.z);
        }

        public CenterX(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI) : base(gameObject, engineAPI) {
            _setSizeOnce = ((System.Action) SetSize).Once();
        }
    }
}