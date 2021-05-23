using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Unity.Mathematics;
using UnityEngine;
using static MapUtils;
using Debug = System.Diagnostics.Debug;

public class MapLoader : MonoBehaviour {
    [SerializeField] GameObject tilePrefab;
    [SerializeField] GraphicsConverter graphicsConverter;
    [SerializeField] SizePositionConverter sizePositionConverter;
    [SerializeField] FileLoader fileLoader;
    
    static string GetAttr(XElement element, string name) => element.Attribute(name)?.Value ?? "";
    static int GetIntAttr(XElement element, string name) => Convert.ToInt32(GetAttr(element, name).IfEmpty("0"));
    static float GetFloatAttr(XElement element, string name) => Convert.ToSingle(GetAttr(element, name).IfEmpty("0"), new ProjectFormatProvider());

    public MapObjectInfo[] LoadMap(string mapPath, GameObject mapObject) {
        var mapInfoSource = fileLoader.LoadText(mapPath);
        Rules.RuleChecker.CheckParsing<Rules.Parsing.Tiled, string>(mapInfoSource, mapPath);

        // TODO: do something with path handling in this file

        var mapDocument = XDocument.Parse(mapInfoSource);
        var root = mapDocument.Root;
        Debug.Assert(root != null, nameof(root) + " != null");

        var mapInfo = new MapInfo {
            MapTileWidth = GetFloatAttr(root, "tilewidth"),
            MapTileHeight = GetFloatAttr(root, "tileheight"),
        };

        const string tileSetElementName = "tileset";
        var tileSets = root.Elements(tileSetElementName)
            .Select(element => GetTileSetInfo(element, mapPath))
            .Select(LoadTileSet)
            .ToArray();

        var layerObjectInfoLoaders =
            new Dictionary<string, Func<XElement, string, string, List<MapObjectInfo>>> {
                {"objectgroup", LoadObjectLayer},
                {"imagelayer", LoadImageLayer}
            };

        var objectInfos = new List<MapObjectInfo>();
        var sortingLayer = 0;
        var layers = root.Elements().Where(el => el.Name != tileSetElementName);
        foreach (var layer in layers) {
            var layerType = layer.Name.ToString();
            if (layerType == "layer") {
                var layerObjects = LoadTileLayer(layer, mapInfo, sortingLayer.ToString(), tileSets);
                sortingLayer++;
                
                var layerObject = new GameObject();
                layerObject.transform.SetParent(mapObject.transform);
                layerObjects.ForEach(o => o.transform.SetParent(layerObject.transform));
            } else {
                var layerObjectInfos =
                    layerObjectInfoLoaders[layerType](layer, sortingLayer.ToString(), mapPath);
                sortingLayer++;
                
                objectInfos.AddRange(layerObjectInfos);
            }
        }

        return objectInfos.ToArray();
    }

    List<GameObject> LoadTileLayer(XElement layer, MapInfo mapInfo, string sortingLayer, TileSet[] tileSets) {
        var data = layer.Elements("data").First();
        var chunks = data.Elements("chunk").ToList();
        if (chunks.Count == 0) {
            chunks.Add(data);
        }

        ChunkInfo GetChunkInfo(XElement chunk) {
            var tileNumbers = chunk.Value.Split('\n')
                .Where(l => l.Length > 0)
                .Select(l => l.EndsWith(",") ? l.Substring(0, l.Length - 1) : l)
                .Select(l => l.Split(',').Select(s => Convert.ToInt32(s)).ToArray())
                .ToArray();

            return new ChunkInfo {
                OffsetX = GetIntAttr(chunk, "x"),
                OffsetY = GetIntAttr(chunk, "y"),
                TileNumbers = tileNumbers
            };
        }

        return chunks.Select(GetChunkInfo)
            .Select(chunk => LoadTileChunk(chunk, mapInfo, sortingLayer, tileSets))
            .SelectMany(tileObjects => tileObjects)
            .ToList();
    }

