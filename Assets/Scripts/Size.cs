using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class Size : MonoBehaviour {
    [SerializeField] Vector2 value;
    readonly HashSet<BoxCollider2D> _colliders = new HashSet<BoxCollider2D>();

    public Vector2 Value {
        get => value;
        set {
            this.value = value;
            ResizeColliders();
        }
    }

    public readonly List<System.Action> AfterResize = new List<System.Action>();

    public float Width => Value.x;
    public float Height => Value.y;

    public void RegisterCollider(BoxCollider2D newCollider) {
        _colliders.Add(newCollider);
    }

    void ResizeColliders() {
        _colliders
            .ToList()
            .ForEach(c => {
                c.size = Value;
            });
        
        AfterResize.ForEach(a => a());
    }
}
