using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.MPE;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

public class RoomNodeGraphEditor : EditorWindow
{
    private GUIStyle roomNodeStyle;
    private GUIStyle roomNodeSelectedStyle;
    
    private static RoomNodeGraphSO currentRoomNodeGraph;
    private RoomNodeTypeListSO roomNodeTypeList;
    private RoomNodeSO currentRoomNode;

    private const float nodeWidth = 160f;
    private const float nodeHeight = 75f;
    private const int nodePadding = 25;
    private const int nodeBoarder = 12;
    
    private const float connectionLineWidth = 3f;
    private const float connectionLineArrowSize = 6f;

    private Vector2 graphOffset;
    private Vector2 graphDrag;

    private const float gridLarge = 100f;
    private const float gridSmall = 25f;
    
    [MenuItem("Room Node Graph Editor", menuItem = "Window/Dungeon Editor/Room Node Graph Editor")]
    private static void OpenWindow()
    {
        GetWindow<RoomNodeGraphEditor>("Room Node Graph Editor");
    }

    [OnOpenAsset(0)]
    public static bool OnDoubleClickAsset(int instanceId, int line)
    {
        RoomNodeGraphSO roomNodeGraph = EditorUtility.InstanceIDToObject(instanceId) as RoomNodeGraphSO;
        if (roomNodeGraph != null)
        {
            OpenWindow();
            currentRoomNodeGraph = roomNodeGraph;
            return true;
        }

        return false;
    }

    private void OnEnable()
    {
        Selection.selectionChanged += InspectorSelectionChanged;
        
        roomNodeStyle = new GUIStyle();
        roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        roomNodeStyle.normal.textColor = Color.white;
        roomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeStyle.border = new RectOffset(nodeBoarder, nodeBoarder, nodeBoarder, nodeBoarder);

        roomNodeSelectedStyle = new GUIStyle();
        roomNodeSelectedStyle.normal.background = EditorGUIUtility.Load("node1 on") as Texture2D;
        roomNodeSelectedStyle.normal.textColor = Color.white;
        roomNodeSelectedStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeSelectedStyle.border = new RectOffset(nodeBoarder, nodeBoarder, nodeBoarder, nodeBoarder);

        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= InspectorSelectionChanged;
    }

    private void InspectorSelectionChanged()
    {
        RoomNodeGraphSO roomNodeGraph = Selection.activeObject as RoomNodeGraphSO;
        if (roomNodeGraph != null)
        {
            currentRoomNodeGraph = roomNodeGraph;
            GUI.changed = true;
        }
    }

    private void OnGUI()
    {
        if(currentRoomNodeGraph != null)
        {
            DrawBackgroundGrid(gridSmall, 0.2f, Color.gray);
            DrawBackgroundGrid(gridLarge, 0.3f, Color.gray);
            
            DrawDraggedLine();
            
            ProcessEvents(Event.current);

            DrawRoomConnections();
            
            DrawRoomNodes();
        }


        if (GUI.changed)
        {
            Repaint();
        }
    }

    private void DrawBackgroundGrid(float gridSize, float gridOpacity, Color gridColor)
    {
        int verticalLineCount = Mathf.CeilToInt((position.width + gridSize) / gridSize);
        int horizontalLineCount = Mathf.CeilToInt((position.height + gridSize) / gridSize);

        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);
        graphOffset += graphDrag * 0.5f;

        Vector3 gridOffset = new Vector3(graphOffset.x % gridSize, graphOffset.y % gridSize, 0);
        for (int i = 0; i < verticalLineCount; i++)
        {
            Handles.DrawLine(new Vector3(gridSize * i, -gridSize, 0) + gridOffset, new Vector3(gridSize * i, position.height + gridSize, 0) + gridOffset);
        }

