using System;
using System.Collections.Generic;
using UnityEngine;

public class TemporaryInstantiator : MonoBehaviour {
    readonly List<Component> _instantiated = new List<Component>();
    
    public Component GetInstance(Type t) {
        var component = gameObject.AddComponent(t);
        _instantiated.Add(component);
        return component;
    }

    public void Clear() {
        foreach (var component in _instantiated) {
            Destroy(component);
        }
        
        _instantiated.Clear();
    }
}