using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomEnemySpawnParameters : MonoBehaviour
{
    public DungeonLevelSO dungeonLevel;
    public int minTotalEnemiesToSpawn;
    public int maxTotalEnemiesToSpawn;
    public int minConcurrentEnemies;
    public int maxConcurrentEnemies;
    public int minSpawnInterval;
    public int maxSpawnInterval;
}
