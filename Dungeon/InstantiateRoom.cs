using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(BoxCollider2D))]
public class InstantiateRoom : MonoBehaviour
{
    [HideInInspector] public Room room;
    [HideInInspector] public Grid grid;
    [HideInInspector] public Tilemap ground;
    [HideInInspector] public Tilemap decoration1;
    [HideInInspector] public Tilemap decoration2;
    [HideInInspector] public Tilemap front;
    [HideInInspector] public Tilemap collision;
    [HideInInspector] public Tilemap minMap;
    [HideInInspector] public Bounds roomColliderBounds;

    private BoxCollider2D boxCollider2D;

    private void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();

        roomColliderBounds = boxCollider2D.bounds;
    }

    public void Initialise(GameObject roomGameObject)
    {
        PopulateTilemapMemberVariables(roomGameObject);

        BlockOffUnuseDoorways();
        
        DisableCollisionTilemapRendere();
    }

    private void BlockOffUnuseDoorways()
    {
        foreach (var doorway in room.doorwayList)
        {
            if (doorway.isConnected) continue;
            if (collision != null)
            {
                BlockDoorwayOnTilemapLayer(collision, doorway);
            }
            if (ground != null)
            {
                BlockDoorwayOnTilemapLayer(ground, doorway);
            }
            if (minMap != null)
            {
                BlockDoorwayOnTilemapLayer(minMap, doorway);
            }
            if (decoration2 != null)
            {
                BlockDoorwayOnTilemapLayer(decoration2, doorway);
            }
            if (decoration1 != null)
            {
                BlockDoorwayOnTilemapLayer(decoration1, doorway);
            }
            if (front != null)
            {
                BlockDoorwayOnTilemapLayer(front, doorway);
            }
        }
    }

    private void BlockDoorwayOnTilemapLayer(Tilemap tilemap, Doorway doorway)
    {
        switch (doorway.orientation)
        {
            case Orientation.north:
            case Orientation.south:
                BlockDoorwayHorizontally(tilemap, doorway);
                break;

            case Orientation.east:
            case Orientation.west:
                BlockDoorwayVertically(tilemap, doorway);
                break;

            case Orientation.none:
                break;

        }
    }

    private void BlockDoorwayHorizontally(Tilemap tilemap, Doorway doorway)
    {
        Vector2Int startPosition = doorway.doorwayStartCopyPosition;
        for (int xPos = 0; xPos < doorway.doorwayCopyTileWidth; xPos++)
        {
            for (int yPos = 0; yPos < doorway.doorwayCopyTileHeight; yPos++)
            {
                // Get rotation of tile being copied
                Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0));

                // Copy tile
                tilemap.SetTile(new Vector3Int(startPosition.x + 1 + xPos, startPosition.y - yPos, 0), tilemap.GetTile(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0)));

                // Set rotation of tile copied
                tilemap.SetTransformMatrix(new Vector3Int(startPosition.x + 1 + xPos, startPosition.y - yPos, 0), transformMatrix);
            }
        }

    }

    private void BlockDoorwayVertically(Tilemap tilemap, Doorway doorway)
    {
        Vector2Int startPosition = doorway.doorwayStartCopyPosition;
        for (int yPos = 0; yPos < doorway.doorwayCopyTileHeight; yPos++)
        {

            for (int xPos = 0; xPos < doorway.doorwayCopyTileWidth; xPos++)
            {
                // Get rotation of tile being copied
                Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0));

                // Copy tile
                tilemap.SetTile(new Vector3Int(startPosition.x + xPos, startPosition.y - 1 - yPos, 0), tilemap.GetTile(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0)));

                // Set rotation of tile copied
                tilemap.SetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - 1 - yPos, 0), transformMatrix);

            }

        }

    }

    private void DisableCollisionTilemapRendere()
    {
        collision.gameObject.GetComponent<TilemapRenderer>().enabled = false;
    }

    private void PopulateTilemapMemberVariables(GameObject roomGameObject)
    {
        grid = roomGameObject.GetComponentInChildren<Grid>();
        Tilemap[] tilemaps = roomGameObject.GetComponentsInChildren<Tilemap>();
        foreach (var tilemap in tilemaps)
        {
            switch (tilemap.gameObject.tag)
            {
                case "decoration1Tilemap":
                    decoration1 = tilemap;
                    break;
                case "decoration2Tilemap":
                    decoration2 = tilemap;
                    break;
                case "groundTilemap":
                    ground = tilemap;
                    break;
                case "frontTilemap":
                    front = tilemap;
                    break;
                case "collisionTilemap":
                    collision = tilemap;
                    break;
                case "minimapTilemap":
                    minMap = tilemap;
                    break;
            }
        }
    }
}
