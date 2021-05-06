using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;

namespace Language.Variables {
    public class Rotation : SpecialVariable<float> {
        protected override float InternalGet() {
            return BoundGameObject.transform.rotation.eulerAngles.z * -1;
        }

        public override void Set(float value) {
            var rotation = BoundGameObject.transform.rotation.eulerAngles;
            BoundGameObject.transform.rotation = Quaternion.Euler(rotation.x, rotation.y, -1 * value % 360);
        }

        public Rotation(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI) : base(gameObject, engineAPI) { }
    }
}