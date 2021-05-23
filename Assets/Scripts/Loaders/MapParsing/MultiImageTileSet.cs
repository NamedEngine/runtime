using System.Collections.Generic;
using UnityEngine;

public class MultiImageTileSet : TileSet {
    readonly Dictionary<int, TileInfo> _tileInfos;

    public struct TileInfo {
        public Sprite Sprite;
        public Vector2 Size;
        public Rect[] Colliders;
    }

    public MultiImageTileSet(Dictionary<int, TileInfo> tileInfos, int firstGid)
        : base(firstGid) {
        _tileInfos = tileInfos;
    }

    protected override int GetTileCount() {
        return _tileInfos.Count;
    }

    public override Sprite GetSprite(int tile) {
        return _tileInfos[GetTileIndex(tile)].Sprite;
    }

    public override Vector2 GetTileSize(int tile) {
        return _tileInfos[GetTileIndex(tile)].Size;
    }

    public override Rect[] GetCollider(int tile) {
        return _tileInfos[GetTileIndex(tile)].Colliders;
    }
}
