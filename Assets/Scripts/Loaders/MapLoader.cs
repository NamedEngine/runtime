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
            MapPath = mapPath
        };

        const string tileSetElementName = "tileset";
        var tileSets = root.Elements(tileSetElementName)
            .Select(element => GetTileSetInfo(element, mapInfo.MapPath))
            .Select(LoadTileSet)
            .ToArray();

        var layerObjectInfoLoaders =
            new Dictionary<string, Func<XElement, string, List<MapObjectInfo>>> {
                {"objectgroup", (layer, sortLayer) => LoadObjectLayer(layer, mapInfo, sortLayer, tileSets)},
                {"imagelayer", (layer, sortLayer) => LoadImageLayer(layer, mapInfo, sortLayer, tileSets)}
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
                var layerObjectInfos = layerObjectInfoLoaders[layerType](layer, sortingLayer.ToString());
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

                var tileSize = tileSet.GetTileSize(tileNumber);
                var xPos = (column + chunkInfo.OffsetX) * mapInfo.MapTileWidth;
                var yPos = (row + chunkInfo.OffsetY) * mapInfo.MapTileHeight;
                var yAlignedPos = yPos + mapInfo.MapTileHeight - tileSize.y;
                var pos = sizePositionConverter.InitialPositionOnMapToUnity(new Vector2(xPos, yAlignedPos), tileSize);

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

    List<MapObjectInfo> LoadObjectLayer(XElement layer, MapInfo mapInfo, string sortingLayer, TileSet[] tileSets) {
        var objectParsersByType = new Dictionary<string, Func<XElement, string, MapObjectInfo>> {
            {RectTypeName, (element, sortLayer) => ParseRect(element, sortLayer, tileSets)},
            {"point", (element, sortLayer) => ParseRect(element, sortLayer, tileSets)},
        };

        return layer.Elements("object")
            .Select(obj => new {obj, type = GetObjectType(obj, RectTypeName)})
            .Select(pair => objectParsersByType[pair.type](pair.obj, sortingLayer))
            .ToList();
    }

    List<MapObjectInfo> LoadImageLayer(XElement layer, MapInfo mapInfo, string sortingLayer, TileSet[] tileSets) {
        var imageInfo = layer.Elements("image").First();
        var rawImagePath = GetAttr(imageInfo, "source");
        var imagePath = Path.Combine(Path.GetDirectoryName(mapInfo.MapPath), rawImagePath);
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

    MapObjectInfo ParseRect(XElement obj, string sortingLayer, TileSet[] tileSets) {
        Rect GetRect(XElement o, bool isFromTile) => RectFromObject(o, o, "x", "y", isFromTile);

        var gid = GetIntAttr(obj, "gid");
        var tileSet = tileSets.FirstOrDefault(t => t.InRange(gid));

        return new MapObjectInfo {
            Name = GetAttr(obj, "name"),
            Rect = GetRect(obj, tileSet != null),
            SortingLayer = sortingLayer,
            Sprite = tileSet?.GetSprite(gid),
            Parameters = ParametersFromObject(obj)
        };
    }

    Rect RectFromObject(XElement sizeObj, XElement posObj, string xPosName, string yPosName, bool isFromTile = false) {
        var width = GetFloatAttr(sizeObj, "width");
        var height = GetFloatAttr(sizeObj, "height");
        var xPos = GetFloatAttr(posObj, xPosName);
        var yPos = GetFloatAttr(posObj, yPosName);

        var mapSize = new Vector2(width, height);
        var size = mapSize * sizePositionConverter.SizeM2U;
        var mapPos = new Vector2(xPos, yPos);
        if (isFromTile) {
            mapPos.y -= mapSize.y;
        }
        var pos = sizePositionConverter.InitialPositionOnMapToUnity(mapPos, mapSize);

        return new Rect(pos, size);
    }

    MapObjectParameter[] ParametersFromObject(XElement obj) {
        MapObjectParameter ParameterFromProperty(XElement prop) {
            var typeString = GetAttr(prop, "type").IfEmpty("string");
            Enum.TryParse(typeString.StartWithUpper(), out ValueType type);

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
        if (!tileSetInfo.TileSetElement.Elements("image").Any()) {
            return LoadMultiImageTileSet(tileSetInfo);
        }

        return LoadOneImageTileSet(tileSetInfo);
    }

    OneImageTileSet LoadOneImageTileSet(TileSetInfo tileSetInfo) {
        var tileWidth = GetFloatAttr(tileSetInfo.TileSetElement, "tilewidth");
        var tileHeight = GetFloatAttr(tileSetInfo.TileSetElement, "tileheight");
        var tileCount = GetIntAttr(tileSetInfo.TileSetElement, "tilecount");
        var columns = GetIntAttr(tileSetInfo.TileSetElement, "columns");
        var rows = tileCount / columns;
        var margin = GetFloatAttr(tileSetInfo.TileSetElement, "margin");
        var spacing = GetFloatAttr(tileSetInfo.TileSetElement, "spacing");

        var imageInfo = tileSetInfo.TileSetElement.Descendants().First();
        var imagePathRaw = GetAttr(imageInfo, "source");
        var imagePath = Path.Combine(Path.GetDirectoryName(tileSetInfo.TileSetPath), imagePathRaw);
        var texture = graphicsConverter.PathToTexture(imagePath);

        var tileInfos = tileSetInfo.TileSetElement.Elements("tile").ToArray();
        var tileIds = tileInfos.Select(i => GetIntAttr(i, "id")).ToArray();

        var tileSize = new Vector2(tileWidth, tileHeight);

        var tileColliders = tileInfos
            .Select(i => TileElementToColliders(i, tileSize))
            .ToArray();

        var colliders = tileIds
            .Zip(tileColliders, (id, rects) => new {id, rects})
            .ToDictionary(item => item.id, item => item.rects);

        return new OneImageTileSet(graphicsConverter, texture, tileSetInfo.FirstGid, tileWidth, tileHeight, columns, rows, 
            margin, spacing, colliders);
    }

    Rect[] TileElementToColliders(XElement tile, Vector2 tileSize) {
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

        return tile.Element("objectgroup")?
            .Elements()
            .Select(PosSizeFromObject)
            .ToArray()
               ?? new Rect[] { };
    }

    MultiImageTileSet LoadMultiImageTileSet(TileSetInfo tileSetInfo) {
        KeyValuePair<int, MultiImageTileSet.TileInfo> TileElementToTileInfo(XElement tile) {
            var id = GetIntAttr(tile, "id");

            var imageElement = tile.Element("image");

            var width = GetFloatAttr(imageElement, "width");
            var height = GetFloatAttr(imageElement, "height");
            var size = new Vector2(width, height);

            var source = GetAttr(imageElement, "source");
            var spritePath = Path.Combine(Path.GetDirectoryName(tileSetInfo.TileSetPath), source);
            var sprite = graphicsConverter.PathToSprite(spritePath);

            var colliders = TileElementToColliders(tile, size);

            return new KeyValuePair<int, MultiImageTileSet.TileInfo>(id, new MultiImageTileSet.TileInfo {
                Sprite = sprite,
                Size = size,
                Colliders = colliders
            });
        }

        var tileInfos = tileSetInfo.TileSetElement
            .Elements("tile")
            .Select(TileElementToTileInfo)
            .ToDictionary();

        return new MultiImageTileSet(tileInfos, tileSetInfo.FirstGid);
    }
}
