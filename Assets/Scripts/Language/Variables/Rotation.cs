using UnityEngine;
using Quaternion = UnityEngine.Quaternion;

namespace Language.Variables {
    public class Rotation : SpecialVariable<float> {
        protected override float SpecialGet() {
            return BoundGameObject.transform.rotation.eulerAngles.z * -1;
        }

        protected override void SpecialSet(float value) {
            var rotation = BoundGameObject.transform.rotation.eulerAngles;
            BoundGameObject.transform.rotation = Quaternion.Euler(rotation.x, rotation.y, -1 * value % 360);
        }

        public Rotation(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI) : base(gameObject, engineAPI) { }
    }
}