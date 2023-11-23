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

    public Transform navMeshSpawnedAsset;
    
    public Transform player;
    public Transform target;
    
    private Vector3 objectPosition;    
    
    private int numberOfNavMeshSpawnedAssets = 1;
    public float range = 10f;
    private float maxDistance = 5.0f; // Maximum distance to search for a NavMesh point


    private NavMeshPath pathToTarget;
    
    public void CheckAccessibilityAndSpawnObjects(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;
        
        /*
        for (int j = 0; j < numberOfNavMeshSpawnedAssets; j++)
        {
            int randomX = Random.Range(0, width);
            int randomY = Random.Range(0, height);
            Debug.Log("spawn at " + randomX + ", " + randomY);

            float currentHeight = heightMap[randomX, randomY];
            
            objectPosition = new Vector3(topLeftX + randomX,
                heightCurve.Evaluate(heightMap[randomX, randomY]) * heightMultiplier, topLeftZ - randomY);

            pathToPlayer = new NavMeshPath();
            if (NavMesh.CalculatePath(player.position, target.position, NavMesh.AllAreas, pathToPlayer))
            {
                Debug.Log("Valid path found");
                Instantiate(navMeshSpawnedAsset, objectPosition, Quaternion.identity);
            }
            else
            {
                Debug.Log("Not spawning. No valid path found");
            }
            Debug.Log("path corners size = " + pathToPlayer.corners.Length);
        }
        */
        
        NavMeshHit playerHit;
        bool isOnNavMesh = NavMesh.SamplePosition(player.position, out playerHit, maxDistance, NavMesh.AllAreas);

        if (isOnNavMesh)
        {
            Debug.Log(player.name + " is on the NavMesh at " + playerHit.position);
            // Instantiate(navMeshSpawnedAsset, hit.position, Quaternion.identity);

            // Optionally, you can move the object to the nearest NavMesh position
            // objectToCheck.position = hit.position;
        }
        else
        {
            Debug.Log(player.name + " is NOT on the NavMesh.");
        }
        
        NavMeshHit targetHit;
        bool isOnNavMesh2 = NavMesh.SamplePosition(target.position, out targetHit, maxDistance, NavMesh.AllAreas);

        if (isOnNavMesh2)
        {
            Debug.Log(target.name + " is on the NavMesh at " + targetHit.position);
            // Instantiate(navMeshSpawnedAsset, hit2.position, Quaternion.identity);

            // Optionally, you can move the object to the nearest NavMesh position
            // objectToCheck.position = hit.position;
        }
        else
        {
            Debug.Log(target.name + " is NOT on the NavMesh.");
        }
        
        

        pathToTarget = new NavMeshPath();
        if (IsAccessibleByPlayer(playerHit.position, targetHit.position, pathToTarget))
        {
            Debug.Log("Valid path found");
        }

        else
        {
            Debug.Log("No valid path found");
        }
    }

    private bool IsAccessibleByPlayer(Vector3 sourcePosition, Vector3 targetPosition, NavMeshPath path)
    {
        if (NavMesh.CalculatePath(sourcePosition, targetPosition, NavMesh.AllAreas, path))
        {
            Debug.Log("path corners size = " + pathToTarget.corners.Length);
            return path.status == NavMeshPathStatus.PathComplete;
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        // DrawPath(pathToTarget, Color.red);
        DrawPath(pathToTarget, Color.yellow);
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
