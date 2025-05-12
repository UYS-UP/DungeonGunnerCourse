using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "RoomNodeGraph", menuName = "SO/Dungeon/Room Node Graph")]
public class RoomNodeGraphSO : ScriptableObject
{
   
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;
    [HideInInspector] public List<RoomNodeSO> roomNodeList = new List<RoomNodeSO>();
    public Dictionary<string, RoomNodeSO> roomNodeDict = new Dictionary<string, RoomNodeSO>();

    private void Awake()
    {
        LoadRoomNodeDict();
    }

    private void LoadRoomNodeDict()
    {
        roomNodeDict.Clear();
        foreach (var node in roomNodeList)
        {
            roomNodeDict[node.id] = node;
        }
    }

    public RoomNodeSO GetRoomNode(RoomNodeTypeSO roomNodeType)
    {
        foreach (var roomNode in roomNodeList)
        {
            if (roomNode.roomNodeType == roomNodeType)
            {
                return roomNode;
            }
        }

        return null;
    }

    public RoomNodeSO GetRoomNode(string id)
    {
        return roomNodeDict.GetValueOrDefault(id);
    }

    public IEnumerable<RoomNodeSO> GetChildRoomNodes(RoomNodeSO parentRoomNode)
    {
        foreach (var childNodeId in parentRoomNode.childRoomNodeIdList)
        {
            yield return GetRoomNode(childNodeId);
        }
    }

#if UNITY_EDITOR
    [HideInInspector] public RoomNodeSO roomNodeToDrawLineFrom;
    [HideInInspector] public Vector2 linePosition = Vector2.zero;

    public void SetNodeToDrawConnectionLineFrom(RoomNodeSO node, Vector2 position)
    {
        roomNodeToDrawLineFrom = node;
        linePosition = position;
    }

    public void OnValidate()
    {
        LoadRoomNodeDict();
    }

#endif

}