    List<GameObject> LoadTileChunk(ChunkInfo chunkInfo, MapInfo mapInfo, string sortingLayer, TileSet[] tileSets) {
        var tileObjects = new List<GameObject>();
        var rows = chunkInfo.TileNumbers.Length;
        for (var row = 0; row < rows; row++) {
            var columns = chunkInfo.TileNumbers[row].Length;
            for (var column = 0; column < columns; column++) {
                var tileNumber = chunkInfo.TileNumbers[row][column];
                var tileSet = tileSets.FirstOrDefault(t => t.InRange(tileNumber));
                if (tileSet == null) {
                    continue;
                }

                var sprite = tileSet.GetSprite(tileNumber);

                var xPos = (column + chunkInfo.OffsetX) * mapInfo.MapTileWidth;
                var yPos = (row + chunkInfo.OffsetY) * mapInfo.MapTileHeight;
                var pos = sizePositionConverter.InitialPositionOnMapToUnity(new Vector2(xPos, yPos), tileSet.TileSize);

                var tile = Instantiate(tilePrefab, pos, quaternion.identity);
                var sr = tile.GetComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingLayerName = sortingLayer;

                var colliders = tileSet.GetCollider(tileNumber);
                foreach (var rect in colliders) {
                    var c = tile.AddComponent<BoxCollider2D>();
                    c.usedByComposite = true;
                    c.offset = rect.position;
                    c.size = rect.size;
                }

                tileObjects.Add(tile);
            }
        }

        return tileObjects;
    }

    List<MapObjectInfo> LoadObjectLayer(XElement layer, string sortingLayer, string mapPath) {
        var objectParsersByType = new Dictionary<string, Func<XElement, string, MapObjectInfo>> {
            {RectTypeName, ParseRect},
            {"point", ParseRect},
        };

        return layer.Elements("object")
            .Select(obj => new {obj, type = GetObjectType(obj, RectTypeName)})
            .Select(pair => objectParsersByType[pair.type](pair.obj, sortingLayer))
            .ToList();
    }

    List<MapObjectInfo> LoadImageLayer(XElement layer, string sortingLayer, string mapPath) {
        var imageInfo = layer.Elements("image").First();
        var rawImagePath = GetAttr(imageInfo, "source");
        var imagePath = Path.Combine(Path.GetDirectoryName(mapPath), rawImagePath);
        var sprite = graphicsConverter.PathToSprite(imagePath);
        var rect = RectFromObject(imageInfo, layer, "offsetx", "offsety");
        
        return new List<MapObjectInfo> { new MapObjectInfo {
            Name = GetAttr(layer, "name"),
            Rect = rect,
            SortingLayer = sortingLayer,
            Sprite = sprite,
            Parameters = ParametersFromObject(layer)
        } };
    }

    MapObjectInfo ParseRect(XElement obj, string sortingLayer) {
        Rect GetRect(XElement o) => RectFromObject(o, o, "x", "y");

        return new MapObjectInfo {
            Name = GetAttr(obj, "name"),
            Rect = GetRect(obj),
            SortingLayer = sortingLayer,
            Sprite = null,
            Parameters = ParametersFromObject(obj)
        };
    }

    Rect RectFromObject(XElement sizeObj, XElement posObj, string xPosName, string yPosName) {
        var width = GetFloatAttr(sizeObj, "width");
        var height = GetFloatAttr(sizeObj, "height");
        var xPos = GetFloatAttr(posObj, xPosName);
        var yPos = GetFloatAttr(posObj, yPosName);

        var mapSize = new Vector2(width, height);
        var size = mapSize * sizePositionConverter.SizeM2U;
        var pos = sizePositionConverter.InitialPositionOnMapToUnity(new Vector2(xPos, yPos), mapSize);

        return new Rect(pos, size);
    }

