using UnityEngine;

namespace Language.Variables {
    public class ScaleX : SpecialVariable<float> {
        protected override float SpecialGet() {
            return BoundGameObject.transform.localScale.x;
        }

        protected override void SpecialSet(float value) {
            var scale = BoundGameObject.transform.localScale;
            BoundGameObject.transform.localScale = new Vector3(value, scale.y, scale.z);
        }

        public ScaleX(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI) : base(gameObject, engineAPI) { }
    }
}