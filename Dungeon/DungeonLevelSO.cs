using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DungeonLevel", menuName = "SO/Dungeon/Dungeon Level")]
public class DungeonLevelSO : ScriptableObject
{
   public string levelName;
   public List<RoomTemplateSO> roomTemplateList;
   public List<RoomNodeGraphSO> roomNodeGraphList;

#if UNITY_EDITOR
   private void OnValidate()
   {
      HelperUtilities.ValidateCheckEmptyString(this, nameof(levelName), levelName);
      if(HelperUtilities.ValidateCheckEnumerableValues(this, nameof(roomTemplateList), roomTemplateList)) return;
      if(HelperUtilities.ValidateCheckEnumerableValues(this, nameof(roomNodeGraphList), roomNodeGraphList)) return;

      bool isEWCorridor = false;
      bool isNSCorridor = false;
      bool isEntrance = false;

      foreach (var roomTemplate in roomTemplateList)
      {
         if(roomTemplate == null) return;
         if (roomTemplate.roomNodeType.isCorridorEW) isEWCorridor = true;
         if (roomTemplate.roomNodeType.isCorridorNS) isNSCorridor = true;
         if (roomTemplate.roomNodeType.isEntrance) isEntrance = true;
      }

      if (!isEWCorridor) Debug.Log(this.name + "房间没有EWCorridor");
      if(!isNSCorridor) Debug.Log(this.name + "房间没有NSCorridor");
      if(!isEntrance) Debug.Log(this.name + "房间没有Entrance");

      foreach (var roomNodeGraph in roomNodeGraphList)
      {
         if(roomNodeGraph == null) return;
         foreach (var roomNode in roomNodeGraph.roomNodeList)
         {
            if(roomNode == null) continue;
            if (roomNode.roomNodeType.isEntrance || roomNode.roomNodeType.isCorridorNS ||
                roomNode.roomNodeType.isCorridorEW || roomNode.roomNodeType.isCorridor || roomNode.roomNodeType.isNone) continue;

            bool isRoomNodeTypeFound = false;
            foreach (var roomTemplate in roomTemplateList)
            {
               if(roomTemplate == null) continue;

               if (roomTemplate.roomNodeType == roomNode.roomNodeType)
               {
                  isRoomNodeTypeFound = true;
                  break;
               }
            }

            if (!isRoomNodeTypeFound)
               Debug.Log($"{roomNodeGraph.name}在{this.name}中没有找到房间类型为{roomNode.roomNodeType.name}");
            
         }
      }
      
   }
#endif
}
