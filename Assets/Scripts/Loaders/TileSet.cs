using System;
using System.Collections.Generic;
using UnityEngine;

public class TileSet {
    readonly Texture2D _texture;
    readonly int _firstgid;
    float _tileWidth;
    public float TileWidth => _tileWidth;
    float _tileHeight;
    public float TileHeight => _tileHeight;
    int _columns;
    int _rows;

    Dictionary<int, Rect[]> _colliders;

    public TileSet(Texture2D texture, int firstgid, float tileWidth, float tileHeight, int columns, int rows, Dictionary<int, Rect[]> colliders = null) {
        _texture = texture;
        _firstgid = firstgid;
        _tileWidth = tileWidth;
        _tileHeight = tileHeight;
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
        
        var xPos = col * _tileWidth;
        var yPos = row * _tileHeight;
        var pos = new Vector2(xPos, yPos);
        var size = new Vector2(_tileWidth, _tileHeight);
        
        var rect = new Rect(pos, size);
        return Sprite.Create(_texture, rect, Vector2.zero);
    }

    public Rect[] GetCollider(int tile) {
        var tileIndex = tile - _firstgid;
        return _colliders.ContainsKey(tileIndex) ? _colliders[tileIndex] : new Rect[] { };
    }
}