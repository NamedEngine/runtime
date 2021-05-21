using System;
using UnityEngine;

public class InstantiatorWrapper : IInstantiator {
    readonly GameObject _gameObject;
    
    public InstantiatorWrapper(GameObject gameObject) {
        _gameObject = gameObject;
    }
    
    public Component GetInstance(Type t) {
        return _gameObject.AddComponent(t);
    }
}
