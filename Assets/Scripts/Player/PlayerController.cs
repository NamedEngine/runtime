using System;
using System.Collections.Generic;
using Language.Variables;
using UnityEngine;

namespace Player {
    public class PlayerController : MonoBehaviour {
        PlayerInput _playerInput;
        PlayerMove _playerMove;
        PlayerInteract _playerInteract;
        PlayerAnimation _playerAnimation;

        VelocityX _velocityX;
        VelocityY _velocityY;
        SizePositionConverter _converter;

        bool _isSet;

        public void Setup(IReadOnlyDictionary<string, IVariable> variables, LogicEngine.LogicEngineAPI engineAPI, Func<Exception, bool> exceptHandler) {
            _velocityX = variables[nameof(VelocityX)] as VelocityX;
            _velocityY = variables[nameof(VelocityY)] as VelocityY;
            _converter = engineAPI.GetSizePosConverter();

            _playerAnimation.Setup(Vector2.down, exceptHandler);
            _isSet = true;
        }

        void Awake() {
            _playerInput = GetComponent<PlayerInput>();
            _playerMove = GetComponent<PlayerMove>();
            _playerInteract = GetComponent<PlayerInteract>();
            _playerAnimation = GetComponent<PlayerAnimation>();
        }

        void FixedUpdate() {
            if (!_isSet) {
                return;
            }

            var directionInput = _playerInput.DirectionInput;

            _playerMove.Move(directionInput, _playerInput.JumpInput, _converter, _velocityX, _velocityY);
            if (_playerInput.InteractInput) {
                 _playerInteract.Interact();   
            }

            _playerAnimation.Animate(directionInput);
        }
    }
}
