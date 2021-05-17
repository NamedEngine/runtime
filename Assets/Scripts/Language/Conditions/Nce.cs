using System;
using UnityEngine;

namespace Language.Conditions {
    public class Nce : Condition {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<int>()}
        };

        int _activationsLeft;

        void SetInteractions() {
            _activationsLeft = (Value<int>) Arguments[0];
        }
        
        System.Action _setInteractionsOnce;

        public Nce(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, values, constraintReference) {
            _setInteractionsOnce = ((System.Action) SetInteractions).Once();
        }

        protected override bool ConditionLogic() {
            _setInteractionsOnce();
            
            _activationsLeft = Math.Max(-1, --_activationsLeft);
            return _activationsLeft >= 0;
        }
    }
}
