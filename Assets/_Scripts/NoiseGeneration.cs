using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class NoiseGeneration
{
    public static float[,] GenerateNoiseMap(int width, int height, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        float[,] noiseMap = new float[width, height];

        System.Random numberGenerator = new System.Random(seed);
        
        // Sample each octave from a different position
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = numberGenerator.Next(-100000, 100000) + offset.x;
            float offsetY = numberGenerator.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }
        
        if (scale <= 0)
            scale = 0.0001f;

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float centerWidth = width / 2f;
        float centerHeight = height / 2f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;
                
                for (int i = 0; i < octaves; i++) // Loop through number of octaves
                {
                    // Higher frequency = further apart sample points which causes height to change more quickly
                    float sampleX = (x-centerWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y-centerHeight) / scale * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1; // Replace this with custom perlin noise function
                    noiseHeight += perlinValue * amplitude; // Increase noise height by perlin value of each octave
                    amplitude *= persistance; // Decreases each octave
                    frequency *= lacunarity; // Increases each octave
                }

                // Check whether to update maxNoiseHeight
                if (noiseHeight > maxNoiseHeight)
                    maxNoiseHeight = noiseHeight;
                
                // Check whether to update minNoiseHeight
                else if (noiseHeight < minNoiseHeight)
                    minNoiseHeight = noiseHeight;
                
                noiseMap[x, y] = noiseHeight;

            }
        }

        // Normalize the noise map so all values are between 0 and 1
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }
}
