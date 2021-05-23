using System;

[Serializable]
public struct ParsedNodeInfo {
    public string id;
    public NodeType type;
    public string name;
    public string parent;
    public string[] parameters;
    public string[] prev;
    public string[] next;
    
    public override string ToString() {
        return
            $"Id: {id}\nType: {type}\nName: {name}\nParent: {parent}\n" +
            $"Parameters: {String.Join(", ", parameters)}\n" +
            $"Prev: {String.Join(", ", prev)}\n" +
            $"Next: {String.Join(", ", next)}"; 
    }

    public string ToNameAndType() {
        return $"\"{name}\" block of \"{type}\" type";
    }
}