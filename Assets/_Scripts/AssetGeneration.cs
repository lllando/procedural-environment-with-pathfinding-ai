using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class AssetGeneration : MonoBehaviour
{
    public bool generateAssets;
    public int numberOfAssetsToTrySpawn = 1000;
    public int numberOfNpcsToSpawn = 5;
    private int numberOfNpcsSpawned = 0;

    public List<TerrainAsset> terrainAssets = new List<TerrainAsset>();

    public GameObject npcPrefab;

    public List<GameObject> spawnedNpcs = new List<GameObject>();
    
    private Vector3 objectPosition;

    private TerrainAsset currentTerrainAsset;
    private TerrainAsset previousTerrainAsset;

    private float minHeight;
    private float maxHeight;
    
    private float overlapRadius = 10f;

    private bool isSpawnValid;

    private string parentObjectName = "OBJECT SPAWNER";



    public void GenerateAssets(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve, bool isCalledFromEditor)
    {
        Transform player = FindFirstObjectByType<PlayerController>().transform;
        
        GameObject spawnedAssets = GameObject.Find(parentObjectName);
        if (spawnedAssets != null)
        {
            DestroyImmediate(spawnedAssets);
        }
        
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        GameObject parentAssetSpawner = new GameObject(parentObjectName);

        for (int n = 0; n < numberOfAssetsToTrySpawn; n++)
        {
            for (int i = 0; i < terrainAssets.Count; i++)
            {
                int previousIndex = i - 1;

                if (previousIndex < 0)
                    previousIndex = 0;

                currentTerrainAsset = terrainAssets[i];
                previousTerrainAsset = terrainAssets[previousIndex];
                
                // Set the min/max height for spawning assets. Also don't try to spawn assets in regions that should not have assets in them
                if (currentTerrainAsset.type == TerrainType.Water)
                {
                    minHeight = currentTerrainAsset.height;
                    continue;
                }

                if (currentTerrainAsset.type == TerrainType.Snow)
                {
                    maxHeight = currentTerrainAsset.height;
                    continue;
                }

                // Try spawning
                isSpawnValid = false;
                while (!isSpawnValid)
                {
                    // Initially mark the spawn as valid so we can catch any invalid situations and re-run the loop. If no issues are flagged the asset is successully spawned
                    isSpawnValid = true;

                    int randomX = Random.Range(0, width);
                    int randomY = Random.Range(0, height);

                    float currentHeight = heightMap[randomX, randomY];

                    // Debug.Log($"{currentHeight} : {currentTerrainAsset.height} : {previousTerrainAsset.height}");

                    // Don't spawn asset if the current height is not in the correct bounds of the terrain height values
                    if (currentHeight > currentTerrainAsset.height || currentHeight <= previousTerrainAsset.height)
                        continue;

                    objectPosition = new Vector3(topLeftX + randomX, heightCurve.Evaluate(heightMap[randomX, randomY]) * heightMultiplier, topLeftZ - randomY);

                    Collider[] objectCollisions = Physics.OverlapSphere(objectPosition, overlapRadius);

                    foreach (var col in objectCollisions)
                    {
                        // Only spawn if there is not an asset already near the location
                        if (!col.CompareTag("TerrainAsset"))
                            continue;

                        // Invalid spawn attempt as an asset is already there. Set while loop to false to try and spawn again
                        Debug.Log("Tried to spawn on an existing terrain asset with name: " + col.gameObject.name);
                        isSpawnValid = false;
                        break;
                    }

                    int terrainObjectNum = Random.Range(0, currentTerrainAsset.assets.Count);
                    GameObject terrainObject = currentTerrainAsset.assets[terrainObjectNum];

                    if (terrainObject == null)
                        return;

                    GameObject spawnObj = Instantiate(terrainObject, objectPosition * MapGeneration.meshScale, Quaternion.identity, parentAssetSpawner.transform);
                    spawnObj.transform.localScale *= MapGeneration.meshScale;
                }
            }
        }
        
        // Exit early if being called when not in play mode. Only want mesh + asset generation in this case.
        if (isCalledFromEditor)
            return;
        
        CheckNavMesh checkNavMesh = FindFirstObjectByType<CheckNavMesh>();
        checkNavMesh.CheckAccessibilityAndSpawnObjects(heightMap, minHeight, maxHeight);
        
        // Dont spawn NPCs if in editor mode of game
        numberOfNpcsSpawned = 0;
        spawnedNpcs = new List<GameObject>();
        while (numberOfNpcsSpawned < numberOfNpcsToSpawn)
        {
            int randomX = Random.Range(0, width);
            int randomY = Random.Range(0, height);

            float currentHeight = heightMap[randomX, randomY];

            // Debug.Log($"{currentHeight} : {currentTerrainAsset.height} : {previousTerrainAsset.height}");

            // Don't spawn asset NPC below minimum height
            if (currentHeight < minHeight || currentHeight > maxHeight)
                continue;

            objectPosition = new Vector3(topLeftX + randomX, heightCurve.Evaluate(heightMap[randomX, randomY]) * heightMultiplier, topLeftZ - randomY);

            Collider[] objectCollisions = Physics.OverlapSphere(objectPosition, overlapRadius);

            foreach (var col in objectCollisions)
            {
                // Only spawn if there is not an asset already near the location
                if (col.CompareTag("TerrainAsset") || col.CompareTag("Pickup") || col.CompareTag("NPC"))
                    break;
                
                GameObject npc = Instantiate(npcPrefab, objectPosition * MapGeneration.meshScale, Quaternion.identity);
                spawnedNpcs.Add(npc);
                numberOfNpcsSpawned++;
            }
        }

        foreach (var npc in spawnedNpcs)
        {
            NodeGrid npcNodeGrid = npc.GetComponent<NodeGrid>();
            npcNodeGrid.AddOtherNodeGrids();
        }

        foreach (var npc in spawnedNpcs)
        {
            NpcBehaviour npcBehaviour = npc.GetComponent<NpcBehaviour>();
            npcBehaviour.InitializeNpc();
        }
        
    }
}

[Serializable]
public struct TerrainAsset
{
    public TerrainType type;
    public float height;
    public List<GameObject> assets;
}
