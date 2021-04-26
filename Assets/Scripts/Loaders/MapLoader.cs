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
    [SerializeField] FileLoader fileLoader;
    
    static Func<XElement, string, string> getAttr = (element, name) => element.Attribute(name).Value;
    static Func<XElement, string, int> getIntAttr = (element, name) => Convert.ToInt32(getAttr(element, name));
    static Func<XElement, string, float> getFloatAttr = (element, name) => Convert.ToSingle(getAttr(element, name));

    void Start() {
        // TODO
        LoadMap("Resources\\Maps\\testmap.tmx");
    }

    void ClearMap() {
        for (int i = 0; i < mapObject.transform.childCount; i++) {
            Destroy(mapObject.transform.GetChild(i).gameObject);
        }
    }

    public void LoadMap(string mapPath) {
        ClearMap();

        var mapDocument = XDocument.Parse(fileLoader.LoadText(mapPath));
        var root = mapDocument.Root;
        
        var tilesets = root.Descendants("tileset")
            .Select(d => new { path = getAttr(d, "source"), gid = getIntAttr(d, "firstgid") })
            .Select(item => new { fullPath = Path.Combine(Path.GetDirectoryName(mapPath), item.path), item.gid })
            .Select(item => LoadTileSet(item.fullPath, item.gid))
            .ToList();

        var layerTypeLoaders = new Dictionary<string, Func<XElement, XElement, int, string, List<TileSet>, List<GameObject>>>();
        layerTypeLoaders.Add("layer", LoadTileLayer);
        layerTypeLoaders.Add("objectgroup", LoadObjectLayer);
        layerTypeLoaders.Add("imagelayer", LoadImageLayer);

        int sortingLayer = 0;
        foreach (var descendant in root.Descendants()) {
            var layerType = descendant.Name.ToString();
            if (layerTypeLoaders.ContainsKey(layerType)) {
                var layerObject = new GameObject();
                layerObject.transform.SetParent(mapObject.transform);
                layerTypeLoaders[layerType](descendant, root, sortingLayer++, mapPath, tilesets).ForEach(o => o.transform.SetParent(layerObject.transform));
            }
        }
    }

    List<GameObject> LoadTileLayer(XElement layer, XElement map, int sortingLayer, string mapPath, List<TileSet> tileSets) {
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

                var trueRow = rows - 1 - row;
                var xPos = column * tileset.TileWidth / graphicsConverter.PixelsPerUnit;
                var yPos = trueRow * tileset.TileHeight / graphicsConverter.PixelsPerUnit;
                var pos = new Vector2(xPos, yPos);

                var tile = Instantiate(staticObjectPrefab, pos, quaternion.identity);
                var sr = tile.GetComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingLayerName = sortingLayer.ToString();

                var colliders = tileset.GetCollider(tileNumber);
                foreach (var rect in colliders) {
                    var c = tile.AddComponent<BoxCollider2D>();
                    c.usedByComposite = true;
                    c.offset = rect.position / graphicsConverter.PixelsPerUnit;
                    c.size = rect.size / graphicsConverter.PixelsPerUnit;
                }
                
                tileObjects.Add(tile);
            }
        }

        return tileObjects;
    }

    List<GameObject> LoadObjectLayer(XElement layer, XElement map, int sortingLayer, string mapPath, List<TileSet> tileSets) {
        var mapHeight = getIntAttr(map, "height") * getFloatAttr(map, "tileheight");
        
        Func<XElement, Rect> posSizeFromObject = obj => {
            var width = getFloatAttr(obj, "width");
            var height = getFloatAttr(obj, "height");
            var xPos = getFloatAttr(obj, "x") /*+ width / 2f*/;
            var rawYPos = getFloatAttr(obj, "y");
            
            var yPos =  mapHeight - rawYPos - height /*/ 2f*/;

            var pos = new Vector2(xPos, yPos) / graphicsConverter.PixelsPerUnit;
            var size = new Vector2(width, height);

            return new Rect(pos, size);
        };
        
        var objectInfos = layer.Descendants("object").ToArray();
        var rects = objectInfos.Select(posSizeFromObject).ToArray();
        // TEST VERSION OF PROPERTIES TODO REMAKE

        var objects = rects
            .Select(rect => Instantiate(kinematicObjectPrefab, rect.position, Quaternion.identity))
            .ToList();
        return objects;
    }

    List<GameObject> LoadImageLayer(XElement layer, XElement map, int sortingLayer, string mapPath, List<TileSet> tileSets) {
        var imageInfo = layer.Descendants("image").First();
        var rawImagePath = getAttr(imageInfo, "source");
        var imagePath = Path.Combine(Path.GetDirectoryName(mapPath), rawImagePath);
        var sprite = graphicsConverter.PathToSprite(imagePath);
        
        var width = getFloatAttr(imageInfo, "width");
        var height = getFloatAttr(imageInfo, "height");
        
        var xPos = getFloatAttr(layer, "offsetx");
        var rawYPos = getFloatAttr(layer, "offsety");

        var mapHeight = getIntAttr(map, "height") * getFloatAttr(map, "tileheight");
        var yPos =  mapHeight - rawYPos - height;

        var pos = new Vector2(xPos, yPos) / graphicsConverter.PixelsPerUnit;
        var image = Instantiate(kinematicObjectPrefab, pos, quaternion.identity);
        var sr = image.GetComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = sortingLayer.ToString();
        
        return new List<GameObject> { image };
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

            var pos = new Vector2(xPos, yPos);
            var size = new Vector2(width, height);

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
