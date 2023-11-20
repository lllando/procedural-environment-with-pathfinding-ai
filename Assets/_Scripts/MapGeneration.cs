using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGeneration : MonoBehaviour
{

    // DrawMethod selects what to render on screen
    public enum DrawMethod
    {
        NoiseMap,
        ColourMap,
        Mesh
    }
    
    public DrawMethod drawMethod;
    
    // SmoothingType changes the way that the mesh will be rendered. GradientMode and FilterMode values are changed based on the SmoothingType set by the user
    public enum SmoothingType
    {
        Fixed, // (Fixed GradientMode + Point FilterMode)
        Gradient, // (Blend GradientMode + Point FilterMode)
        Smooth // (Blend GradientMode + Bilinear FilterMode)
    }

    public SmoothingType smoothingType;
    
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

    public Gradient heightGradient;
    private FilterMode filterMode = FilterMode.Point;

    private Dictionary<TerrainType, Terrain> terrainGradientColours = new Dictionary<TerrainType, Terrain>();
    
    private void Awake()
    {
        InitializeTerrainGradientColours();
    }

    public void GenerateMap()
    {
        float[,] noiseMap = NoiseGeneration.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);

        Color[] colourMap = new Color[mapWidth * mapHeight];

        heightGradient.mode = GradientMode.Fixed; // Reset the gradient mode
        filterMode = FilterMode.Point; // Reset the filter mode

        if (smoothingType != SmoothingType.Fixed) // All SmoothingTypes other than Fixed require a blended gradient
        {
            heightGradient.mode = GradientMode.Blend; // Update the gradient mode to Blend
            
            if (smoothingType == SmoothingType.Smooth) 
                filterMode = FilterMode.Bilinear; // Set filter mode to bilinear as SmoothingType is set to smooth (Blend + Bilinear)
        }
        
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float currentHeight = noiseMap[x, y];

                foreach (var t in terrainGradientColours)
                {
                    if (currentHeight <= t.Value.height)
                    {
                        Color heightColour = heightGradient.Evaluate(currentHeight);
                        colourMap[y * mapWidth + x] = heightColour;
                    }
                }
            }
        }

        DisplayMap display = FindFirstObjectByType<DisplayMap>();
        
        // Different logic based on what DrawMethod is set by the user in the inspector
        if (drawMethod == DrawMethod.NoiseMap)
            display.DrawTextureMap(TextureGeneration.TextureFromHeightMap(noiseMap));
        
        else if (drawMethod == DrawMethod.ColourMap)
            display.DrawTextureMap(TextureGeneration.TextureFromColourMap(colourMap, mapWidth, mapHeight));
        
        else if (drawMethod == DrawMethod.Mesh)
        {
            // Draw the mesh using a specific FilterMode setting that smooths the gradient
            display.DrawMesh(MeshGeneration.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve), 
                TextureGeneration.TextureFromColourMap(colourMap, mapWidth, mapHeight, filterMode));
        }
    }

    public void InitializeTerrainGradientColours()
    {
        GradientColorKey[] colourKeys = heightGradient.colorKeys;
        TerrainType[] terrainTypes = (TerrainType[]) Enum.GetValues(typeof(TerrainType));
        
        for (int i = 0; i < colourKeys.Length; i++)
        {
            if (i > terrainTypes.Length) // There are no more terrain types. Dont try to add more elements to terrainGradientColours.
                return;
            
            decimal heightValue = Decimal.Round((decimal)colourKeys[i].time, 2); // Round the time value to 2 decimal place so it can be used for height

            if (!terrainGradientColours.ContainsKey(terrainTypes[i])) // Don't add a new terrain if one already exists with that key
            {
                Terrain terrain = new Terrain(terrainTypes[i], colourKeys[i].color, (float)heightValue); // Create a new terrain object
                terrainGradientColours.Add(terrainTypes[i], terrain); // Add the terrain object and terrain type to dictionary so it can be used later
            }
        }
    }
}

public enum TerrainType
{
    Water,
    Sand,
    LightGrass,
    DarkGrass,
    LightRock,
    DarkRock,
    Snow
}

[Serializable]
public struct Terrain
{
    public TerrainType type;
    public Color colour;
    public float height;
    public int pathWeight;

    public Terrain(TerrainType t, Color c, float h, int pw = 1)
    {
        type = t;
        colour = c;
        height = h;
        pathWeight = pw;
    }
}

/*
[Serializable]
public struct TerrainType
{
    public string type;
    public float height;
    public Color colour;
}
*/
