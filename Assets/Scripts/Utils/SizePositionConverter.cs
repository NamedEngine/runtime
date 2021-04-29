﻿using System;
using UnityEngine;

public class SizePositionConverter : MonoBehaviour {
    [SerializeField] GraphicsConverter _graphicsConverter;

    static readonly Vector2 VerticalFlip = new Vector2(1, -1);
    
    public Vector2 PositionM2U(Vector2 posOnMap, float heightOnMap) {
        return (posOnMap * VerticalFlip + Vector2.down * heightOnMap) * SizeM2U;
    }

    public Vector2 PositionU2M(Vector2 posInUnity, float heightInUnity) {
        return (posInUnity + Vector2.up * heightInUnity) * VerticalFlip / SizeM2U;
    }
    
    public float SizeM2U => 1 / _graphicsConverter.PixelsPerUnit;
}