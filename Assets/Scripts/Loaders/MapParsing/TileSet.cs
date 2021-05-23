using UnityEngine;

public abstract class TileSet {
    readonly int _firstGid;

    protected TileSet(int firstGid) {
        _firstGid = firstGid;
    }

    public bool InRange(int tile) {
        return tile >= _firstGid && tile < _firstGid + GetTileCount();
    }

    protected abstract int GetTileCount();

    protected int GetTileIndex(int tile) {
        return tile - _firstGid;
    }

    public abstract Sprite GetSprite(int tile);

    public abstract Vector2 GetTileSize(int tile);

    public abstract Rect[] GetCollider(int tile);
}
