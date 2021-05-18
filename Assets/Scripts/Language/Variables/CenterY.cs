using UnityEngine;

namespace Language.Variables {
    public class CenterY : SpecialVariable<float> {
        Size _size;

        void SetSize() {
            _size = BoundGameObject.GetComponent<Size>();
        }

        readonly System.Action _setSizeOnce;
        
        protected override float SpecialGet() {
            _setSizeOnce();

            return EngineAPI.GetSizePosConverter().PositionU2M(BoundGameObject.transform.position).y;
        }

        protected override void SpecialSet(float value) {
            _setSizeOnce();

            var y = EngineAPI.GetSizePosConverter().PositionM2U(new Vector2(0, value)).y;
            var pos = BoundGameObject.transform.position;
            BoundGameObject.transform.position = new Vector3(pos.x, y, pos.z);
        }

        public CenterY(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI) : base(gameObject, engineAPI) {
            _setSizeOnce = ((System.Action) SetSize).Once();
        }
    }
}