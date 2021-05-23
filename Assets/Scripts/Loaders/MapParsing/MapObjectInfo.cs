using UnityEngine;

public struct MapObjectInfo {
    public string Name;
    public Rect Rect;
    public float Rotation;
    public string SortingLayer;
    public Sprite Sprite;
    public MapObjectParameter[] Parameters;

    public override string ToString() {
        return $"\"{Name}\" {Rect} {Rotation}\nLayer: {SortingLayer}\nParams: {string.Join("; ", Parameters)}";
    }

    public static MapObjectInfo GetEmpty() {
        return new MapObjectInfo {
            Name = "",
            Rect = new Rect(),
            Parameters = new MapObjectParameter[] { }
        };
    }
}
