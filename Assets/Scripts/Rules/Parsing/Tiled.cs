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
                .Select(obj => MapUtils.GetObjectType(obj, MapUtils.RectTypeName))
                .Where(objType => !supportedTypes.Contains(objType));

            foreach (var objType in unsupportedTypes) {
                var message = $"Object type \"{objType}\" is not supported" +
                              "\nThe only supported types are: " + string.Join(", ", supportedTypes);

                throw new MapParseException(file, message);
            }
        }

        public List<Action<string, string>> GetCheckerMethods() {
            return new List<Action<string, string>> {
                CheckMalformed,
                CheckSupportedOrientation,
                CheckSupportedEncoding,
                CheckSupportedObjects,
            };
        }
    }
}
