using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpawnableObject<T>
{
    private struct chanceBoundaries
    {
        public T spwanableObject;
        public int lowBoundaryValue;
        public int highBoundaryVlaue;
    }

    private int ratioValueTotal = 0;
    private List<chanceBoundaries> chanceBoundariesList = new List<chanceBoundaries>();
    private List<SpawnableObjectsLevel<T>> spawnableObjectsLevels;

    public RandomSpawnableObject(List<SpawnableObjectsLevel<T>> spawnableObjectsLevels)
    {
        this.spawnableObjectsLevels = spawnableObjectsLevels;
    }

    public T GetItem()
    {
        int upperBoundary = -1;
        ratioValueTotal = 0;
        chanceBoundariesList.Clear();
        T spawnableObject = default(T);
        foreach (var spawnableObjectsLevel in spawnableObjectsLevels)
        {
            if (spawnableObjectsLevel.dungeonLevel == GameManager.Instance.GetCurrentDungeonLevel())
            {
                foreach (var spawnableObjectRatio in spawnableObjectsLevel.spawnableObjectRatios)
                {
                    int lowBoundary = upperBoundary + 1;
                    upperBoundary = lowBoundary + spawnableObjectRatio.ratio - 1;
                    ratioValueTotal += spawnableObjectRatio.ratio;
                    chanceBoundariesList.Add(new chanceBoundaries()
                    {
                        spwanableObject = spawnableObjectRatio.dungeonObject,
                        lowBoundaryValue = lowBoundary,
                        highBoundaryVlaue = upperBoundary
                    });
                }
            }
        }

        if (chanceBoundariesList.Count == 0) return default(T);
        int lookUpValue = Random.Range(0, ratioValueTotal);
        foreach (var spawnChance in chanceBoundariesList)
        {

            if (lookUpValue >= spawnChance.lowBoundaryValue && lookUpValue <= spawnChance.highBoundaryVlaue)
            {
                spawnableObject = spawnChance.spwanableObject;
                break;
            }
        }

        return spawnableObject;
    }
}
