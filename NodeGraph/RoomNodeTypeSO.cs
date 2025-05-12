using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeType", menuName = "SO/Dungeon/Room Node Type")]
public class RoomNodeTypeSO : ScriptableObject
{
    [Header("房间类型")] public string roomNodeTypeName;
    [Header("是否在编辑器窗口可见")] public bool displayInNodeGraphEditor = true;
    [Header("走廊")] public bool isCorridor;
    [Header("上下走廊")] public bool isCorridorNS;
    [Header("左右走廊")] public bool isCorridorEW;
    [Header("入口")] public bool isEntrance;
    [Header("Boss房间")] public bool isBossRoom;
    [Header("无类型")] public bool isNone;
    
    #if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(roomNodeTypeName), roomNodeTypeName);
    }
    
    #endif
}
