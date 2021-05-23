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
