using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SpriteAnimator : MonoBehaviour {
    SpriteRenderer _spriteRenderer;
    GraphicsConverter _graphicsConverter;
    
    static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();

    Sprite FromCacheOrLoad(string path) {
        if (!SpriteCache.ContainsKey(path)) {
            SpriteCache[path] = _graphicsConverter.PathToSprite(path);
        }

        return SpriteCache[path]; 
    }

    void Start() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _graphicsConverter = GameObject.Find("Utils").GetComponent<GraphicsConverter>();
    }
    
    // TODO: something about sprite size?

    public void SetSprite(string path) {
        StopAllCoroutines();
        _spriteRenderer.sprite = FromCacheOrLoad(path);
    }

    public void SetAnimation(string dirPath, float time, int repeats) {
        StopAllCoroutines();
        StartCoroutine(PlayAnimationCoro(dirPath, time, repeats));
    }

    public IEnumerator PlayAnimation(string dirPath, float time, int repeats) {
        StopAllCoroutines();

        yield return StartCoroutine(PlayAnimationCoro(dirPath, time, repeats));
    }

    IEnumerator PlayAnimationCoro(string dirPath, float time, int repeats) {
        var sprites = Directory.GetFiles(dirPath)
            .Select(FromCacheOrLoad)
            .ToArray();

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