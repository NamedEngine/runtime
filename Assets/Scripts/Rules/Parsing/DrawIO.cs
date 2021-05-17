using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Rules.Parsing {
    public class DrawIO : IParsingChecker<string> {
        static bool IsNamedLanguageBlock(XElement block) {
            return NodeTypeConverter.NodeTypesByNotation.ContainsKey(block.Attribute("type")?.Value ?? "");
        }

        static XElement GetById(XElement root, XAttribute id) {
            return root.Elements()
                .First(obj => obj.Attribute("id").Value == id.Value);
        }
        
        static void CheckBlocks(string logicSource, string file) {
            var document = XDocument.Parse(logicSource);
            var root = document.Descendants("root").First();
            
            var objects = root
                .Elements("object")
                .Where(obj => !IsNamedLanguageBlock(obj))
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

        static void CheckArrows(string logicSource, string file) {
            var document = XDocument.Parse(logicSource);
            var root = document.Descendants("root").First();

            var arrows = root
                .Elements("mxCell")
                .Where(rel => rel.Attributes().Any(attr => attr.Name == "edge"))  // filtering anything but edges (arrows)
                .ToArray();

            foreach (var arrow in arrows) {
                var sourceId = arrow.Attribute("source");
                var targetId = arrow.Attribute("target");
                
                if (sourceId != null && targetId != null) {
                    var blocksToCheck = new [] {GetById(root, sourceId), GetById(root, targetId)};
                    foreach (var block in blocksToCheck) {
                        if (IsNamedLanguageBlock(block)) {
                            continue;
                        }

                        var connectedCommentMessage = "One of arrows is connected to a commentary block!" +
                                      $"\nText: {block.Attribute("value")?.Value}";
                        
                        throw new LogicParseException(file, connectedCommentMessage);
                    }
                    
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
                var connectedObject = GetById(root, connectedId);
                
                var message = "One of arrows is connected to only one object;\nObject ";

                if (IsNamedLanguageBlock(connectedObject)) {
                    var type = NodeTypeConverter.NodeTypesByNotation[connectedObject.Attribute("type").Value];
                    var label = connectedObject.Attribute("label").Value;
                    message += $"type: {type}, label: \"{label}\"";
                } else {
                    var text = connectedObject.Attribute("value").Value;;
                    message += $"text: {text}";
                }

                throw new LogicParseException(file, message);
            }
        }

        public List<Action<string, string>> GetCheckerMethods() {
            return new List<Action<string, string>> {
                CheckBlocks,
                CheckArrows
            };
        }
    }
}