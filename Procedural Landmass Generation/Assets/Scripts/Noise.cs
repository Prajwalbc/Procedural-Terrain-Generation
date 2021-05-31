using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
  public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
  {

    float[,] noiseMap = new float[mapWidth, mapHeight];

    //PseudoRandomNumberGenerator
    System.Random prng = new System.Random(seed);
    //used for offsettting it, to provide continueous change on either axises
    Vector2[] octaveOffsets = new Vector2[octaves];
    for (int i = 0; i < octaves; i++)
    {
      float offsetX = prng.Next(-100000, 100000) + offset.x;
      float offsetY = prng.Next(-100000, 100000) + offset.y;
      octaveOffsets[i] = new Vector2(offsetX, offsetY);
    }

    if (scale <= 0)
    {
      scale = 0.0001f;
    }

    //dont want to set it to zero (default sets it to zero)
    float maxNoiseHeight = float.MinValue;
    float minNoiseHeight = float.MaxValue;

    //scale it in the center
    float halfWidth = mapWidth * 0.5f;
    float halfHeight = mapHeight * 0.5f;

    for (int y = 0; y < mapHeight; y++)
    {
      for (int x = 0; x < mapWidth; x++)
      {
        float amplitude = 1;
        float frequency = 1;
        float noiseHeight = 0;
        for (int i = 0; i < octaves; i++)
        {
          float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x * frequency;
          float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y * frequency;

          // to also get negative values
          float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
          noiseHeight += perlinValue * amplitude;

          amplitude *= persistance;
          frequency *= lacunarity;
        }
        //used for normalizing
        if (noiseHeight > maxNoiseHeight)
        {
          maxNoiseHeight = noiseHeight;
        }
        else if (noiseHeight < minNoiseHeight)
        {
          minNoiseHeight = noiseHeight;
        }
        noiseMap[x, y] = noiseHeight;
      }
    }

    //normalizing the values
    for (int y = 0; y < mapHeight; y++)
    {
      for (int x = 0; x < mapWidth; x++)
      {
        //gives values bw 0 to 1
        noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
      }
    }
    return noiseMap;
  }
}
