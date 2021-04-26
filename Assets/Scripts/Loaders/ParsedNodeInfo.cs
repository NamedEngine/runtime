using System;

[Serializable]
public struct ParsedNodeInfo {
    public string id;
    public string type;
    public string name;
    public string parent;
    public string[] children;
    public string[] prev;
    public string[] next;
    
    public override string ToString() {
        return
            $"Id: {id}\nType: {type};\nName: {name}\nParent: {parent}\n" +
            $"Children: {String.Join(", ", children)}\n" +
            $"Prev: {String.Join(", ", prev)}\n" +
            $"Next: {String.Join(", ", next)}"; 
    }
}