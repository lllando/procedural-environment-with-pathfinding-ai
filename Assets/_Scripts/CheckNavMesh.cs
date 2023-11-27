using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class CheckNavMesh : MonoBehaviour
{
    private NavMeshSurface navMeshSurface;
    
    public Transform player;
    private Vector3 targetNavMeshPosition;

    public List<GameObject> objectsToSpawnUsingNavmesh = new List<GameObject>();

    private int numberOfNavMeshSpawnedObjects = 20;
    private int spawnedCount = 0;
    
    private float range = 30f;
    private float overlapRadius = 1f;
    private float checkRange = 25f; // Maximum distance to search for a NavMesh point

    public MapGeneration mapGeneration;

    private List<NavMeshPath> validPathsToPlayer = new List<NavMeshPath>();
    
    private string parentObjectName = "NAVMESH OBJECT SPAWNER";

    public GameObject testObject;

    [HideInInspector]
    public List<GameObject> spawnedPickupObjects;
    
    
    private void Start()
    {
        // CheckAccessibilityAndSpawnObjects(mapGeneration.noiseMap);
    }
    
    public void CheckAccessibilityAndSpawnObjects(float[,] heightMap, float minHeight, float maxHeight)
    {
        navMeshSurface = GetComponent<NavMeshSurface>();
        navMeshSurface.BuildNavMesh();
        
        spawnedPickupObjects = new List<GameObject>();
        
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        NavMeshHit playerHit;
        bool isOnNavMesh = NavMesh.SamplePosition(player.position, out playerHit, checkRange, NavMesh.AllAreas);

        if (isOnNavMesh)
        {
            Debug.Log(player.name + " is on the NavMesh at " + playerHit.position);
        }
        else
        {
            Debug.LogError(player.name + " is NOT on the NavMesh.");
        }

        player.transform.position = playerHit.position;

        GameObject spawnedNavMeshAssetSpawner = GameObject.Find(parentObjectName);
        if (spawnedNavMeshAssetSpawner != null)
        {
            DestroyImmediate(spawnedNavMeshAssetSpawner);
        }
        
        GameObject navMeshAssetSpawner = new GameObject(parentObjectName);

        while (spawnedCount < numberOfNavMeshSpawnedObjects)
        {
            int randomObject = Random.Range(0, objectsToSpawnUsingNavmesh.Count);
            GameObject obj = objectsToSpawnUsingNavmesh[randomObject];

            Vector3 randomDir = Random.insideUnitSphere * range;
            randomDir += player.transform.position;
            
            Debug.Log("Range: " + range + ", Random Direction: " + randomDir);
            
            bool isOnNavMesh2 = NavMesh.SamplePosition(randomDir, out NavMeshHit hit, checkRange, NavMesh.AllAreas);

            if (isOnNavMesh2)
            {
                Debug.Log(obj.name + " is on the NavMesh at " + hit.position + " with a minHeight of " + minHeight);
                Vector3 finalPos = hit.position;
                // Instantiate(testObject, hit.position, Quaternion.identity);
                float newMinHeight = minHeight + 0.1f;

                if (finalPos.y < newMinHeight || finalPos.y > maxHeight)
                    continue;
                
                NavMeshPath pathToPlayer = new NavMeshPath();
                if (IsAccessibleByPlayer(playerHit.position, finalPos, pathToPlayer))
                {
                    Debug.Log("Valid path found");


                    Collider[] objectCollisions = Physics.OverlapSphere(finalPos, overlapRadius);

                    bool shouldSpawn = false;
                    foreach (var col in objectCollisions)
                    {
                        // Only spawn if there is not an asset near the location
                        if (!col.CompareTag("TerrainAsset") && !col.CompareTag("Pickup"))
                            shouldSpawn = true;
                    }

                    if (shouldSpawn)
                    {
                        GameObject spawnedObject = Instantiate(obj, finalPos, Quaternion.identity, navMeshAssetSpawner.transform);
                        spawnedPickupObjects.Add(spawnedObject);
                        Debug.Log($"{spawnedObject} spawned at {finalPos}");
                        validPathsToPlayer.Add(pathToPlayer);
                        spawnedCount++;
                    }
                }
                else
                {
                    Debug.Log("Invalid path");
                }
            }
        }
    }

    public bool IsAccessibleByPlayer(Vector3 sourcePosition, Vector3 targetPosition, NavMeshPath path)
    {
        if (NavMesh.CalculatePath(sourcePosition, targetPosition, NavMesh.AllAreas, path))
        {
            Debug.Log("path corners size = " + path.corners.Length);
            return path.status == NavMeshPathStatus.PathComplete;
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        foreach (var p in validPathsToPlayer)
        {
            DrawPath(p, Color.yellow);
        }
    }

    private void DrawPath(NavMeshPath path, Color color)
    {
        if (path != null)
        {
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                Gizmos.color = color;
                for (int i = 0; i < path.corners.Length - 1; i++)
                {
                    Gizmos.DrawLine(path.corners[i], path.corners[i + 1]);
                }
            }
        }
    }
}
