using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class CheckNavMesh : MonoBehaviour
{
    public NavMeshSurface navMeshSurface;
    
    public Transform player;
    private Vector3 targetNavMeshPosition;

    public List<GameObject> objectsToSpawnUsingNavmesh = new List<GameObject>();
    
    private int numberOfNavMeshSpawnedObjects = 30;
    private int spawnedCount = 0;
    
    private float range = 30f;
    private float overlapRadius = 1f;
    private float checkRange = 25f; // Maximum distance to search for a NavMesh point

    public MapGeneration mapGeneration;

    private List<NavMeshPath> validPathsToPlayer = new List<NavMeshPath>();
    
    private string parentObjectName = "NAVMESH OBJECT SPAWNER";

    public GameObject testObject;

    public List<GameObject> spawnedPickupObjects;
    
    
    private void Start()
    {
        // CheckAccessibilityAndSpawnObjects(mapGeneration.noiseMap);
    }

    public void BuildNavMeshBasedOnTerrain()
    {
        navMeshSurface.BuildNavMesh();
    }

    public void CheckAccessibilityAndSpawnObjects(float[,] heightMap)
    {
        BuildNavMeshBasedOnTerrain();
        
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
                Debug.Log(obj.name + " is on the NavMesh at " + hit.position);
                Vector3 finalPos = hit.position;

                NavMeshPath pathToPlayer = new NavMeshPath();
                if (IsAccessibleByPlayer(playerHit.position, finalPos, pathToPlayer))
                {
                    Debug.Log("Valid path found");


                    Collider[] objectCollisions = Physics.OverlapSphere(finalPos, overlapRadius);

                    bool shouldSpawn = true;
                    foreach (var col in objectCollisions)
                    {
                        // Only spawn if there is not an asset near the location
                        if (col.CompareTag("TerrainAsset") || col.CompareTag("Pickup"))
                            shouldSpawn = false;
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
            }
        }
    }

    private bool IsAccessibleByPlayer(Vector3 sourcePosition, Vector3 targetPosition, NavMeshPath path)
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
            Gizmos.color = color;
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                Gizmos.DrawLine(path.corners[i], path.corners[i + 1]);
            }
        }
    }
}
