using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Unity.Mathematics;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

public class MapLoader : MonoBehaviour {
    [SerializeField] GameObject tilePrefab;
    [SerializeField] GameObject mapObject;
    [SerializeField] GraphicsConverter graphicsConverter;
    [SerializeField] SizePositionConverter sizePositionConverter;
    [SerializeField] FileLoader fileLoader;
    
    static string GetAttr(XElement element, string name) => element.Attribute(name)?.Value ?? "";
    static int GetIntAttr(XElement element, string name) => Convert.ToInt32(GetAttr(element, name).IfEmpty("0"));
    static float GetFloatAttr(XElement element, string name) => Convert.ToSingle(GetAttr(element, name).IfEmpty("0"), new ProjectFormatProvider());

    void ClearMap() {
        for (var i = 0; i < mapObject.transform.childCount; i++) {
            Destroy(mapObject.transform.GetChild(i).gameObject);
        }
    }

    public MapObjectInfo[] LoadMap(string mapPath) {
        ClearMap();
        
        // TODO: do something with path handling in this file

        var mapDocument = XDocument.Parse(fileLoader.LoadText(mapPath));
        var root = mapDocument.Root;
        Debug.Assert(root != null, nameof(root) + " != null");

        var tileSets = root.Descendants("tileset")
            .Select(d => new { path = GetAttr(d, "source"), gid = GetIntAttr(d, "firstgid") })
            .Select(item => new { fullPath = Path.Combine(Path.GetDirectoryName(mapPath), item.path), item.gid })
            .Select(item => LoadTileSet(item.fullPath, item.gid))
            .ToList();

        var layerObjectInfoLoaders =
            new Dictionary<string, Func<XElement, XElement, string, string, List<MapObjectInfo>>> {
                {"objectgroup", LoadObjectLayer},
                {"imagelayer", LoadImageLayer}
            };

        var objectInfos = new List<MapObjectInfo>();
        var sortingLayer = 0;
        foreach (var descendant in root.Descendants()) {
            var layerType = descendant.Name.ToString();
            if (layerType == "layer") {
                var layerObjects = LoadTileLayer(descendant, sortingLayer.ToString(), tileSets);
                sortingLayer++;
                
                var layerObject = new GameObject();
                layerObject.transform.SetParent(mapObject.transform);
                layerObjects.ForEach(o => o.transform.SetParent(layerObject.transform));
            } else if (layerObjectInfoLoaders.ContainsKey(layerType)) {
                var layerObjectInfos =
                    layerObjectInfoLoaders[layerType](descendant, root, sortingLayer.ToString(), mapPath);
                sortingLayer++;
                
                objectInfos.AddRange(layerObjectInfos);
            }
        }

        return objectInfos.ToArray();
    }

    List<GameObject> LoadTileLayer(XElement layer, string sortingLayer, List<TileSet> tileSets) {
        var data = layer.Descendants("data").First().Value;
        
        var tileNumbers = data.Split('\n')
            .Where(l => l.Length > 0)
            .Select(l => l.EndsWith(",") ? l.Substring(0, l.Length - 1) : l)
            .Select(l => l.Split(',').Select(s => Convert.ToInt32(s)).ToArray())
            .ToArray();

        var tileObjects = new List<GameObject>();
        var rows = tileNumbers.Length;
        for (var row = 0; row < rows; row++) {
            var columns = tileNumbers[row].Length;
            for (var column = 0; column < columns; column++) {
                var tileNumber = tileNumbers[row][column];
                var tileSet = tileSets.FirstOrDefault(t => t.InRange(tileNumber));
                if (tileSet == null) {
                    continue;
                }
                
                var sprite = tileSet.GetSprite(tileNumber);

                var xPos = column * tileSet.TileWidth;
                var yPos = row * tileSet.TileHeight;
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

    List<MapObjectInfo> LoadObjectLayer(XElement layer, XElement map, string sortingLayer, string mapPath) {

        Rect GetRect(XElement obj) => RectFromObject(obj, obj, "x", "y");

        return layer.Descendants("object")
            .Select(obj => (obj, GetRect(obj)))
            .Select(pair => new MapObjectInfo {
                Name = GetAttr(pair.obj, "name"),
                Rect = pair.Item2,
                SortingLayer = sortingLayer,
                Sprite = null,
                Parameters = ParametersFromObject(pair.obj)
            })
            .ToList();
    }

    List<MapObjectInfo> LoadImageLayer(XElement layer, XElement map, string sortingLayer, string mapPath) {
        var imageInfo = layer.Descendants("image").First();
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

    TileSet LoadTileSet(string tileSetPath, int firstgid) {
        var tileSetDocument = XDocument.Parse(fileLoader.LoadText(tileSetPath));

        var root = tileSetDocument.Root;
        Debug.Assert(root != null, nameof(root) + " != null");
        
        var tileWidth = GetFloatAttr(root, "tilewidth");
        var tileHeight = GetFloatAttr(root, "tileheight");
        var tileCount = GetIntAttr(root, "tilecount");
        var columns = GetIntAttr(root, "columns");
        var rows = tileCount / columns;

        var imageInfo = root.Descendants().First();
        var imagePathRaw = GetAttr(imageInfo, "source");
        var imagePath = Path.Combine(Path.GetDirectoryName(tileSetPath), imagePathRaw);
        var texture = graphicsConverter.PathToTexture(imagePath);

        var tileInfos = root.Descendants("tile").ToArray();
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

        return new TileSet(graphicsConverter, texture, firstgid, tileWidth, tileHeight, columns, rows, colliders);
    }
}
