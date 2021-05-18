using System.Linq;
using UnityEngine;

namespace Language.Variables.CameraFrame {
    public class MaxCameraYSize : SpecialVariable<float> {
        Camera _camera;
        const int PlayerSizesInCamera = 5;

        void SetCamera() {
            _camera = Camera.main;
        }

        readonly System.Action _setCameraOnce;

        
        protected override float SpecialGet() {
            _setCameraOnce();
            
            return _camera.orthographicSize * 2;
        }

        protected override void SpecialSet(float value) {
            _setCameraOnce();
            if (value < 0) {
                throw new LogicException(nameof(MaxCameraYSize), $"Can't set camera size to negative value {value}");
            }

            if (value == 0) {
                var players = GameObject.FindGameObjectsWithTag("Player");
                if (players.Length == 0) {
                    _camera.orthographicSize = float.Epsilon;
                    return;
                }

                var meanSize = players
                    .Select(p => p.GetComponent<Size>())
                    .Aggregate(0f, (res, size) => res + size.Value.y) / players.Length;

                _camera.orthographicSize = PlayerSizesInCamera * meanSize / 2;
                return;
            }
            
            _camera.orthographicSize = value / 2;
        }

        public MaxCameraYSize(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI) : base(gameObject, engineAPI) {
            _setCameraOnce = ((System.Action) SetCamera).Once();
        }
    }
}
