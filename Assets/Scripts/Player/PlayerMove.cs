using Language.Variables;
using UnityEngine;

namespace Player {
    public abstract class PlayerMove : MonoBehaviour {
        public abstract void Move(Vector2 directionInput, bool jumpInput, SizePositionConverter converter, VelocityX velocityX, VelocityY velocityY);
    }
}
