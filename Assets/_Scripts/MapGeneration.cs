using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGeneration : MonoBehaviour
{
    [Range(1, 1000)]
    public int mapWidth, mapHeight;
    
    [Range(1, 100)]
    public float noiseScale;

    [Range(0, 10)]
    public int octaves;
    
    [Range(0,1)]
    public float persistance;
    
    [Range(1, 10)]
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public bool updateAutomatically;

    public TerrainType[] regions;

    public void GenerateMap()
    {
        float[,] noiseMap = NoiseGeneration.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);
        
        DisplayMap display = FindFirstObjectByType<DisplayMap>();
        display.DrawNoiseMap(noiseMap);
    }
}

[Serializable]
public struct TerrainType
{
    public string type;
    public float height;
    public Color colour;
}
