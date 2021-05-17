using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Player {
    public class PlayerInteract : MonoBehaviour {
        readonly HashSet<InteractableComponent> _interactables = new HashSet<InteractableComponent>();
        Size _size;

        void Start() {
            _size = GetComponent<Size>();
        }

        public void RegisterInteractable(InteractableComponent interactable) {
            _interactables.Add(interactable);
        }
        
        public void UnregisterInteractable(InteractableComponent interactable) {
            _interactables.Remove(interactable);
        }

        public void Interact() {
            if (_interactables.Count == 0) {
                return;
            }

            var center = (Vector2) transform.position + _size.Value / 2f;
            var closest = _interactables
                .Select(interactable => (interactable, (Vector2) interactable.transform.position + interactable.size.Value / 2f))  // interactable and its center
                .Select(pair => (pair.interactable, (center - pair.Item2).magnitude))  // interactable and distance to its center
                .Aggregate((acc, pair) => pair.magnitude < acc.magnitude ? pair : acc)
                .interactable;

            closest.interact = true;
        }
    }
}
