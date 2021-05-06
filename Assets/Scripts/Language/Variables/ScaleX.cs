using UnityEngine;

namespace Language.Variables {
    public class ScaleX : SpecialVariable<float> {
        protected override float InternalGet() {
            return BoundGameObject.transform.localScale.x;
        }

        public override void Set(float value) {
            var scale = BoundGameObject.transform.localScale;
            BoundGameObject.transform.localScale = new Vector3(value, scale.y, scale.z);
        }

        public ScaleX(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI) : base(gameObject, engineAPI) { }
    }
}