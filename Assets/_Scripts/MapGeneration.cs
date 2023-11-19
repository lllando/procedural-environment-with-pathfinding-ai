using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGeneration : MonoBehaviour
{

    public enum DrawMethod
    {
        NoiseMap,
        ColourMap,
        Mesh
    }

    public DrawMethod drawMethod;
    
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

    [Range(1, 10)]
    public float meshHeightMultiplier;

    public AnimationCurve meshHeightCurve;

    public bool updateAutomatically;

    public TerrainType[] regions;

    public void GenerateMap()
    {
        float[,] noiseMap = NoiseGeneration.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);

        Color[] colourMap = new Color[mapWidth * mapHeight];
        
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        colourMap[y * mapWidth + x] = regions[i].colour;
                        break;
                    }
                }
            }
        }

        DisplayMap display = FindFirstObjectByType<DisplayMap>();
        
        if (drawMethod == DrawMethod.NoiseMap)
            display.DrawTextureMap(TextureGeneration.TextureFromHeightMap(noiseMap));
        
        else if (drawMethod == DrawMethod.ColourMap)
            display.DrawTextureMap(TextureGeneration.TextureFromColourMap(colourMap, mapWidth, mapHeight));
        
        else if (drawMethod == DrawMethod.Mesh)
            display.DrawMesh(MeshGeneration.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve),
                TextureGeneration.TextureFromColourMap(colourMap, mapWidth, mapHeight));
    }
}

[Serializable]
public struct TerrainType
{
    public string type;
    public float height;
    public Color colour;
}
