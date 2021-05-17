using UnityEngine;

namespace Player {
    public class PlayerInput : MonoBehaviour {
        public Vector2 DirectionInput {
            get {
                var horizontalInput = Input.GetAxisRaw("Horizontal");
                var verticalInput = Input.GetAxisRaw("Vertical");
                var input = new Vector2(horizontalInput, verticalInput);
                return input;
            }
        }

        bool _jumpInput;
        public bool JumpInput {
            get {
                var res = _jumpInput;
                _jumpInput = false;
                return res;
            }
        }

        bool _interactInput;
        public bool InteractInput {
            get {
                var res = _interactInput;
                _interactInput = false;
                return res;
            }
        }

        void Update() {
            _jumpInput = _jumpInput || Input.GetKeyDown(KeyCode.Space);
            _interactInput = _interactInput || Input.GetKeyDown(KeyCode.E);
        }
    }
}
