using System.Collections.Generic;
using Language.Variables;
using UnityEngine;

namespace Player {
    public class PlayerController : MonoBehaviour {
        PlayerInput _playerInput;
        PlayerMove _playerMove;
        PlayerInteract _playerInteract;

        VelocityX _velocityX;
        VelocityY _velocityY;
        SizePositionConverter _converter;

        bool _isSet;

        public void Setup(IReadOnlyDictionary<string, IVariable> variables, LogicEngine.LogicEngineAPI engineAPI) {
            _velocityX = variables[nameof(VelocityX)] as VelocityX;
            _velocityY = variables[nameof(VelocityY)] as VelocityY;
            _converter = engineAPI.GetSizePosConverter();
            _isSet = true;
        }

        void Start() {
            _playerInput = GetComponent<PlayerInput>();
            _playerMove = GetComponent<PlayerMove>();
            _playerInteract = GetComponent<PlayerInteract>();
        }

        void FixedUpdate() {
            if (!_isSet) {
                return;
            }
            
            _playerMove.Move(_playerInput.DirectionInput, _playerInput.JumpInput, _converter, _velocityX, _velocityY);
            if (_playerInput.InteractInput) {
                 _playerInteract.Interact();   
            }
        }
    }
}
