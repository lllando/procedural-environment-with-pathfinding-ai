using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AssetGeneration : MonoBehaviour
{
    public List<TerrainAssets> terrainAssets = new List<TerrainAssets>();
    private int meshScale;
    
    private MeshData meshData;
    
    [SerializeField] private MapGeneration mapGeneration;
    public void GenerateAssets(MeshData mData, int scale)
    {
        meshData = mData;
        meshScale = scale;
    }

    void OnDrawGizmosSelected()
    {
        if (!mapGeneration.assetGeneration)
            return;
        
        foreach (var v in meshData.vertices)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(v * meshScale, 1f);
        }
    }
}

[Serializable]
public struct TerrainAssets
{
    public TerrainType type;
    public List<GameObject> assets;
}
