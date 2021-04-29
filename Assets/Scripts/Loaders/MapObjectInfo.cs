using UnityEngine;

public struct MapObjectInfo {
    public GameObject GameObject;
    public string Name;
    public Rect Rect;
    public MapObjectParameter[] Parameters;
}

public struct MapObjectParameter {
    public string Name;
    public ValueType Type;
    public string Value;
    public override string ToString() {
        return Name + ": " + Type + " = " + Value;
    }

    public bool IsDefault() {
        return Equals(this, new MapObjectParameter());
    }
}
