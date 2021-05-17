using UnityEngine;

namespace Language.Variables {
    public class Interactable : SpecialVariable<bool> {
        InteractableComponent _interactComponent;

        void SetInteractComponent() {
            _interactComponent = BoundGameObject.AddComponent<InteractableComponent>();
        }

        readonly System.Action _setInteractComponentOnce;

        protected override bool SpecialGet() {
            _setInteractComponentOnce();

            return _interactComponent.Interactable;
        }

        protected override void SpecialSet(bool value) {
            _setInteractComponentOnce();
            
            _interactComponent.Interactable = value;
        }

        public Interactable(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI) : base(gameObject, engineAPI) {
            _setInteractComponentOnce = ((System.Action) SetInteractComponent).Once();
        }
    }
}
