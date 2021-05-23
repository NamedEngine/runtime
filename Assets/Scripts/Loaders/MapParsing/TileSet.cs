using System;
using System.Collections.Generic;
using UnityEngine;

public class TileSet {
    readonly GraphicsConverter _converter;
    readonly Texture2D _texture;
    readonly int _firstgid;
    public float TileWidth { get; }
    public float TileHeight { get; }
    public Vector2 TileSize => new Vector2(TileWidth, TileHeight);
    readonly int _columns;
    readonly int _rows;

    Dictionary<int, Rect[]> _colliders;

    public TileSet(GraphicsConverter converter, Texture2D texture, int firstgid, float tileWidth, float tileHeight, int columns, int rows,
        Dictionary<int, Rect[]> colliders = null) {
        _converter = converter;
        _texture = texture;
        _firstgid = firstgid;
        TileWidth = tileWidth;
        TileHeight = tileHeight;
        _columns = columns;
        _rows = rows;
        _colliders = colliders ?? new Dictionary<int, Rect[]>();
    }

    public bool InRange(int tile) {
        return tile >= _firstgid && tile < _firstgid + _rows * _columns;
    }

    public Sprite GetSprite(int tile) {
        if (!InRange(tile)) {
            throw new IndexOutOfRangeException();
        }

        var tileIndex = tile - _firstgid;
        
        var col = tileIndex % _columns;
        var row = _rows - 1 - tileIndex / _columns;
        
        var xPos = col * TileWidth;
        var yPos = row * TileHeight;
        var pos = new Vector2(xPos, yPos);
        var size = new Vector2(TileWidth, TileHeight);
        
        var rect = new Rect(pos, size);
        return Sprite.Create(_texture, rect, _converter.DefaultPivot, _converter.PixelsPerUnit);
    }

    public Rect[] GetCollider(int tile) {
        var tileIndex = tile - _firstgid;
        return _colliders.ContainsKey(tileIndex) ? _colliders[tileIndex] : new Rect[] { };
    }
}
