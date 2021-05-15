using UnityEngine;

public struct MapObjectInfo {
    public string Name;
    public Rect Rect;
    public string SortingLayer;
    public Sprite Sprite;
    public MapObjectParameter[] Parameters;

    public override string ToString() {
        return $"\"{Name}\" {Rect} Layer: {SortingLayer}\nParams: {string.Join("; ", Parameters)}";
    }

    public static MapObjectInfo GetEmpty() {
        return new MapObjectInfo {
            Name = "",
            Rect = new Rect(),
            Parameters = new MapObjectParameter[] { }
        };
    }
}

public struct MapObjectParameter {
    public string Name;
    public ValueType Type;
    public string Value;
    public override string ToString() {
        return $"{Name}: {Type} = {Value}";
    }

    public bool IsDefault() {
        return Equals(this, default(MapObjectParameter));
    }
}
