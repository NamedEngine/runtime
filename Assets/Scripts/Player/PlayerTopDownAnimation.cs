using System;
using System.Collections.Generic;
using UnityEngine;

namespace Player {
    public class PlayerTopDownAnimation : PlayerAnimation {
        SpriteAnimator _animator;
        
        public readonly RefWrapper<bool> FlipXIfNotPresent = new RefWrapper<bool>(true);
        public readonly RefWrapper<bool> FlipYIfNotPresent = new RefWrapper<bool>(true);
        public float animationTime = 1;
        public bool isXMoreImportant;
        // public bool previousOnDiagonal;
        
        public readonly RefWrapper<string> StandAnimationRight = new RefWrapper<string>(/*"StandRight"*/);
        public readonly RefWrapper<string> StandAnimationLeft = new RefWrapper<string>(/*"StandLeft"*/);
        public readonly RefWrapper<string> StandAnimationUp = new RefWrapper<string>(/*"StandUp"*/);
        public readonly RefWrapper<string> StandAnimationDown = new RefWrapper<string>(/*"StandDown"*/);
        
        public readonly RefWrapper<string> MoveAnimationRight = new RefWrapper<string>();
        public readonly RefWrapper<string> MoveAnimationLeft = new RefWrapper<string>();
        public readonly RefWrapper<string> MoveAnimationUp = new RefWrapper<string>();
        public readonly RefWrapper<string> MoveAnimationDown = new RefWrapper<string>();
        
        
        Vector2 _previousPrimaryDirection = Vector2.zero;

        Dictionary<Vector2, RefWrapper<string>> _moveAnimations;
        Dictionary<Vector2, RefWrapper<string>> _standAnimations;

        Dictionary<Vector2, RefWrapper<bool>> _flipConditionByDirection;
        Dictionary<Vector2, (bool, bool)> _flipAlongDirection;
        
        Dictionary<Vector2, (bool, bool)> _flipXYAlongDirection;

        PlayerTopDownAnimation() {
            _standAnimations = new Dictionary<Vector2, RefWrapper<string>> {
                {Vector2.right, StandAnimationRight},
                {Vector2.left, StandAnimationLeft},
                {Vector2.up, StandAnimationUp},
                {Vector2.down, StandAnimationDown},
            };
            
            _moveAnimations = new Dictionary<Vector2, RefWrapper<string>> {
                {Vector2.right, MoveAnimationRight},
                {Vector2.left, MoveAnimationLeft},
                {Vector2.up, MoveAnimationUp},
                {Vector2.down, MoveAnimationDown},
            };
            
            _flipConditionByDirection = new Dictionary<Vector2, RefWrapper<bool>> {
                {Vector2.right, FlipXIfNotPresent},
                {Vector2.left, FlipXIfNotPresent},
                {Vector2.up, FlipYIfNotPresent},
                {Vector2.down, FlipYIfNotPresent},
            };

            _flipXYAlongDirection = new Dictionary<Vector2, (bool, bool)> {
                {Vector2.right, (true, false)},
                {Vector2.left, (true, false)},
                {Vector2.up, (false, true)},
                {Vector2.down, (false, true)},
            };

            isXMoreImportant = true;
        }
        
        public override void Setup(Vector2 standDirection) {
            _animator = GetComponent<SpriteAnimator>();
            _animator.UpdateDefaultSprite();
            SetStanding(standDirection);
        }
        
        public override void Animate(Vector2 directionInput) {
            var (primaryDir, _) = DirectionToComponents(directionInput);
            if (primaryDir == _previousPrimaryDirection) {
                return;
            }

            if (primaryDir == Vector2.zero) {
                SetStanding(_previousPrimaryDirection);
                _previousPrimaryDirection = Vector2.zero;
                return;
            }

            var movingSet = TrySetAnimation(directionInput, _moveAnimations, _previousPrimaryDirection);
            if (!movingSet) {
                SetStanding(directionInput, _previousPrimaryDirection);
            }

            _previousPrimaryDirection = primaryDir;
        }

        void SetStanding(Vector2 direction, Vector2? previousDirection = null) {
            var set = TrySetAnimation(direction, _standAnimations, previousDirection);
            if (set) {
                return;
            }
            
            var directionWithoutZeroAxes = PopulateDirectionAxes(direction);
            var (primaryDir, _) = DirectionToComponents(directionWithoutZeroAxes);
            
            // do not flip to right and down, flip to up and left
            var isPointingTowardRigthOrDown = primaryDir.x + -1 * primaryDir.y > 0;
            if (!isPointingTowardRigthOrDown && _flipConditionByDirection[primaryDir]) {
                var (flipX, flipY) = _flipXYAlongDirection[primaryDir];
                _animator.SetDefaultSprite(flipX, flipY);
            } else {
                _animator.SetDefaultSprite();
            }
        }
        
        bool TrySetAnimation(Vector2 direction, Dictionary<Vector2, RefWrapper<string>> animations, Vector2? previousDirection = null) {
            var directionWithoutZeroAxes = PopulateDirectionAxes(direction);
            var (primaryDir, secondaryDir) = DirectionToComponents(directionWithoutZeroAxes, previousDirection);

            var primarySet = TrySetDirectionAnimation(primaryDir, animations, true);
            if (primarySet) {
                return true;
            }

            var secondarySet = TrySetDirectionAnimation(secondaryDir, animations, false);
            return secondarySet;
        }

        bool TrySetDirectionAnimation(Vector2 direction, Dictionary<Vector2, RefWrapper<string>> animations, bool isPrimary) {
            var animationPath = animations[direction];
            if (!string.IsNullOrEmpty(animationPath)) {
                _animator.SetAnimation(animationPath, animationTime, 0);
                return true;
            }
            
            var oppositeAnimationPath = animations[direction * -1];
            if (string.IsNullOrEmpty(oppositeAnimationPath)) {
                return false;
            } 
            
            if (_flipConditionByDirection[direction] && isPrimary) {
                var (flipX, flipY) = _flipXYAlongDirection[direction];
                _animator.SetAnimation(oppositeAnimationPath, animationTime, 0, flipX, flipY);
            } else {
                _animator.SetAnimation(oppositeAnimationPath, animationTime, 0);
            }

            return true;
        }

        (Vector2, Vector2) DirectionToComponents(Vector2 direction, Vector2? previous = null) {
            if (direction == Vector2.zero) {
                return (Vector2.zero, Vector2.zero);
            }
            
            // TODO: use PreviousOnDiagonal someday
            /*if (continuePrevious
                && previous is Vector2 previousDirection && previousDirection != Vector2.zero
                &&  Mathf.Approximately(Math.Abs(direction.x), Math.Abs(direction.y))) {

                var secondaryDirection = direction.normalized * 2 - previousDirection;
                return (previousDirection, secondaryDirection);
            }*/

            bool isXPrimary;
            if (isXMoreImportant) {
                isXPrimary = Math.Abs(direction.x) >= Math.Abs(direction.y);
            } else {
                isXPrimary = Math.Abs(direction.x) > Math.Abs(direction.y);
            }
             
            var xDirection = new Vector2(direction.x, 0).normalized;
            var yDirection = new Vector2(0, direction.y).normalized;
            
            return isXPrimary ? (xDirection, yDirection) : (yDirection, xDirection);
        }

        static Vector2 PopulateDirectionAxes(Vector2 direction) {
            if (direction.x == 0) {
                direction += Vector2.right * .001f;
            }
            if (direction.y == 0) {
                direction += Vector2.down * .001f;
            }
            return direction;
        }
    }
}
