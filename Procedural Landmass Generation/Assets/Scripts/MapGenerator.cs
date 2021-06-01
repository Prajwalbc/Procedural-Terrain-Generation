using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;


public class MapGenerator : MonoBehaviour
{

  public enum DrawMode { NoiseMap, ColorMap, Mesh };
  public DrawMode drawMode;

  public Noise.NormalizeMode normalizeMode;

  public const int mapChunkSize = 241;
  [Range(0, 6)]
  public int editorPreviewLOD;
  public float noiseScale;

  public int octaves;
  [Range(0, 1)] //sets persistence as slider
  public float persistance;
  public float lacunarity;

  public int seed;
  public Vector2 offset;

  public float meshHeightMultiplier;
  public AnimationCurve meshHeightCurve;

  public bool autoUpdate;

  public TerrainType[] regions;

  Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
  Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

  //for preview mesh and plane
  public void DrawMapInEditor()
  {
    MapData mapData = GenerateMapData(Vector2.zero);

    MapDisplay display = FindObjectOfType<MapDisplay>();
    //preview on plane
    if (drawMode == DrawMode.NoiseMap)
    {
      display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
    }
    //preview on plane
    else if (drawMode == DrawMode.ColorMap)
    {
      display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
    }
    //preview on 
    else if (drawMode == DrawMode.Mesh)
    {
      display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLOD), TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
    }
  }

  //Threading
  //OnMapDataRecieved is the callback here
  public void RequestMapData(Vector2 centre, Action<MapData> callback) // method just starts a new thread
  {
    //ThreadStart type defines what the thread will do 
    //lambda function delegate here is used to negate the writing of another method which then needs to be invoked
    ThreadStart threadStart = delegate
    {
      MapDataThread(centre, callback);
    };
    new Thread(threadStart).Start();
  }

  void MapDataThread(Vector2 centre, Action<MapData> callback)
  {
    MapData mapData = GenerateMapData(centre);
    //cant call the callback herelike(callback(mapData)) because this operation runs on the thread and we only want the thread to generate data so we enqueue it in a queue  
    //locking the queue so only one thread can access at a time
    lock (mapDataThreadInfoQueue)
    {
      mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
    }
  }

  public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
  {
    ThreadStart threadStart = delegate
    {
      MeshDataThread(mapData, lod, callback);
    };
    new Thread(threadStart).Start();
  }

  void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
  {
    MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod);
    lock (meshDataThreadInfoQueue)
    {
      meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
    }
  }

  void Update()
  {
    //In the unity's main thread we check if the both the mapDataThreadInfoQueue and mapDataThreadInfoQueue is > 0, if yes, we then dequeue them from their respective queue and 
    // here we excute the respective callback with their respective parameter(mapData or MeshData accordingly)

    if (mapDataThreadInfoQueue.Count > 0)
    {
      for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
      {
        lock (mapDataThreadInfoQueue)
        {
          MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
          threadInfo.callback(threadInfo.parameter);
        }
      }
    }

    if (meshDataThreadInfoQueue.Count > 0)
    {
      for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
      {
        lock (meshDataThreadInfoQueue)
        {
          MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
          threadInfo.callback(threadInfo.parameter);
        }
      }
    }
  }

  MapData GenerateMapData(Vector2 centre)
  {
    float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, centre + offset, normalizeMode);

    Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
    for (int y = 0; y < mapChunkSize; y++)
    {
      for (int x = 0; x < mapChunkSize; x++)
      {
        float currentHeight = noiseMap[x, y];
        for (int i = 0; i < regions.Length; i++)
        {
          if (currentHeight >= regions[i].height)
          {
            colorMap[y * mapChunkSize + x] = regions[i].color;
          }
          else
          {
            break;
          }
        }
      }
    }
    return new MapData(noiseMap, colorMap);
  }

  void OnValidate()
  {
    if (lacunarity < 1)
    {
      lacunarity = 1;
    }
    if (octaves < 0)
    {
      octaves = 0;
    }
  }

  struct MapThreadInfo<T>
  {
    public readonly Action<T> callback;
    public readonly T parameter;

    public MapThreadInfo(Action<T> callback, T parameter)
    {
      this.callback = callback;
      this.parameter = parameter;
    }
  }
}

[System.Serializable]
public struct TerrainType
{
  public string name;
  public float height;
  public Color color;
}

public struct MapData
{
  public readonly float[,] heightMap;
  public readonly Color[] colorMap;

  public MapData(float[,] heightMap, Color[] colorMap)
  {
    this.heightMap = heightMap;
    this.colorMap = colorMap;
  }
}