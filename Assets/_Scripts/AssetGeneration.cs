using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AssetGeneration : MonoBehaviour
{
    public List<TerrainAssets> terrainAssets = new List<TerrainAssets>();
    
    private MeshData meshData;
    public void GenerateAssets(MeshData mData)
    {
        meshData = mData;
    }

    void OnDrawGizmosSelected()
    {
        foreach (var v in meshData.vertices)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(v, 1f);
        }
    }
}

[Serializable]
public struct TerrainAssets
{
    public TerrainType type;
    public List<GameObject> assets;
}
