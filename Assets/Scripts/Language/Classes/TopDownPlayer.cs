using System;
using System.Linq;
using Language.Variables;
using Language.Variables.TopDownPlayer;
using Player;
using UnityEngine;
using static LogicUtils;

namespace Language.Classes {
    public class TopDownPlayer : BaseClass {
        protected override void BeforeStartProcessingInternal() {
            GetComponents<BoxCollider2D>()
                .Where(coll => !coll.isTrigger)
                .ToList()
                .ForEach(coll => coll.size *= .9875f);

            
            void AdjustColliders() {
                var playerController = GetComponent<PlayerController>();
                if (playerController) {
                    playerController.Setup(Variables, EngineAPI);
                }
            }
            
            AdjustColliders();
            GetComponent<Size>().AfterResize.Add(AdjustColliders);
            
            Camera.main.GetComponent<CameraController>().AddPlayer(this);
        }

        public override string ShouldInheritFrom() {
            return nameof(Empty);
        }

        public override string BaseClassName() {
            return nameof(TopDownPlayer);
        }

        public override (string, Func<GameObject, LogicEngine.LogicEngineAPI, IVariable>)[] BaseVariables() {
            return new [] {
                GetSpecialVariablePair<Collidable, bool>(true),
                GetSpecialVariablePair<TopDownSpeed>(),
                GetSpecialVariablePair<AnimationTime, float>(1),
                GetSpecialVariablePair<IsXMoreImportant>(),
                GetSpecialVariablePair<FlipXIfNotPresent>(),
                GetSpecialVariablePair<FlipYIfNotPresent>(),
                GetSpecialVariablePair<MoveAnimationRight>(),
                GetSpecialVariablePair<MoveAnimationLeft>(),
                GetSpecialVariablePair<MoveAnimationUp>(),
                GetSpecialVariablePair<MoveAnimationDown>(),
                GetSpecialVariablePair<StandAnimationRight>(),
                GetSpecialVariablePair<StandAnimationLeft>(),
                GetSpecialVariablePair<StandAnimationUp>(),
                GetSpecialVariablePair<StandAnimationDown>(),
            };
        }
    }
}
