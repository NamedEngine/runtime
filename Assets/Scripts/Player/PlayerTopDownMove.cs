using Language.Variables;
using UnityEngine;

namespace Player {
    public class PlayerTopDownMove : PlayerMove {
        public float speed;

        public override void Move(Vector2 directionInput, bool jumpInput, SizePositionConverter converter,
            VelocityX velocityX, VelocityY velocityY) {
            var velocity = converter.DirectionU2M(directionInput.normalized * speed);

            velocityX.Set(velocity.x);
            velocityY.Set(velocity.y);
        }
    }
}
