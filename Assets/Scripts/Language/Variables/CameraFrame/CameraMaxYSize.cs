using System.Linq;
using UnityEngine;

namespace Language.Variables.CameraFrame {
    public class CameraMaxYSize : SpecialVariable<float> {
        CameraController _cameraController;
        const int PlayerSizesInCamera = 5;
        const float MinSize = 0.00001f;

        void SetCamera() {
            _cameraController = Camera.main.GetComponent<CameraController>();
        }

        readonly System.Action _setCameraOnce;

        
        protected override float SpecialGet() {
            _setCameraOnce();
            
            return _cameraController.maxHeight;
        }

        protected override void SpecialSet(float value) {
            _setCameraOnce();

            if (value < 0) {
                throw new LogicException(nameof(CameraMaxYSize), $"Can't set camera size to negative value {value}");
            }

            if (value < MinSize) {
                var players = GameObject.FindGameObjectsWithTag("Player");
                if (players.Length == 0) {
                    _cameraController.maxHeight = MinSize;
                    return;
                }

                var meanSize = players
                    .Select(p => p.GetComponent<Size>())
                    .Aggregate(0f, (res, size) => res + size.Value.y) / players.Length;

                _cameraController.maxHeight = Mathf.Max(PlayerSizesInCamera * meanSize, MinSize);
                return;
            }
            
            _cameraController.maxHeight = value;
        }

        public CameraMaxYSize(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI) : base(gameObject, engineAPI) {
            _setCameraOnce = ((System.Action) SetCamera).Once();
        }
    }
}
