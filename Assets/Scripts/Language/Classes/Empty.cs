using System;
using Language.Variables;
using UnityEngine;
using static LogicUtils;

namespace Language.Classes {
    public class Empty : BaseClass {
        public override string ShouldInheritFrom() {
            return null;
        }

        public override string BaseClassName() {
            return nameof(Empty);
        }

        public override (string, Func<GameObject, LogicEngine.LogicEngineAPI, IVariable>)[] BaseVariables() {
            return new [] {
                GetSpecialVariablePair<Visible, bool>(true),
                GetSpecialVariablePair<DrawingLayer>(),
                GetSpecialVariablePair<Collidable, bool>(false),
                GetSpecialVariablePair<Interactable, bool>(false),
                GetSpecialVariablePair<CenterX>(),
                GetSpecialVariablePair<CenterY>(),
                GetSpecialVariablePair<Rotation>(),
                GetSpecialVariablePair<SizeX>(),
                GetSpecialVariablePair<SizeY>(),
                GetSpecialVariablePair<VelocityX>(),
                GetSpecialVariablePair<VelocityY>(),
            };
        }
    }
}
