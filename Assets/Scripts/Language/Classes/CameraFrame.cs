using System;
using Language.Variables;
using Language.Variables.CameraFrame;
using UnityEngine;
using static LogicUtils;

namespace Language.Classes {
    public class CameraFrame : BaseClass {
        static int _instanceNum;
        public Size size;
        
        protected override void BeforeStartProcessingInternal() {
            _instanceNum++;
            if (_instanceNum > 1) {
                throw new LogicException(nameof(CameraFrame), "Only one CameraFrame should exist");
            }

            size = GetComponent<Size>();
            Camera.main.GetComponent<CameraController>().SetFrame(this);
        }

        public override string ShouldInheritFrom() {
            return null;
        }

        public override string BaseClassName() {
            return nameof(CameraFrame);
        }

        public override (string, Func<GameObject, LogicEngine.LogicEngineAPI, IVariable>)[] BaseVariables() {
            return new [] {
                GetSpecialVariablePair<CenterX>(),
                GetSpecialVariablePair<CenterY>(),
                GetSpecialVariablePair<SizeX>(),
                GetSpecialVariablePair<SizeY>(),
                GetSpecialVariablePair<MaxCameraYSize, float>(0),
            };
        }
    }
}
