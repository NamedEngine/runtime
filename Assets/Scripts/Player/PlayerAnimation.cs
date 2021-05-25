using System;
using UnityEngine;

namespace Player {
    public abstract class PlayerAnimation : MonoBehaviour {
        public abstract void Animate(Vector2 directionInput);

        public abstract void Setup(Vector2 standDirection, Func<Exception, bool> exceptHandler);
    }
}
