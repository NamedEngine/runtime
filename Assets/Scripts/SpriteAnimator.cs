using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpriteAnimator : MonoBehaviour {
    SpriteRenderer _spriteRenderer;
    GraphicsConverter _graphicsConverter;
    FileLoader _fileLoader;
    Sprite _defaultSprite;

    static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();
    static readonly Dictionary<string, string[]> PathToNamesCache = new Dictionary<string, string[]>();

    Sprite FromCacheOrLoad(string path) {
        if (!SpriteCache.ContainsKey(path)) {
            SpriteCache[path] = _graphicsConverter.PathToSprite(path);
        }
        
        return SpriteCache[path];
    }
    
    Sprite[] FromCacheOrLoadDir(string dirPath) {
        if (!PathToNamesCache.ContainsKey(dirPath)) {
            PathToNamesCache[dirPath] = _fileLoader
                .LoadAllWithExtensionAndNames((path) => "", null, dirPath)
                .Select(pair => pair.Item2)
                .ToArray();
        }
        
        return PathToNamesCache[dirPath]
            .Select(FromCacheOrLoad)
            .ToArray();
    }
    
    

    void Awake() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        var utils = GameObject.Find("Utils");
        _graphicsConverter = utils.GetComponent<GraphicsConverter>();
        _fileLoader = utils.GetComponent<FileLoader>();
        _defaultSprite = _spriteRenderer.sprite;
    }
    
    public void UpdateDefaultSprite() {
        _defaultSprite = _spriteRenderer.sprite;
    }

    // TODO: something about sprite size?

    public void SetDefaultSprite(bool flipX = false, bool flipY = false) {
        InternalSetSprite(_defaultSprite, flipX, flipY);
    }

    public void SetSprite(string path, bool flipX = false, bool flipY = false) {
        InternalSetSprite(FromCacheOrLoad(path.ToProperPath()), flipX, flipY);
    }

    void InternalSetSprite(Sprite sprite, bool flipX, bool flipY) {
        StopAllCoroutines();
        _spriteRenderer.flipX = flipX;
        _spriteRenderer.flipY = flipY;

        _spriteRenderer.sprite = sprite;
    }

    public void SetAnimation(string dirPath, float time, int repeats, bool flipX = false, bool flipY = false) {
        StopAllCoroutines();
        StartCoroutine(PlayAnimationCoro(dirPath, time, repeats, flipX, flipY));
    }

    public IEnumerator PlayAnimation(string dirPath, float time, int repeats, bool flipX = false, bool flipY = false) {
        StopAllCoroutines();

        yield return StartCoroutine(PlayAnimationCoro(dirPath, time, repeats, flipX, flipY));
    }

    IEnumerator PlayAnimationCoro(string dirPath, float time, int repeats, bool flipX, bool flipY) {
        _spriteRenderer.flipX = flipX;
        _spriteRenderer.flipY = flipY;

        var sprites = FromCacheOrLoadDir(dirPath.ToProperPath());

        var timePerSprite = time / sprites.Length;

        var infinite = repeats == 0;
        var currSprite = 0;
        while (repeats >= 0) {
            _spriteRenderer.sprite = sprites[currSprite];
            currSprite = (currSprite + 1) % sprites.Length;
            
            if (infinite || repeats != 0) {
                yield return new WaitForSeconds(timePerSprite);
            }
            
            if (!infinite) {
                repeats--;
            }
        }
    }
}
