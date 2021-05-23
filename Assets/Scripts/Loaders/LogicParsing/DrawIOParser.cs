using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

public class DrawIOParser : ILogicParser<string> {
    readonly IdGenerator _idGenerator;

    public DrawIOParser(IdGenerator idGenerator) {
        _idGenerator = idGenerator;
    }
    static string GetAttr(XElement element, string name) => element.Attribute(name)?.Value;

    static KeyValuePair<string, ParsedNodeInfo> ObjectToInfoPair(XElement obj) {
        string GetObjAttr(string name) => GetAttr(obj, name);

        var info = new ParsedNodeInfo {
            id = GetObjAttr("id"),
            type = NodeTypeConverter.NodeTypesByNotation[GetObjAttr("type")],
            name = GetObjAttr("label"),
            parameters = new string[]{},
            prev = new string[]{},
            next = new string[]{}
        };

        var childObj = obj.Elements("mxCell").First();
        var parent = GetAttr(childObj, "parent");
        info.parent = parent == "1" ? null : parent;

        return new KeyValuePair<string,ParsedNodeInfo>(info.id, info);
    }

    static KeyValuePair<string, string> RelationToPair(XElement relation) {
        string GetRelAttr(string name) => GetAttr(relation, name);
        
        return new KeyValuePair<string, string>(GetRelAttr("source"), GetRelAttr("target"));
    }
        
    public Dictionary<string, ParsedNodeInfo> Parse(string logicSource) {
        var document = XDocument.Parse(logicSource);
        var root = document.Descendants("root").First();
        
        // type, name, parent
        var infoDict = root
            .Elements("object")
            .Select(ObjectToInfoPair)
            .ToDictionary();

        // add children
        var childrenDict = infoDict
            .Where(pair => pair.Value.parent != null)
            .GroupBy(pair => pair.Value.parent)
            .Select(gr => new KeyValuePair<string, string[]>(gr.Key,
                gr.Select(pair => pair.Key).ToArray()
            ))
            .ToDictionary();
        foreach (var parent in childrenDict.Keys) {
            var updatedInfo = infoDict[parent];
            updatedInfo.parameters = childrenDict[parent];
            infoDict[parent] = updatedInfo;
        }

        var relArray = root
            .Elements("mxCell")
            .Where(rel => rel.Attributes().Any(attr => attr.Name == "edge"))  // filtering anything but edges (arrows)
            .Select(RelationToPair)
            .ToArray();
        
        // add next
        var nextDict = relArray
            .GroupBy(pair => pair.Key)
            .Select(gr => new KeyValuePair<string, string[]>(gr.Key,
                gr.Select(pair => pair.Value).ToArray()
            ))
            .ToDictionary();
        foreach (var prev in nextDict.Keys) {
            var updatedInfo = infoDict[prev];
            updatedInfo.next = nextDict[prev];
            infoDict[prev] = updatedInfo;
        }
        
        // add prev
        var prevDict = relArray
            .GroupBy(pair => pair.Value)
            .Select(gr => new KeyValuePair<string, string[]>(gr.Key,
                gr.Select(pair => pair.Key).ToArray()
            ))
            .ToDictionary();
        foreach (var next in prevDict.Keys) {
            var updatedInfo = infoDict[next];
            updatedInfo.prev = prevDict[next];
            infoDict[next] = updatedInfo;
        }

        var oldToNewIds = new Dictionary<string, string>();

        string OldToNewId(string oldId) {
            if (!oldToNewIds.ContainsKey(oldId)) {
                oldToNewIds[oldId] = _idGenerator.NewId();
            }
            
            return oldToNewIds[oldId];
        }

        return infoDict.ToDictionary(pair => OldToNewId(pair.Key), pair => {
            var info = pair.Value;
            return new ParsedNodeInfo {
                id = OldToNewId(info.id),
                type = info.type,
                name = info.name,
                parent = info.parent == null ? null : OldToNewId(info.parent),
                parameters = info.parameters.Select(OldToNewId).ToArray(),
                prev = info.prev.Select(OldToNewId).ToArray(),
                next = info.next.Select(OldToNewId).ToArray(),
            };
        });
    }
}
