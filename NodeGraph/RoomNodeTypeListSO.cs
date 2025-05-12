using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeTypeList", menuName = "SO/Dungeon/Room Node Type List")]
public class RoomNodeTypeListSO : ScriptableObject
{
   [Header("房间节点列表")]
   public List<RoomNodeTypeSO> list;

#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(list), list);
    }
#endif
}
