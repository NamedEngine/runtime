using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Unity.Mathematics;
using UnityEngine;

public class MapLoader : MonoBehaviour {
    [SerializeField] GameObject staticObjectPrefab;  // TODO: most likely this would be assigned by logic engine
    [SerializeField] GameObject kinematicObjectPrefab;
    [SerializeField] GameObject mapObject;
    [SerializeField] GraphicsConverter graphicsConverter;
    [SerializeField] SizePositionConverter sizePositionConverter;
    [SerializeField] FileLoader fileLoader;
    
    static Func<XElement, string, string> getAttr = (element, name) => element.Attribute(name)?.Value ?? "";
    static Func<XElement, string, int> getIntAttr = (element, name) => Convert.ToInt32(getAttr(element, name).IfEmpty("0"));
    static Func<XElement, string, float> getFloatAttr = (element, name) => Convert.ToSingle(getAttr(element, name).IfEmpty("0"));

    void ClearMap() {
        for (int i = 0; i < mapObject.transform.childCount; i++) {
            Destroy(mapObject.transform.GetChild(i).gameObject);
        }
    }

    public MapObjectInfo[] LoadMap(string mapPath) {
        ClearMap();
        
        // TODO: do something with path handling in this file

        var mapDocument = XDocument.Parse(fileLoader.LoadText(mapPath));
        var root = mapDocument.Root;
        
        var tilesets = root.Descendants("tileset")
            .Select(d => new { path = getAttr(d, "source"), gid = getIntAttr(d, "firstgid") })
            .Select(item => new { fullPath = Path.Combine(Path.GetDirectoryName(mapPath), item.path), item.gid })
            .Select(item => LoadTileSet(item.fullPath, item.gid))
            .ToList();

        var layerObjectInfoLoaders = new Dictionary<string, Func<XElement, XElement, int, string, List<MapObjectInfo>>>();
        layerObjectInfoLoaders.Add("objectgroup", LoadObjectLayer);
        layerObjectInfoLoaders.Add("imagelayer", LoadImageLayer);

        var objectInfos = new List<MapObjectInfo>();
        int sortingLayer = 0;
        foreach (var descendant in root.Descendants()) {
            var layerType = descendant.Name.ToString();

            List<GameObject> layerObjects;
            if (layerType == "layer") {
                layerObjects = LoadTileLayer(descendant, sortingLayer++, tilesets);
            } else if (layerObjectInfoLoaders.ContainsKey(layerType)) {
                var layerObjectInfos = layerObjectInfoLoaders[layerType](descendant, root, sortingLayer++, mapPath);
                objectInfos.AddRange(layerObjectInfos);
                
                layerObjects = layerObjectInfos.Select(info => info.GameObject).ToList();
            } else {
                continue;
            }
            
            var layerObject = new GameObject();
            layerObject.transform.SetParent(mapObject.transform);
            layerObjects.ForEach(o => o.transform.SetParent(layerObject.transform));
        }

        return objectInfos.ToArray();
    }

