using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Doorway
{
    public Vector2Int position;
    public Orientation orientation;
    public GameObject doorPrefab;

    [Header("入口左上角位置信息")] public Vector2Int doorwayStartCopyPosition;
    [Header("入口宽度信息")] public int doorwayCopyTileWidth;
    [Header("入口高度信息")] public int doorwayCopyTileHeight;
    [HideInInspector] public bool isConnected = false;
    [HideInInspector] public bool isUnavailable = false;
}
