using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNode", menuName = "SO/Dungeon/Room Node")]
public class RoomNodeSO : ScriptableObject
{
    public string id;
    public List<string> parentRoomNodeIdList = new List<string>();
    public List<string> childRoomNodeIdList = new List<string>();
    [HideInInspector] public RoomNodeGraphSO roomNodeGraph;
    public RoomNodeTypeSO roomNodeType;
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;


    
    
#if UNITY_EDITOR
    [HideInInspector] public Rect rect;
    [HideInInspector] public bool isLeftClickDragging;
    [HideInInspector] public bool isSelected;
    public void Initialize(Rect rect, RoomNodeGraphSO roomNodeGraph, RoomNodeTypeSO roomNodeType)
    {
        this.rect = rect;
        this.name = "RoomNode";
        this.id = Guid.NewGuid().ToString();
        this.roomNodeType = roomNodeType;
        this.roomNodeGraph = roomNodeGraph;

        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }
    
    public void Draw(GUIStyle roomNodeStyle)
    {
        GUILayout.BeginArea(rect, roomNodeStyle);
        EditorGUI.BeginChangeCheck();
        if (parentRoomNodeIdList.Count > 0 || roomNodeType.isEntrance)
        {
            EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName);
        }
        else
        {
            int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);
            int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay());
            roomNodeType = roomNodeTypeList.list[selection];

            // 解决删除节点后出现可编辑节点导致规则破坏
            if (roomNodeTypeList.list[selected].isCorridor && !roomNodeTypeList.list[selection].isCorridor ||
                !roomNodeTypeList.list[selected].isCorridor
                && roomNodeTypeList.list[selection].isCorridor || !roomNodeTypeList.list[selection].isBossRoom &&
                roomNodeTypeList.list[selection].isBossRoom)
            {
                if (childRoomNodeIdList.Count > 0)
                {
                    for (int i = childRoomNodeIdList.Count - 1; i >= 0; i--)
                    {
                        var childRoomNode = roomNodeGraph.GetRoomNode(childRoomNodeIdList[i]);
                        if (childRoomNode == null) continue;
                        RemoveChildRoomNodeIdFromRoomNode(childRoomNode.id);
                        childRoomNode.RemoveParentRoomNodeIdFromRoomNode(id);
                    }
                }
            }
        }

        if(EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(this);
        GUILayout.EndArea();
    }

    private string[] GetRoomNodeTypesToDisplay()
    {
        string[] roomArray = new string[roomNodeTypeList.list.Count];
        for (int i = 0; i < roomNodeTypeList.list.Count; i++)
        {
            if (roomNodeTypeList.list[i].displayInNodeGraphEditor)
            {
                roomArray[i] = roomNodeTypeList.list[i].roomNodeTypeName;
            }
        }

        return roomArray;
    }
    
    public void ProcessEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;
            default:
                break;
        }
    }

    private void ProcessMouseDragEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            ProcessLeftDragEvent(currentEvent);
        }
    }

    private void ProcessLeftDragEvent(Event currentEvent)
    {
        isLeftClickDragging = true;
        DragNode(currentEvent.delta);
        GUI.changed = true;
    }
    
    public void DragNode(Vector2 delta)
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
    }

    private void ProcessMouseUpEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            ProcessLeftClickUpEvent();
        }
    }

    private void ProcessLeftClickUpEvent()
    {
        if (isLeftClickDragging)
        {
            isLeftClickDragging = false;
        }
    }

    private void ProcessMouseDownEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            ProcessLeftClickDownEvent();
        }else if (currentEvent.button == 1)
        {
            ProcessRightClickDownEvent(currentEvent);
        }
    }

    private void ProcessRightClickDownEvent(Event currentEvent)
    {
        roomNodeGraph.SetNodeToDrawConnectionLineFrom(this, currentEvent.mousePosition);
    }

    private void ProcessLeftClickDownEvent()
    {
        Selection.activeObject = this;
        isSelected = !isSelected;
    }

    public bool AddChildRoomNodeIdToRoomNode(string childId)
    {
        if (!IsChildRoomValid(childId)) return false;
        childRoomNodeIdList.Add(childId);
        return true;
    }

    // 验证连接有效性
    private bool IsChildRoomValid(string childId)
    {
        bool isConnectedBossNodeAlready = false;
        foreach (var node in roomNodeGraph.roomNodeList)
        {
            if (node.roomNodeType.isBossRoom && node.parentRoomNodeIdList.Count > 0)
            {
                isConnectedBossNodeAlready = true;
            }
        }

        // 如果子节点是Boss节点且已经有Boss节点则返回false
        if (roomNodeGraph.GetRoomNode(childId).roomNodeType.isBossRoom && isConnectedBossNodeAlready)
        {
            Debug.Log("如果子节点是Boss节点且已经有Boss节点则返回false");
            return false;
        }

        // 如果子节点是Node节点则返回false
        if (roomNodeGraph.GetRoomNode(childId).roomNodeType.isNone)
        {
            Debug.Log("如果子节点是Node节点则返回false");
            return false;
        }

        // 如果子节点是自己则返回false
        if (id == childId)
        {
            Debug.Log("如果子节点是自己则返回false");
            return false;
        }

        // 如果子节点已经有父节点则返回false
        if (roomNodeGraph.GetRoomNode(childId).parentRoomNodeIdList.Count > 0)
        {
            Debug.Log("如果子节点已经有父节点则返回false");
            return false;
        }

        // 如果子节点是走廊节点，父节点也是走廊节点则返回false
        if (roomNodeGraph.GetRoomNode(childId).roomNodeType.isCorridor && roomNodeType.isCorridor)
        {
            Debug.Log("如果子节点是走廊节点，父节点也是走廊节点则返回false");
            return false;
        }

        // 如果子节点不是走廊节点，父节点也不是走廊节点则返回false
        if (!roomNodeGraph.GetRoomNode(childId).roomNodeType.isCorridor && !roomNodeType.isCorridor)
        {
            Debug.Log("如果子节点不是走廊节点，父节点也不是走廊节点则返回false");
            return false;
        }

        // 如果子节点是走廊节点且父节点的子节点数量超过最大限制则返回false
        if (roomNodeGraph.GetRoomNode(childId).roomNodeType.isCorridor &&
            childRoomNodeIdList.Count >= Settings.maxChildCorridors)
        {
            Debug.Log("如果子节点是走廊节点且父节点的子节点数量超过最大限制则返回false");
            return false;
        }

        // 如果子节点是一个入口节点则返回false
        if (roomNodeGraph.GetRoomNode(childId).roomNodeType.isEntrance)
        {
            Debug.Log("如果子节点是一个入口节点则返回false");
            return false;
        }

        // 如果子节点不是走廊节点且已经有房间相连则返回false
        if (!roomNodeGraph.GetRoomNode(childId).roomNodeType.isCorridor && childRoomNodeIdList.Count > 0)
        {
            Debug.Log("如果子节点不是走廊节点且已经有房间相连则返回false");
            return false;
        }

        return true;
    }

    public bool AddParentRoomNodeIdToRoomNode(string parentId)
    {
        parentRoomNodeIdList.Add(parentId);
        return true;
    }

    public bool RemoveChildRoomNodeIdFromRoomNode(string childId)
    {
        if (!childRoomNodeIdList.Contains(childId)) return false;
        childRoomNodeIdList.Remove(childId);
        return true;

    }
    
    public bool RemoveParentRoomNodeIdFromRoomNode(string parentId)
    {
        if (!parentRoomNodeIdList.Contains(parentId)) return false;
        parentRoomNodeIdList.Remove(parentId);
        return true;
    }
    

    
    
#endif



}
