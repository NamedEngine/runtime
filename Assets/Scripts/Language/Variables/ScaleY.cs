using UnityEngine;

namespace Language.Variables {
    public class ScaleY : SpecialVariable<float> {
        protected override float SpecialGet() {
            return BoundGameObject.transform.localScale.y;
        }

        protected override void SpecialSet(float value) {
            var scale = BoundGameObject.transform.localScale;
            BoundGameObject.transform.localScale = new Vector3(scale.x, value, scale.z);
        }

        public ScaleY(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI) : base(gameObject, engineAPI) { }
    }
}