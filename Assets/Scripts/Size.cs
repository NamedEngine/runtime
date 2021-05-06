using System.Collections.Generic;
using UnityEngine;
public class Size : MonoBehaviour {
    Vector2 _value;
    List<BoxCollider2D> _colliders = new List<BoxCollider2D>();

    public Vector2 Value {
        get => _value;
        set {
            _value = value;
            ResizeColliders();
        }
    }

    public float Width => Value.x;
    public float Height => Value.y;

    public void RegisterCollider(BoxCollider2D newCollider) {
        _colliders.Add(newCollider);
    }

    void ResizeColliders() {
        _colliders
            .ForEach(c => c.size = Value);
    }
}