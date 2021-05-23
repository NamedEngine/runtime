using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Rules.Parsing {
    public class Tiled : IParsingChecker<string> {
        static void CheckMalformed(string mapInfoSource, string file) {
            var malformedException = new MapParseException(file, "This map is malformed");

            XDocument document;
            try {
                document = XDocument.Parse(mapInfoSource);
            }
            catch (XmlException) {
                throw malformedException;
            }

            var root = document.Root;
            if (root == null || root.Name != "map") {
                throw malformedException;
            }

            var mandatoryAttributes = new[] {
                "orientation",
                "infinite",
                "width",
                "height",
                "tilewidth",
                "tileheight"
            };

            var noMandatoryAttribute = mandatoryAttributes
                .Select(attr => !root.Attributes(attr).Any())
                .Any(doesNotHave => doesNotHave);

            if (noMandatoryAttribute) {
                throw malformedException;
            }
        }

        static void CheckSupportedOrientation(string mapInfoSource, string file) {
            var document = XDocument.Parse(mapInfoSource);
            var root = document.Root;
            Debug.Assert(root != null, nameof(root) + " != null");
            
            var orientation = root.Attributes("orientation").First().Value;
            var supportedOrientations = new[] {
                "orthogonal"
            };

            if (supportedOrientations.Contains(orientation)) {
                return;
            }

            var message = $"Orientation \"{orientation}\" is not supported" +
                          "\nThe only supported orientations are: " + string.Join(", ", supportedOrientations);

            throw new MapParseException(file, message);
        }

        static void CheckSupportedEncoding(string mapInfoSource, string file) {
            var document = XDocument.Parse(mapInfoSource);
            var root = document.Root;
            Debug.Assert(root != null, nameof(root) + " != null");

            var supportedEncodings = new[] {
                "csv"
            };

            var encodings = root.Elements("layer")
                .Select(layer => layer.Elements("data").First().Attributes("encoding").First().Value);

            foreach (var encoding in encodings) {
                if (supportedEncodings.Contains(encoding)) {
                    continue;
                }

                var message = $"Encoding \"{encoding}\" is not supported" +
                              "\nThe only supported encodings are: " + string.Join(", ", supportedEncodings);

                throw new MapParseException(file, message);
            }
        }

        static string GetObjectNaming(XElement obj) {
            var name = obj.Attribute("name")?.Value;
            return name != null ? $"object named \"{name}\"" : "unnamed object";
        }

        static void CheckSupportedObjects(string mapInfoSource, string file) {
            var document = XDocument.Parse(mapInfoSource);
            var root = document.Root;
            Debug.Assert(root != null, nameof(root) + " != null");

            
            var supportedTypes = new[] {
                MapUtils.RectTypeName,
                "point"
            };

            var unsupportedTypes = root.Elements("objectgroup")
                .Select(objectGroup => objectGroup.Elements("object"))
                .SelectMany(obj => obj)
                .Select(obj => new { obj, type = MapUtils.GetObjectType(obj, MapUtils.RectTypeName) })
                .Where(pair => !supportedTypes.Contains(pair.type));

            foreach (var pair in unsupportedTypes) {
                var objNaming = GetObjectNaming(pair.obj);

                var message = $"Object type \"{pair.type}\" of an {objNaming} is not supported" +
                              "\nThe only supported types are: " + string.Join(", ", supportedTypes);

                throw new MapParseException(file, message);
            }
        }

        static void CheckSupportedPropertyTypes(string mapInfoSource, string file) {
            var document = XDocument.Parse(mapInfoSource);
            var root = document.Root;
            Debug.Assert(root != null, nameof(root) + " != null");

            const string propertiesElementName = "properties";
            var unsupportedPropertyTypes = root.Elements("objectgroup")
                .Select(objectGroup => objectGroup.Elements("object"))
                .SelectMany(obj => obj)
                .Where(obj => obj.Elements(propertiesElementName).Any())
                .SelectMany(obj => obj.Element(propertiesElementName)
                    .Elements("property")
                    .Select(prop => new { obj, prop }))
                .Select(pair => new {
                    pair.obj,
                    pair.prop,
                    type = (pair.prop.Attribute("type")?.Value ?? "").IfEmpty("string").StartWithUpper()
                })
                .Where(triple => !Enum.TryParse(triple.type, out ValueType _));

            foreach (var triple in unsupportedPropertyTypes) {
                var objNaming = GetObjectNaming(triple.obj);
                var propName = triple.prop.Attribute("name")?.Value;

                var message = $"Property \"{propName}\" of type \"{triple.type}\"" +
                              $"\n of an {objNaming} is not supported" +
                              "\nThe only supported types are: " + string.Join(", ", ValueTypeConverter.ValueTypes);

                throw new MapParseException(file, message);
            }
        }

        public List<Action<string, string>> GetCheckerMethods() {
            return new List<Action<string, string>> {
                CheckMalformed,
                CheckSupportedOrientation,
                CheckSupportedEncoding,
                CheckSupportedObjects,
                CheckSupportedPropertyTypes,
            };
        }
    }
}
