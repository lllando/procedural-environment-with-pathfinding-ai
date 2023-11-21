using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class AssetGeneration : MonoBehaviour
{
    public List<TerrainAsset> terrainAssets = new List<TerrainAsset>();
    
    [SerializeField] private MapGeneration mapGeneration;
    public void GenerateAssets(float [,] heightMap, float heightMultiplier, AnimationCurve heightCurve, int scale)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float currentHeight = heightMap[x, y];

                for (int i = 0; i < terrainAssets.Count; i++)
                {
                    int previousIndex = i - 1;
                    
                    if (previousIndex < 0)
                        previousIndex = 0;

                    TerrainAsset currentTerrainAsset = terrainAssets[i];
                    TerrainAsset previousTerrainAsset = terrainAssets[previousIndex];

                    if (currentTerrainAsset.type == TerrainType.Water || currentTerrainAsset.type == TerrainType.Snow)
                        continue;
                    
                    if (currentHeight <= currentTerrainAsset.height && currentHeight >= previousTerrainAsset.height)
                    {
                        int terrainObjectNum = Random.Range(0, currentTerrainAsset.assets.Count);
                        GameObject terrainObject = currentTerrainAsset.assets[terrainObjectNum];

                        if (terrainObject != null)
                        {
                            Vector3 objectPosition = new Vector3(topLeftX + x, heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier, topLeftZ - y);
                        
                            GameObject spawnObj = Instantiate(terrainObject, objectPosition * scale, Quaternion.identity);
                        }
                    }
                }
                foreach (var t in terrainAssets)
                {
                    if (currentHeight <= t.height)
                    {
                        
                    }
                }
            }
        }
        
        /*
        foreach (var v in mData.vertices)
        {
            for (int i = 0; i < terrainAssets.Count; i++)
            {
                int previousIndex = i - 1;
                if (previousIndex < 0)
                    previousIndex = 0;
                
                
                // Spawns a debug object at every vertex point
                TerrainAsset t = terrainAssets[0];
                GameObject spawnObj = Instantiate(t.assets[0], v * scale, Quaternion.identity);
                
          
                TerrainAsset t = terrainAssets[i];
                TerrainAsset previousTerrain = terrainAssets[previousIndex];

                if (t.type == TerrainType.Water || t.type == TerrainType.Snow)
                    continue;


  
                if (v.y <= previousterrain.height * scale)
                    continue;

                
                if (v.y <= t.height * scale && v.y > previousTerrain.height * scale)
                {
                    int terrainObjectNum = Random.Range(0, t.assets.Count);
                    GameObject terrainObject = t.assets[terrainObjectNum];

                    if (terrainObject != null)
                    {
                        // Debug.Log(t.type + " spawned at " + v * scale);
                        GameObject spawnObj = Instantiate(terrainObject, v * scale, Quaternion.identity);
                    }
                }
            }
            */
    }
    

    void OnDrawGizmosSelected()
    {
        /*
        if (!mapGeneration.assetGeneration)
            return;
        
        foreach (var v in meshData.vertices)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(v * meshScale, 1f);
        }
        */
    }
}

[Serializable]
public struct TerrainAsset
{
    public TerrainType type;
    public float height;
    public List<GameObject> assets;
}
