using UnityEngine;

public class GraphicsConverter: MonoBehaviour {
    [SerializeField] FileLoader fileLoader;
    [SerializeField] float pixelsPerUnit = 100;
    public float PixelsPerUnit => pixelsPerUnit;
    public readonly Vector2 DefaultPivot = new Vector2(.5f, .5f);

    byte[] PathToBytes(string path) {
        return fileLoader.LoadBytes(path, PathType.Image);
    }

    Texture2D BytesToTexture(byte[] bytes) {
        var texture = new Texture2D(1, 1);
        texture.LoadImage(bytes);
        texture.filterMode = FilterMode.Point;
        return texture;
    }

    Sprite TextureToSprite(Texture2D texture) {
        var rect = new Rect(Vector2.zero, new Vector2(texture.width, texture.height));
        var sprite = Sprite.Create(texture, rect, DefaultPivot, pixelsPerUnit);
        return sprite;
    }

    public Texture2D PathToTexture(string path) {
        return BytesToTexture(PathToBytes(path));
    }

    public Sprite PathToSprite(string path) {
        return TextureToSprite(PathToTexture(path));
    }
}
