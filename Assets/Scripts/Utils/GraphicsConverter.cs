using System.IO;
using UnityEngine;

public class GraphicsConverter: MonoBehaviour {
    [SerializeField] PathLimiter pathLimiter;
    [SerializeField] float pixelsPerUnit = 100;
    public float PixelsPerUnit => pixelsPerUnit;

    public byte[] PathToBytes(string path) {
        pathLimiter.CheckRange(path);
        return File.ReadAllBytes(path);
    }
    
    public Texture2D BytesToTexture(byte[] bytes) {
        var texture = new Texture2D(1, 1);
        texture.LoadImage(bytes);
        texture.filterMode = FilterMode.Point;
        return texture;
    }

    public Sprite TextureToSprite(Texture2D texture) {
        var rect = new Rect(Vector2.zero, new Vector2(texture.width, texture.height));
        var sprite = Sprite.Create(texture, rect, Vector2.zero, pixelsPerUnit);
        return sprite;
    }

    public Texture2D PathToTexture(string path) {
        return BytesToTexture(PathToBytes(path));
    }

    public Sprite PathToSprite(string path) {
        return TextureToSprite(PathToTexture(path));
    }
}