    List<GameObject> LoadTileLayer(XElement layer, int sortingLayer, List<TileSet> tileSets) {
        var data = layer.Descendants("data").First().Value;
        
        var tileNumbers = data.Split('\n')
            .Where(l => l.Length > 0)
            .Select(l => l.EndsWith(",") ? l.Substring(0, l.Length - 1) : l)
            .Select(l => l.Split(',').Select(s => Convert.ToInt32(s)).ToArray())
            .ToArray();

        var tileObjects = new List<GameObject>();
        var rows = tileNumbers.Length;
        for (int row = 0; row < rows; row++) {
            var columns = tileNumbers[row].Length;
            for (int column = 0; column < columns; column++) {
                var tileNumber = tileNumbers[row][column];
                var tileset = tileSets.FirstOrDefault(t => t.InRange(tileNumber));
                if (tileset == null) {
                    continue;
                }
                
                var sprite = tileset.GetSprite(tileNumber);

                // var trueRow = rows - 1 - row;
                var xPos = column * tileset.TileWidth;
                var yPos = row * tileset.TileHeight;
                var pos = sizePositionConverter.PositionM2U(new Vector2(xPos, yPos), tileset.TileHeight);

                var tile = Instantiate(staticObjectPrefab, pos, quaternion.identity);
                var sr = tile.GetComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingLayerName = sortingLayer.ToString();

                var colliders = tileset.GetCollider(tileNumber);
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

    List<MapObjectInfo> LoadObjectLayer(XElement layer, XElement map, int sortingLayer, string mapPath) {

        Rect GetRect(XElement obj) => RectFromObject(obj, obj, "x", "y");

        return layer.Descendants("object")
            .Select(obj => (obj, GetRect(obj)))
            .Select(pair => new MapObjectInfo {
                GameObject = Instantiate(kinematicObjectPrefab, pair.Item2.position, Quaternion.identity),
                Name = getAttr(pair.obj, "name"),
                Rect = pair.Item2,
                Parameters = ParametersFromObject(pair.obj)
            })
            .ToList();
    }

    List<MapObjectInfo> LoadImageLayer(XElement layer, XElement map, int sortingLayer, string mapPath) {
        var imageInfo = layer.Descendants("image").First();
        var rawImagePath = getAttr(imageInfo, "source");
        var imagePath = Path.Combine(Path.GetDirectoryName(mapPath), rawImagePath);
        var sprite = graphicsConverter.PathToSprite(imagePath);

        var rect = RectFromObject(imageInfo, layer, "offsetx", "offsety");
        var image = Instantiate(kinematicObjectPrefab, rect.position, quaternion.identity);
        var sr = image.GetComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = sortingLayer.ToString();
        
        return new List<MapObjectInfo> { new MapObjectInfo {
            GameObject = image,
            Name = getAttr(layer, "name"),
            Rect = rect,
            Parameters = ParametersFromObject(layer)
        } };
    }

    Rect RectFromObject(XElement sizeObj, XElement posObj, string xPosName, string yPosName) {
        var width = getFloatAttr(sizeObj, "width");
        var height = getFloatAttr(sizeObj, "height");
        var xPos = getFloatAttr(posObj, xPosName);
        var yPos = getFloatAttr(posObj, yPosName);

        var pos = sizePositionConverter.PositionM2U(new Vector2(xPos, yPos), height);
        var size = new Vector2(width, height) * sizePositionConverter.SizeM2U;

        return new Rect(pos, size);
    }

    MapObjectParameter[] ParametersFromObject(XElement obj) {
        MapObjectParameter ParameterFromProperty(XElement prop) {
            var typeString = getAttr(prop, "type").IfEmpty("string");
            var parsed = Enum.TryParse(typeString.StartWithUpper(), out ValueType type);
            if (!parsed) {
                var objNaming = getAttr(obj, "name") != ""
                    ? "object named \"" + getAttr(obj, "name") + "\""
                    : "unnamed object";
                var id = getAttr(obj, "id");
                throw new ArgumentException("Found unsupported type \"" + typeString + "\" in the " + objNaming +
                                            " with id " + id); // TODO: move to LANG RULES
            }

            var paramName = getAttr(prop, "name");
            var value = getAttr(prop, "value");

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
        
        var tileWidth = getFloatAttr(root, "tilewidth");
        var tileHeight = getFloatAttr(root, "tileheight");
        var tileCount = getIntAttr(root, "tilecount");
        var columns = getIntAttr(root, "columns");
        var rows = tileCount / columns;

        var imageInfo = root.Descendants().First();
        var imagePathRaw = getAttr(imageInfo, "source");
        var imagePath = Path.Combine(Path.GetDirectoryName(tileSetPath), imagePathRaw);
        var texture = graphicsConverter.PathToTexture(imagePath);

        var tileInfos = root.Descendants("tile").ToArray();
        var tileIds = tileInfos.Select(i => getIntAttr(i, "id")).ToArray();

        Func<XElement, Rect> posSizeFromObject = obj => {
            var width = getFloatAttr(obj, "width");
            var height = getFloatAttr(obj, "height");
            var xPos = getFloatAttr(obj, "x") + width / 2f;
            var rawYPos = getFloatAttr(obj, "y");

            var yPos = tileHeight - rawYPos - height / 2f;

            var pos = new Vector2(xPos, yPos) * sizePositionConverter.SizeM2U;
            var size = new Vector2(width, height) * sizePositionConverter.SizeM2U;

            return new Rect(pos, size);
        };

        var tileColliders = tileInfos
            .Select(i => i.Descendants("objectgroup").First().Descendants().ToArray())
            .Select(objects => objects.Select(o => posSizeFromObject(o)).ToArray())
            .ToArray();

        var colliders = tileIds
            .Zip(tileColliders, (id, rects) => new {id, rects})
            .ToDictionary(item => item.id, item => item.rects);

        return new TileSet(texture, firstgid, tileWidth, tileHeight, columns, rows, colliders);
    }
}
