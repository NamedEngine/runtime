using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpriteAnimator : MonoBehaviour {
    SpriteRenderer _spriteRenderer;
    GraphicsConverter _graphicsConverter;
    FileLoader _fileLoader;
    SpriteHolder _defaultSprite;

    class SpriteHolder {
        readonly Sprite[] _sprites = new Sprite[GetIndex(true, true) + 1];
        readonly int _baseIdx = GetIndex(false, false);
        
        public SpriteHolder(Sprite sprite) {
            _sprites[_baseIdx] = sprite;
        }

        public Sprite this[bool flipX, bool flipY] {
            get {
                var idx = GetIndex(flipX, flipY);
                var res = _sprites[idx];
                if (res != default) {
                    return res;
                }

                var baseSprite = _sprites[_baseIdx];
                var pivot = Vector2.zero;
                if (flipX) {
                    pivot.x += 1;
                }
                if (flipY) {
                    pivot.y += 1;
                }
                
                _sprites[idx] = Sprite.Create(baseSprite.texture, baseSprite.rect, pivot, baseSprite.pixelsPerUnit);
                return _sprites[idx];
            }
        }

        static int GetIndex(bool flipX, bool flipY) {
            return Convert.ToInt32(flipX) * 1 + Convert.ToInt32(flipY) * 2;
        }
    }
    
    static readonly Dictionary<string, SpriteHolder> SpriteCache = new Dictionary<string, SpriteHolder>();
    static readonly Dictionary<string, string[]> PathToNamesCache = new Dictionary<string, string[]>();

    SpriteHolder FromCacheOrLoad(string path) {
        if (!SpriteCache.ContainsKey(path)) {
            SpriteCache[path] = new SpriteHolder(_graphicsConverter.PathToSprite(path));
        }
        
        return SpriteCache[path];
    }
    
    SpriteHolder[] FromCacheOrLoadDir(string dirPath) {
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
        _defaultSprite = new SpriteHolder(_spriteRenderer.sprite);
    }
    
    public void UpdateDefaultSprite() {
        _defaultSprite = new SpriteHolder(_spriteRenderer.sprite);
    }

    // TODO: something about sprite size?

    public void SetDefaultSprite(bool flipX = false, bool flipY = false) {
        Debug.Log($"Setting default sprite with flips: {flipX}|{flipY}");
        InternalSetSprite(_defaultSprite, flipX, flipY);
    }

    public void SetSprite(string path, bool flipX = false, bool flipY = false) {
        InternalSetSprite(FromCacheOrLoad(path.ToProperPath()), flipX, flipY);
    }

    void InternalSetSprite(SpriteHolder sprite, bool flipX, bool flipY) {
        StopAllCoroutines();
        _spriteRenderer.flipX = flipX;
        _spriteRenderer.flipY = flipY;

        _spriteRenderer.sprite = sprite[flipX, flipY];
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
        Debug.Log($"Setting animation \"{dirPath}\" with flips: {flipX}|{flipY}");
        _spriteRenderer.flipX = flipX;
        _spriteRenderer.flipY = flipY;

        var sprites = FromCacheOrLoadDir(dirPath.ToProperPath());

        var timePerSprite = time / sprites.Length;

        var infinite = repeats == 0;
        var currSprite = 0;
        while (repeats >= 0) {
            _spriteRenderer.sprite = sprites[currSprite][flipX, flipY];
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
