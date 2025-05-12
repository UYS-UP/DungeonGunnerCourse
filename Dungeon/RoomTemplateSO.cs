using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Room_", menuName = "SO/Dungeon/Room")]
public class RoomTemplateSO : ScriptableObject
{
    public string guid;
    [Tooltip("房间预制体")]
    public GameObject prefab;
    [HideInInspector] public GameObject previousPrefab;
    public RoomNodeTypeSO roomNodeType;
    
    [Tooltip("房间包围盒的左下角位置信息")]
    public Vector2Int lowerBounds;
    [Tooltip("房间包围盒的右上角位置信息")]
    public Vector2Int upperBounds;
    
    [Tooltip("入口信息列表")]
    [SerializeField] public List<Doorway> doorwayList;
    
    [Tooltip("进入房间位置")]
    public Vector2Int[] spawnPositionArray;

    public List<Doorway> GetDoorwayList()
    {
        return doorwayList;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (guid == "" || previousPrefab != prefab)
        {
            guid = Guid.NewGuid().ToString();
            previousPrefab = prefab;
            EditorUtility.SetDirty(this);
        }

        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(doorwayList), doorwayList);
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(spawnPositionArray), spawnPositionArray);
    }
#endif
}