        for (int i = 0; i < horizontalLineCount; i++)
        {
            Handles.DrawLine(new Vector3(-gridSize, gridSize * i, 0) + gridOffset, new Vector3(position.width + gridSize, gridSize * i, 0) + gridOffset);
        }
        Handles.color = Color.white;
    }

    private void DrawRoomConnections()
    {
        foreach (var node in currentRoomNodeGraph.roomNodeList)
        {
            if (node.childRoomNodeIdList.Count > 0)
            {
                foreach (var id in node.childRoomNodeIdList)
                {
                    if (currentRoomNodeGraph.roomNodeDict.ContainsKey(id))
                    {
                        DrawConnectionLine(node, currentRoomNodeGraph.roomNodeDict[id]);
                        GUI.changed = true;
                    }
                }
            }
        }
    }

    private void DrawConnectionLine(RoomNodeSO parent, RoomNodeSO child)
    {
        Vector2 startPosition = parent.rect.center;
        Vector2 endPosition = child.rect.center;

        Vector2 midPosition = (endPosition + startPosition) / 2f;
        Vector2 direction = endPosition - startPosition;

        // 绘制箭头
        Vector2 arrowTailPoint1 =
            midPosition - new Vector2(-direction.y, direction.x).normalized * connectionLineArrowSize;
        Vector2 arrowTailPoint2 =
            midPosition + new Vector2(-direction.y, direction.x).normalized * connectionLineArrowSize;
        Vector2 arrowHeadPoint = midPosition + direction.normalized * connectionLineArrowSize;
        
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint1, arrowHeadPoint, arrowTailPoint1, Color.white, null, connectionLineWidth);
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint2, arrowHeadPoint, arrowTailPoint2, Color.white, null, connectionLineWidth);
        
        Handles.DrawBezier(startPosition, endPosition, startPosition, endPosition, Color.white, null, connectionLineWidth);
        GUI.changed = true;
    }

    private void DrawDraggedLine()
    {
        if (currentRoomNodeGraph.roomNodeToDrawLineFrom != null && currentRoomNodeGraph.linePosition != Vector2.zero)
        {
            Handles.DrawBezier(currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition,
                currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition, Color.white, null, connectionLineWidth);
        }
    }

    private void DrawRoomNodes()
    {
        foreach (var roomNode in currentRoomNodeGraph.roomNodeList)
        {
            roomNode.Draw(roomNode.isSelected ? roomNodeSelectedStyle : roomNodeStyle);
            GUI.changed = true;
        }

        
    }

    private void ProcessEvents(Event currentEvent)
    {
        graphDrag = Vector2.zero;
        
        if(currentRoomNode == null || currentRoomNode.isLeftClickDragging == false)
        {
            currentRoomNode = IsMouseOverRoomNode(currentEvent);
        }

        if (currentRoomNode == null || currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            ProcessRoomNodeGraphEvents(currentEvent);
        }
        else
        {
            currentRoomNode.ProcessEvents(currentEvent);
        }
       
    }

    private RoomNodeSO IsMouseOverRoomNode(Event currentEvent)
    {
        for (int i = currentRoomNodeGraph.roomNodeList.Count - 1; i >= 0; i--)
        {
            if (currentRoomNodeGraph.roomNodeList[i].rect.Contains(currentEvent.mousePosition))
            {
                return currentRoomNodeGraph.roomNodeList[i];
            }
        }

        return null;
    }

    // 处理鼠标事件
    private void ProcessRoomNodeGraphEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;
            default:
                break;
        }
    }

    private void ProcessMouseUpEvent(Event currentEvent)
    {
        if (currentEvent.button == 1 && currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            // 创建父子关系，起始节点为父节点，拖拽结束节点为子节点
            RoomNodeSO roomNode = IsMouseOverRoomNode(currentEvent);
            if (roomNode != null && !roomNode.id.Equals(currentRoomNodeGraph.roomNodeToDrawLineFrom.id))
            {
                if (currentRoomNodeGraph.roomNodeToDrawLineFrom.AddChildRoomNodeIdToRoomNode(roomNode.id))
                {
                    roomNode.AddParentRoomNodeIdToRoomNode(currentRoomNodeGraph.roomNodeToDrawLineFrom.id);
                }
            }
            ClearLineDrag();
        }
    }

    private void ClearLineDrag()
    {
        currentRoomNodeGraph.roomNodeToDrawLineFrom = null;
        currentRoomNodeGraph.linePosition = Vector2.zero;
        GUI.changed = true;
    }

    private void ProcessMouseDragEvent(Event currentEvent)
    {
        if (currentEvent.button == 1)
        {
            DragConnectionLine(currentEvent.delta);
            
        }else if (currentEvent.button == 0)
        {
            DragGraph(currentEvent.delta);
        }
        
    }

    private void DragGraph(Vector2 delta)
    {
        graphDrag = delta;
        for (int i = 0; i < currentRoomNodeGraph.roomNodeList.Count; i++)
        {
            currentRoomNodeGraph.roomNodeList[i].DragNode(delta);
            
        }
        GUI.changed = true;
    }

    private void DragConnectionLine(Vector2 delta)
    {
        currentRoomNodeGraph.linePosition += delta;
        GUI.changed = true;
    }

    private void ProcessMouseDownEvent(Event currentEvent)
    {
        if (currentEvent.button == 1)
        {
            ShowContextMenu(currentEvent.mousePosition);
        }else if (currentEvent.button == 0)
        {
            ClearLineDrag();
            ClearAllSelectedRoomNodes();
        }
    }

    private void ClearAllSelectedRoomNodes()
    {
        foreach (var node in currentRoomNodeGraph.roomNodeList)
        {
            if (!node.isSelected) continue;
            node.isSelected = false;
            GUI.changed = true;
        }
    }

    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("创建节点"), false, CreateRoomNode, mousePosition);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("选中所有节点"), false, SelectAllRoomNode);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("删除选中节点连接"), false, DeleteSelectionRoomNodeLinks);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("删除选中节点"), false, DeleteSelectionRoomNodes);
        menu.ShowAsContext();
    }

    // 删除选中节点
    private void DeleteSelectionRoomNodes()
    {
        Queue<RoomNodeSO> roomNodeDeleteQueue = new Queue<RoomNodeSO>();
        foreach (var node in currentRoomNodeGraph.roomNodeList)
        {
            if(!node.isSelected || node.roomNodeType.isEntrance) continue;
            roomNodeDeleteQueue.Enqueue(node);
            foreach (var childId in node.childRoomNodeIdList)
            {
                var childRoomNode = currentRoomNodeGraph.GetRoomNode(childId);
                if (childRoomNode != null)
                {
                    childRoomNode.RemoveParentRoomNodeIdFromRoomNode(node.id);
                }
            }

            foreach (var parentId in node.parentRoomNodeIdList)
            {
                var parentRoomNode = currentRoomNodeGraph.GetRoomNode(parentId);
                if (parentRoomNode != null)
                {
                    parentRoomNode.RemoveChildRoomNodeIdFromRoomNode(node.id);
                }
            }
        }
        while (roomNodeDeleteQueue.Count > 0)
        {
            var roomNodeToDelete = roomNodeDeleteQueue.Dequeue();
            currentRoomNodeGraph.roomNodeDict.Remove(roomNodeToDelete.id);
            currentRoomNodeGraph.roomNodeList.Remove(roomNodeToDelete);
            
            DestroyImmediate(roomNodeToDelete, true);
            AssetDatabase.SaveAssets();
        }
        

    }

    // 删除选中节点连接
    private void DeleteSelectionRoomNodeLinks()
    {
        foreach (var node in currentRoomNodeGraph.roomNodeList)
        {
            if (node.isSelected && node.childRoomNodeIdList.Count > 0)
            {
                for (int i = node.childRoomNodeIdList.Count - 1; i >= 0; i--)
                {
                    var childRoomNode = currentRoomNodeGraph.GetRoomNode(node.childRoomNodeIdList[i]);
                    if (childRoomNode == null || !childRoomNode.isSelected) continue;
                    node.RemoveChildRoomNodeIdFromRoomNode(childRoomNode.id);
                    childRoomNode.RemoveParentRoomNodeIdFromRoomNode(node.id);
                }
            }
        }
        ClearAllSelectedRoomNodes();
    }

    private void SelectAllRoomNode()
    {
        foreach (var node in currentRoomNodeGraph.roomNodeList)
        {
            node.isSelected = true;
        }

        GUI.changed = true;
    }

    private void CreateRoomNode(object mousePosition)
    {
        // 当创建第一个节点时自动创建入口节点
        if (currentRoomNodeGraph.roomNodeList.Count == 0)
        {
            CreateRoomNode(new Vector2(200f, 200f), roomNodeTypeList.list.Find(x => x.isEntrance));
        }
        CreateRoomNode(mousePosition, roomNodeTypeList.list.Find(x => x.isNone));
    }

    private void CreateRoomNode(object mousePositionObj, RoomNodeTypeSO roomNodeType)
    {
        Vector2 mousePosition = (Vector2)mousePositionObj;
        RoomNodeSO roomNode = ScriptableObject.CreateInstance<RoomNodeSO>();
        currentRoomNodeGraph.roomNodeList.Add(roomNode);
        roomNode.Initialize(new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)), currentRoomNodeGraph,
            roomNodeType);
        AssetDatabase.AddObjectToAsset(roomNode, currentRoomNodeGraph);
        AssetDatabase.SaveAssets();
        
        currentRoomNodeGraph.OnValidate();
    }
}
