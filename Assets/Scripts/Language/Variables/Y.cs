using UnityEngine;

namespace Language.Variables {
    public class Y : SpecialVariable<float> {
        Size _size;

        void SetSize() {
            _size = BoundGameObject.GetComponent<Size>();
        }

        readonly System.Action _setSizeOnce;
        
        protected override float InternalGet() {
            _setSizeOnce();

            return EngineAPI.GetSizePosConverter().PositionU2M(BoundGameObject.transform.position, _size.Height).y;
        }

        public override void Set(float value) {
            _setSizeOnce();

            var y = EngineAPI.GetSizePosConverter().PositionM2U(new Vector2(0, value), _size.Height).y;
            var pos = BoundGameObject.transform.position;
            BoundGameObject.transform.position = new Vector3(pos.x, y, pos.z);
        }

        public Y(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI) : base(gameObject, engineAPI) {
            _setSizeOnce = ((System.Action) SetSize).Once();
        }
    }
}