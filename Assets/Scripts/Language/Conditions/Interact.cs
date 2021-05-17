using UnityEngine;

namespace Language.Conditions {
    public class Interact : Condition {
        static readonly IValue[][] ArgTypes = { };
        
        InteractableComponent _interactComponent;

        void SetInteractComponent() {
            _interactComponent = BoundGameObject.GetComponent<InteractableComponent>();
        }

        readonly System.Action _setInteractComponentOnce;
        
        public Interact(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, values, constraintReference) { 
            _setInteractComponentOnce = ((System.Action) SetInteractComponent).Once();
        }
        
        protected override bool ConditionLogic() {
            _setInteractComponentOnce();
            var res = _interactComponent.interact;
            _interactComponent.interact = false;   // TODO: or should i not?

            return res;
        }
    }
}
