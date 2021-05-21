using Player;
using UnityEngine;

public class InteractableComponent : MonoBehaviour {
    BoxCollider2D _collider;
    public Size size;

    public bool Interactable {
        get {
            _setColliderOnce();

            return _collider.enabled;
        }
        set {
            _setColliderOnce();
            
            _collider.enabled = value;
        }
    }

    bool _canInteract;
    public bool CanInteract => _canInteract;
    public bool interact;

    InteractableComponent() {
        _setColliderOnce = ((System.Action) SetCollider).Once();
    }
    
    void SetCollider() {
        _collider = gameObject.AddComponent<BoxCollider2D>();
        _collider.enabled = false;
        _collider.isTrigger = true;
        
        size = gameObject.GetComponent<Size>();
        size.RegisterCollider(_collider);
        _collider.size = size.Value;
    }

    readonly System.Action _setColliderOnce;

    bool _shouldDisableInteract;
    void Update() {
        if (_shouldDisableInteract) {
            interact = false;
            _shouldDisableInteract = false;
        }

        if (interact) {
            _shouldDisableInteract = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (!Interactable || !other.gameObject.CompareTag("Player")) {
            return;
        }

        other.gameObject.GetComponent<PlayerInteract>().RegisterInteractable(this);
        _canInteract = true;
    }

    void OnTriggerExit2D(Collider2D other) {
        if (!Interactable || !other.gameObject.CompareTag("Player")) {
            return;
        }
        
        other.gameObject.GetComponent<PlayerInteract>().UnregisterInteractable(this);
        _canInteract = false;
    }
}
