using UnityEngine;

public class SizePositionConverter : MonoBehaviour {
    [SerializeField] GraphicsConverter graphicsConverter;

    static readonly Vector2 VerticalFlip = new Vector2(1, -1);
    
    public Vector2 PositionM2U(Vector2 posOnMap) {
        return posOnMap * VerticalFlip * SizeM2U;
    }

    public Vector2 PositionU2M(Vector2 posInUnity) {
        return posInUnity * VerticalFlip / SizeM2U;
    }

    public Vector2 InitialPositionOnMapToUnity(Vector2 posOnMap, Vector2 sizeOnMap) {
        return (posOnMap + sizeOnMap / 2) * VerticalFlip * SizeM2U;
    }

    public float SizeM2U => 1 / graphicsConverter.PixelsPerUnit;

    public Vector2 DirectionM2U(Vector2 dirOnMap) {
        return dirOnMap * VerticalFlip * SizeM2U;
    }
    
    public Vector2 DirectionU2M(Vector2 dirInUnity) {
        return dirInUnity * VerticalFlip / SizeM2U;
    }
}