    MapObjectParameter[] ParametersFromObject(XElement obj) {
        MapObjectParameter ParameterFromProperty(XElement prop) {
            var typeString = GetAttr(prop, "type").IfEmpty("string");
            var parsed = Enum.TryParse(typeString.StartWithUpper(), out ValueType type);
            if (!parsed) {
                var objNaming = GetAttr(obj, "name") != ""
                    ? "object named \"" + GetAttr(obj, "name") + "\""
                    : "unnamed object";
                var id = GetAttr(obj, "id");
                throw new ArgumentException("Found unsupported type \"" + typeString + "\" in the " + objNaming +
                                            " with id " + id);  // TODO: to map rules
            }

            var paramName = GetAttr(prop, "name");
            var value = GetAttr(prop, "value");

            return new MapObjectParameter() {
                Name = paramName,
                Type = type,
                Value = value
            };
        }

        return obj.Descendants("property")
            .Select(ParameterFromProperty)
            .ToArray();
    }

    TileSetInfo GetTileSetInfo(XElement tileSetElement, string mapPath) {
        var firstGid = GetIntAttr(tileSetElement, "firstgid");

        const string sourceAttrName = "source";
        var hasSource = tileSetElement.Attributes().Any(attr => attr.Name == sourceAttrName);
        if (!hasSource) {
            return new TileSetInfo {
                TileSetElement = tileSetElement,
                FirstGid = firstGid,
                TileSetPath = mapPath
            };
        }
        
        var source = GetAttr(tileSetElement, sourceAttrName);
        var path = Path.Combine(Path.GetDirectoryName(mapPath), source);
        return new TileSetInfo {
            TileSetElement = XDocument.Parse(fileLoader.LoadText(path)).Root,
            FirstGid = firstGid,
            TileSetPath = path
        };
    }

    TileSet LoadTileSet(TileSetInfo tileSetInfo) {
        var tileWidth = GetFloatAttr(tileSetInfo.TileSetElement, "tilewidth");
        var tileHeight = GetFloatAttr(tileSetInfo.TileSetElement, "tileheight");
        var tileCount = GetIntAttr(tileSetInfo.TileSetElement, "tilecount");
        var columns = GetIntAttr(tileSetInfo.TileSetElement, "columns");
        var rows = tileCount / columns;

        var imageInfo = tileSetInfo.TileSetElement.Descendants().First();
        var imagePathRaw = GetAttr(imageInfo, "source");
        var imagePath = Path.Combine(Path.GetDirectoryName(tileSetInfo.TileSetPath), imagePathRaw);
        var texture = graphicsConverter.PathToTexture(imagePath);

        var tileInfos = tileSetInfo.TileSetElement.Descendants("tile").ToArray();
        var tileIds = tileInfos.Select(i => GetIntAttr(i, "id")).ToArray();

        var tileSize = new Vector2(tileWidth, tileHeight);

        Rect PosSizeFromObject(XElement obj) {
            var width = GetFloatAttr(obj, "width");
            var height = GetFloatAttr(obj, "height");
            var xPos = GetFloatAttr(obj, "x");
            var yPos = GetFloatAttr(obj, "y");

            var mapSize = new Vector2(width, height);
            var size = mapSize * sizePositionConverter.SizeM2U;
            var pos = sizePositionConverter.PositionM2U((mapSize - tileSize) / 2 + new Vector2(xPos, yPos));

            return new Rect(pos, size);
        }

        var tileColliders = tileInfos
            .Select(i => i.Descendants("objectgroup").First().Descendants().ToArray())
            .Select(objects => objects.Select(PosSizeFromObject).ToArray())
            .ToArray();

        var colliders = tileIds
            .Zip(tileColliders, (id, rects) => new {id, rects})
            .ToDictionary(item => item.id, item => item.rects);

        return new TileSet(graphicsConverter, texture, tileSetInfo.FirstGid, tileWidth, tileHeight, columns, rows, colliders);
    }
}
