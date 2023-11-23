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
    public bool generateAssets = true;
    public int numberOfAssetsToTrySpawn = 1000;

    public List<TerrainAsset> terrainAssets = new List<TerrainAsset>();

    private Vector3 objectPosition;

    private TerrainAsset currentTerrainAsset;
    private TerrainAsset previousTerrainAsset;
    
    private float overlapRadius = 10f;

    private bool isSpawnValid;



    public void GenerateAssets(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve)
    {
        GameObject spawnedAssets = GameObject.Find("PARENT SPAWNER");
        if (spawnedAssets != null)
        {
            DestroyImmediate(spawnedAssets);
        }
        
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        GameObject parentSpawner = new GameObject("PARENT SPAWNER");

        for (int n = 0; n < numberOfAssetsToTrySpawn; n++)
        {
            for (int i = 0; i < terrainAssets.Count; i++)
            {
                int previousIndex = i - 1;

                if (previousIndex < 0)
                    previousIndex = 0;

                currentTerrainAsset = terrainAssets[i];
                previousTerrainAsset = terrainAssets[previousIndex];


                // Don't try to spawn in regions that should not have assets in them
                if (currentTerrainAsset.type == TerrainType.Water || currentTerrainAsset.type == TerrainType.Snow)
                {
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

                    objectPosition = new Vector3(topLeftX + randomX,
                        heightCurve.Evaluate(heightMap[randomX, randomY]) * heightMultiplier, topLeftZ - randomY);

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

                    GameObject spawnObj = Instantiate(terrainObject, objectPosition * MapGeneration.meshScale,
                        Quaternion.identity, parentSpawner.transform);
                    spawnObj.transform.localScale *= MapGeneration.meshScale;
                }
            }
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
