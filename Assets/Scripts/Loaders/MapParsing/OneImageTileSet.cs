using System;
using System.Collections.Generic;
using UnityEngine;

public class OneImageTileSet : TileSet{
    readonly GraphicsConverter _converter;
    readonly Texture2D _texture;
    readonly float _tileWidth;
    readonly float _tileHeight;
    readonly int _columns;
    readonly int _rows;
    readonly float _margin;
    readonly float _spacing;
    readonly Dictionary<int, Rect[]> _colliders;

    public OneImageTileSet(GraphicsConverter converter, Texture2D texture, int firstGid, float tileWidth, float tileHeight,
        int columns, int rows, float margin, float spacing, Dictionary<int, Rect[]> colliders) : base(firstGid){
        _converter = converter;
        _texture = texture;
        _tileWidth = tileWidth;
        _tileHeight = tileHeight;
        _columns = columns;
        _rows = rows;
        _margin = margin;
        _spacing = spacing;
        _colliders = colliders;
    }

    protected override int GetTileCount() {
        return _rows * _columns;
    }

    public override Sprite GetSprite(int tile) {
        if (!InRange(tile)) {
            throw new IndexOutOfRangeException();
        }

        var tileIndex = GetTileIndex(tile);

        var col = tileIndex % _columns;
        var row = _rows - 1 - tileIndex / _columns;

        var xPos = _margin + col * (_tileWidth + _spacing);
        var yPos = _margin + row * (_tileHeight + _spacing);
        var pos = new Vector2(xPos, yPos);
        var size = new Vector2(_tileWidth, _tileHeight);

        var rect = new Rect(pos, size);
        return Sprite.Create(_texture, rect, _converter.DefaultPivot, _converter.PixelsPerUnit);
    }

    public override Vector2 GetTileSize(int tile) {
        return new Vector2(_tileWidth, _tileHeight);
    }

    public override Rect[] GetCollider(int tile) {
        var tileIndex = GetTileIndex(tile);
        return _colliders.ContainsKey(tileIndex) ? _colliders[tileIndex] : new Rect[] { };
    }
}
