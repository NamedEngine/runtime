using System.Linq;
using System.Xml.Linq;

namespace Rules.Parsing {
    public static class DrawIO {
        public static void CheckBlocks(string logicSource, string file) {
            var document = XDocument.Parse(logicSource);
            var root = document.Descendants("root").First();
            
            var objects = root
                .Elements("object")
                .Where(obj => !NodeTypeConverter.NodeTypesByNotation.ContainsKey(obj.Attribute("type")?.Value ?? ""))
                .ToArray();

            foreach (var obj in objects) {
                var message = "One of used blocks is not from NamedLanguage block collection";
                
                var label = obj.Attribute("label")?.Value;
                if (label != null) {
                    message += $"\nLabel: \"{label}\"";
                }
                
                throw new LogicParseException(file, message);
            }
        }
        
        public static void CheckArrows(string logicSource, string file) {
            var document = XDocument.Parse(logicSource);
            var root = document.Descendants("root").First();

            var arrows = root
                .Elements("mxCell")
                .Where(rel => rel.HasElements)
                .ToArray();

            foreach (var arrow in arrows) {
                var sourceId = arrow.Attribute("source");
                var targetId = arrow.Attribute("target");
                
                if (sourceId != null && targetId != null) {
                    continue;
                }

                if (sourceId == null && targetId == null) {
                        (string, string) GetPointCoords(string pointType) {
                            var sourcePoint = arrow
                                .Descendants("mxPoint")
                                .First(cell => cell.Attribute("as")?.Value == pointType);
                            var X = sourcePoint.Attribute("x").Value;
                            var Y = sourcePoint.Attribute("y").Value;
                            return (X, Y);
                        }

                        var (sourceX, sourceY) = GetPointCoords("sourcePoint");
                        var (targetX, targetY) = GetPointCoords("targetPoint");

                        throw new LogicParseException(file, "One of arrows is not connected to anything\n" +
                                                      $"Source point is ({sourceX};{sourceY})\n" +
                                                      $"Target point is ({targetX};{targetY})");
                }
                
                var connectedId = sourceId ?? targetId;
                var connectedObject = root.Elements("object")
                    .First(obj => obj.Attribute("id").Value == connectedId.Value);
                
                var message = "One of arrows is connected to only one object;\nObject ";
                    
                var typeString = connectedObject.Attribute("type")?.Value;
                if (typeString != null && NodeTypeConverter.NodeTypesByNotation.ContainsKey(typeString)) {
                    var type = NodeTypeConverter.NodeTypesByNotation[typeString];
                    message += $"type: {type}, ";
                }
                
                var label = connectedObject.Attribute("label").Value;
                message += $"label: \"{label}\"";
                
                throw new LogicParseException(file, message);
            }
        }
    }
}