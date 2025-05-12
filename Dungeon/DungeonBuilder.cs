using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class DungeonBuilder : SingletonMonobehaviour<DungeonBuilder>
{
    public Dictionary<string, Room> dungeonBuilderRoomDict = new();
    private Dictionary<string, RoomTemplateSO> roomTemplateDict = new();
    private List<RoomTemplateSO> roomTemplateList;
    private RoomNodeTypeListSO roomNodeTypeList;
    private bool dungeonBuildSuccessful;

    protected override void Awake()
    {
        base.Awake();
        LoadRoomNodeTypeList();
        
        GameResources.Instance.dimmedMaterial.SetFloat("Alpha_Slider", 1f);

    }

    private void LoadRoomNodeTypeList()
    {
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    public bool GenerateDungeon(DungeonLevelSO currentDungeonLevel)
    {
        roomTemplateList = currentDungeonLevel.roomTemplateList;
        LoadRoomTemplatesIntoDict();
        dungeonBuildSuccessful = false;
        int dungeonBuildAttempts = 0;
        while (!dungeonBuildSuccessful && dungeonBuildAttempts < Settings.maxDungeonBuildAttempts)
        {
            dungeonBuildAttempts++;

            var roomNodeGraph = SelectRandomRoomNodeGraph(currentDungeonLevel.roomNodeGraphList);

            int dungeonReBuildAttemptsForNodeGraph = 0;
            dungeonBuildSuccessful = false;

            while (!dungeonBuildSuccessful &&
                   dungeonReBuildAttemptsForNodeGraph <= Settings.maxDungeonRebuildAttemptsForRoomGraph)
            {
                ClearDungeon();
                dungeonReBuildAttemptsForNodeGraph++;
                dungeonBuildSuccessful = AttemptToBuildRandomDungeon(roomNodeGraph);

            }
            
                            
            if (dungeonBuildSuccessful)
            {
                InstantiateRoomGameObject();
            }
            

        }

        return dungeonBuildSuccessful;
    }
    
    /// <summary>
    /// 实例化
    /// </summary>
    private void InstantiateRoomGameObject()
    {
        foreach (var item in dungeonBuilderRoomDict)
        {
            Room room = item.Value;
            Vector3 roomPosition = new Vector3(room.lowerBounds.x - room.templateLowerBounds.x,
                room.lowerBounds.y - room.templateLowerBounds.y, 0f);

            var roomGameObject = Instantiate(room.prefab, roomPosition, Quaternion.identity, transform);
            var instantiateRoom = roomGameObject.GetComponentInChildren<InstantiateRoom>();
            instantiateRoom.room = room;
            instantiateRoom.Initialise(roomGameObject);
            room.instantiateRoom = instantiateRoom;
        }
    }

    /// <summary>
    /// 根据地图随机化构建地牢,使用BFS算法遍历整个地图节点
    /// </summary>
    private bool AttemptToBuildRandomDungeon(RoomNodeGraphSO roomNodeGraph)
    {
        Queue<RoomNodeSO> openRoomNodeQueue = new();
        RoomNodeSO entranceNode = roomNodeGraph.GetRoomNode(roomNodeTypeList.list.Find(x => x.isEntrance));
        if (entranceNode != null)
        {
            openRoomNodeQueue.Enqueue(entranceNode);
        }
        else
        {
            Debug.Log("没有入口");
            return false;
        }
        
        bool noRoomOverlaps = ProcessRoomsInOpenRoomNodeQueue(roomNodeGraph, openRoomNodeQueue, true);
        if (openRoomNodeQueue.Count == 0 && noRoomOverlaps)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool ProcessRoomsInOpenRoomNodeQueue(RoomNodeGraphSO roomNodeGraph, Queue<RoomNodeSO> openRoomNodeQueue, bool noRoomOverlaps)
    {
        while (openRoomNodeQueue.Count > 0 && noRoomOverlaps)
        {
            var roomNode = openRoomNodeQueue.Dequeue();

            foreach (var childRoomNode in roomNodeGraph.GetChildRoomNodes(roomNode))
            {
                openRoomNodeQueue.Enqueue(childRoomNode);
            }

            // 处理入口节点和非入口节点
            if (roomNode.roomNodeType.isEntrance)
            {
                var roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);
                var room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);
                room.isPositioned = true;
                dungeonBuilderRoomDict.Add(room.id, room);
            }
            else
            {
                var parentRoom = dungeonBuilderRoomDict[roomNode.parentRoomNodeIdList[0]];
                noRoomOverlaps = CanPlaceRoomWithNoOverlaps(roomNode, parentRoom);
            }
        }

        return noRoomOverlaps;
        
    }

    
    private bool CanPlaceRoomWithNoOverlaps(RoomNodeSO roomNode, Room parentRoom)
    {
        bool roomOverlaps = true;

        while (roomOverlaps)
        {
            List<Doorway> unconnectedAvailableParentDoorways =
                GetUnconnectedAvailableDoorway(parentRoom.doorwayList).ToList();
            if (unconnectedAvailableParentDoorways.Count == 0)
            {
                return false;
            }

            Doorway doorwayParent =
                unconnectedAvailableParentDoorways[Random.Range(0, unconnectedAvailableParentDoorways.Count)];
            RoomTemplateSO roomTemplate = GetRandomTemplateForRoomConsistentWithParent(roomNode, doorwayParent);
            Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);
            if (PlaceTheRoom(parentRoom, doorwayParent, room))
            {
                roomOverlaps = false;
                room.isPositioned = true;
                dungeonBuilderRoomDict.Add(room.id, room);
            }
            else
            {
                roomOverlaps = true;
            }
        }

        return true;
    }

    private bool PlaceTheRoom(Room parentRoom, Doorway doorwayParent, Room room)
    {
        Doorway doorway = GetOppositeDoorway(doorwayParent, room.doorwayList);
        if (doorway == null)
        {
            doorwayParent.isUnavailable = true;
            return false;
        }
        
        Vector2Int parentDoorwayPosition =
            parentRoom.lowerBounds + doorwayParent.position - parentRoom.templateLowerBounds;
        Vector2Int adjustment = Vector2Int.zero;
        switch (doorway.orientation)
        {
            case Orientation.north:
                adjustment = new Vector2Int(0, -1);
                break;
            case Orientation.east:
                adjustment = new Vector2Int(-1, 0);
                break;
            case Orientation.south:
                adjustment = new Vector2Int(0, 1);
                break;
            case Orientation.west:
                adjustment = new Vector2Int(1, 0);
                break;
            case Orientation.none:
                break;
        }

        // 计算全局边界
        room.lowerBounds = parentDoorwayPosition + adjustment + room.templateLowerBounds - doorway.position;
        room.upperBounds = room.lowerBounds + room.templateUpperBounds - room.templateLowerBounds;
        Room overlappingRoom = CheckForRoomOverlap(room);
        if (overlappingRoom == null)
        {
            doorwayParent.isConnected = true;
            doorwayParent.isUnavailable = true;

            doorway.isConnected = true;
            doorway.isUnavailable = true;

            return true;
        }
        else
        {
            doorwayParent.isUnavailable = true;
            return false;
        }
    }

    private Room CheckForRoomOverlap(Room roomToTest)
    {
        foreach (var item in dungeonBuilderRoomDict)
        {
            Room room = item.Value;
            if (room.id == roomToTest.id || !room.isPositioned)
            {
                continue;
            }

            if (IsOverLappingRoom(roomToTest, room))
            {
                return room;
            }
        }

        return null;
    }

    private bool IsOverLappingRoom(Room room1, Room room2)
    {
        bool isOverlappingX = IsOverLappingInterval(room1.lowerBounds.x, room1.upperBounds.x, room2.lowerBounds.x, room2.upperBounds.x);

        bool isOverlappingY = IsOverLappingInterval(room1.lowerBounds.y, room1.upperBounds.y, room2.lowerBounds.y, room2.upperBounds.y);

        return isOverlappingX && isOverlappingY;

    }

    /// <summary>
    /// 判断两个房间位置是否有重叠
    /// </summary>
    private bool IsOverLappingInterval(int min1, int max1, int min2, int max2)
    {
        return Mathf.Max(min1, min2) <= Mathf.Min(max1, max2);
    }
    
    private Doorway GetOppositeDoorway(Doorway doorwayParent, List<Doorway> roomDoorwayList)
    {
        foreach (var doorwayToCheck in roomDoorwayList)
        {
            switch (doorwayParent.orientation)
            {
                case Orientation.east when doorwayToCheck.orientation == Orientation.west:
                    return doorwayToCheck;
                case Orientation.west when doorwayToCheck.orientation == Orientation.east:
                    return doorwayToCheck;
                case Orientation.north when doorwayToCheck.orientation == Orientation.south:
                    return doorwayToCheck;
                case Orientation.south when doorwayToCheck.orientation == Orientation.north:
                    return doorwayToCheck;
            }
        }

        return null;
    }

    private RoomTemplateSO GetRandomTemplateForRoomConsistentWithParent(RoomNodeSO roomNode, Doorway doorwayParent)
    {
        RoomTemplateSO roomTemplate = null;
        if (roomNode.roomNodeType.isCorridor)
        {
            switch (doorwayParent.orientation)
            {
                case Orientation.north:
                case Orientation.south:
                    roomTemplate = GetRandomRoomTemplate(roomNodeTypeList.list.Find(x => x.isCorridorNS));
                    break;
                case Orientation.east:
                case Orientation.west:
                    roomTemplate = GetRandomRoomTemplate(roomNodeTypeList.list.Find(x => x.isCorridorEW));
                    break;
                case Orientation.none:
                    break;
                default:
                    break;
            }
        }
        else
        {
            roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);
        }

        return roomTemplate;
    }

    private IEnumerable<Doorway> GetUnconnectedAvailableDoorway(List<Doorway> roomDoorwayList)
    {
        foreach (var doorway in roomDoorwayList)
        {
            if (!doorway.isConnected && !doorway.isUnavailable)
                yield return doorway;
        }
    }

    private Room CreateRoomFromRoomTemplate(RoomTemplateSO roomTemplate, RoomNodeSO roomNode)
    {
        Room room = new Room();
        room.templateId = roomTemplate.guid;
        room.id = roomNode.id;
        room.prefab = roomTemplate.prefab;
        room.roomNodeType = roomNode.roomNodeType;
        room.lowerBounds = roomTemplate.lowerBounds;
        room.upperBounds = roomTemplate.upperBounds;
        room.spawnPositionArray = roomTemplate.spawnPositionArray;
        room.templateLowerBounds = roomTemplate.lowerBounds;
        room.templateUpperBounds = roomTemplate.upperBounds;
        room.childRoomIdList = CopyStringList(roomNode.childRoomNodeIdList);
        room.doorwayList = CopyDoorwayList(roomTemplate.doorwayList);

        if (roomNode.parentRoomNodeIdList.Count == 0)
        {
            room.parentRoomId = "";
            room.isPreviouslyVisited = true;
        }
        else
        {
            room.parentRoomId = roomNode.parentRoomNodeIdList[0];
        }

        return room;
    }

    /// <summary>
    /// 随机获取一个对应类型的房间模板
    /// </summary>
    private RoomTemplateSO GetRandomRoomTemplate(RoomNodeTypeSO roomNodeType)
    {
        List<RoomTemplateSO> matchingRoomTemplateList = new List<RoomTemplateSO>();
        foreach (var roomTemplate in roomTemplateList)
        {
            if (roomTemplate.roomNodeType == roomNodeType)
            {
                matchingRoomTemplateList.Add(roomTemplate);
            }
        }

        if (matchingRoomTemplateList.Count == 0) return null;
        return matchingRoomTemplateList[Random.Range(0, matchingRoomTemplateList.Count)];
    }


    private void LoadRoomTemplatesIntoDict()
    {
        roomTemplateDict.Clear();
        foreach (var roomTemplate in roomTemplateList)
        {
            if (!roomTemplateDict.TryAdd(roomTemplate.guid, roomTemplate))
            {
                Debug.Log("已经存在Key为" + roomTemplate.guid + "的房间");
            }
        }
    }

    /// <summary>
    /// 随机选择地图构建
    /// </summary>
    private RoomNodeGraphSO SelectRandomRoomNodeGraph(List<RoomNodeGraphSO> roomNodeGraphList)
    {
        if (roomNodeGraphList.Count > 0)
        {
            return roomNodeGraphList[Random.Range(0, roomNodeGraphList.Count)];
        }
        else
        {
            Debug.Log("没有任何地图模板可以创建");
            return null;
        }
    }

    private List<string> CopyStringList(List<string> oldStringList)
    {
        List<string> newStringList = new List<string>();
        foreach (var stringValue in oldStringList)
        {
            newStringList.Add(stringValue);
        }

        return newStringList;
    }

    private List<Doorway> CopyDoorwayList(List<Doorway> oldDoorwayList)
    {
        List<Doorway> newDoorwayList = new List<Doorway>();
        foreach (var doorway in oldDoorwayList)
        {
            Doorway newDoorway = new Doorway();
            newDoorway.position = doorway.position;
            newDoorway.orientation = doorway.orientation;
            newDoorway.doorPrefab = doorway.doorPrefab;
            newDoorway.isConnected = doorway.isConnected;
            newDoorway.isUnavailable = doorway.isUnavailable;
            newDoorway.doorwayStartCopyPosition = doorway.doorwayStartCopyPosition;
            newDoorway.doorwayCopyTileWidth = doorway.doorwayCopyTileWidth;
            newDoorway.doorwayCopyTileHeight = doorway.doorwayCopyTileHeight;
            
            newDoorwayList.Add(newDoorway);
        }

        return newDoorwayList;
    }

    public RoomTemplateSO GetRoomTemplateById(string id)
    {
        return roomTemplateDict.GetValueOrDefault(id);
    }

    public Room GetRoomById(string id)
    {
        return dungeonBuilderRoomDict.GetValueOrDefault(id);
    }
    
    
    private void ClearDungeon()
    {
        if (dungeonBuilderRoomDict.Count > 0)
        {
            foreach (var item in dungeonBuilderRoomDict)
            {
                Room room = item.Value;
                if (room.instantiateRoom != null)
                {
                    Destroy(room.instantiateRoom.gameObject);
                }
            }
            dungeonBuilderRoomDict.Clear();
        }
    }
}
