using System;
using System.Linq;
using Language.Variables;
using Player;
using UnityEngine;
using static LogicUtils;

namespace Language.Classes {
    public class TopDownPlayer : BaseClass {
        void Start() {
            if (EngineAPI == null) {
                return;
            }

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
            
        }

        public override string ShouldInheritFrom() {
            return nameof(Empty);
        }

        public override string BaseClassName() {
            return nameof(TopDownPlayer);
        }

        public override (string, Func<GameObject, LogicEngine.LogicEngineAPI, IVariable>)[] BaseVariables() {
            return new [] {
                GetSpecialVariablePair<TopDownSpeed>(),
                GetSpecialVariablePair<Collidable, bool>(true),
            };
        }
    }
}
