using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;


public class MapGenerator : MonoBehaviour
{
  public enum DrawMode { NoiseMap, FalloffMap, Mesh };
  public DrawMode drawMode;

  public MeshSettings meshSettings;
  public HeightMapSettings heightMapSettings;
  public TextureData textureData;

  public Material terrainMaterial;



  [Range(0, MeshSettings.numSupportedLODs - 1)]
  public int editorPreviewLOD;
  public bool autoUpdate;

  float[,] falloffMap;

  Queue<MapThreadInfo<HeightMap>> heightMapThreadInfoQueue = new Queue<MapThreadInfo<HeightMap>>();
  Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();



  void Start()
  {
    textureData.UpdateMeshHeight(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);
  }

  void OnValuesUpdated()
  {
    if (Application.isEditor)
    {
      DrawMapInEditor();
    }
  }

  // void OnTextureValuesUpdated()
  // {
  //   textureData.ApplyToMaterial(terrainMaterial);
  // }

  //for preview mesh and plane
  public void DrawMapInEditor()
  {
    textureData.UpdateMeshHeight(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

    HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, Vector2.zero);

    MapDisplay display = FindObjectOfType<MapDisplay>();
    //preview on plane
    if (drawMode == DrawMode.NoiseMap)
    {
      display.DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap.values));
    }
    else if (drawMode == DrawMode.FalloffMap)
    {
      display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(meshSettings.numVertsPerLine)));
    }
    //preview on mesh
    else if (drawMode == DrawMode.Mesh)
    {
      display.DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editorPreviewLOD));
    }
  }

  //THREADING

  //OnheightMapRecieved is the callback here
  public void RequestHeightMap(Vector2 centre, Action<HeightMap> callback) // method just starts a new thread
  {
    //ThreadStart type defines what the thread will do 
    //lambda function delegate here is used to negate the writing of another method which then needs to be invoked
    ThreadStart threadStart = delegate
    {
      HeightMapThread(centre, callback);
    };
    new Thread(threadStart).Start();
  }

  void HeightMapThread(Vector2 centre, Action<HeightMap> callback)
  {
    HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, centre); ;
    //cant call the callback herelike(callback(heightMap)) because this operation runs on the thread and we only want the thread to generate data so we enqueue it in a queue  
    //locking the queue so only one thread can access at a time
    lock (heightMapThreadInfoQueue)
    {
      heightMapThreadInfoQueue.Enqueue(new MapThreadInfo<HeightMap>(callback, heightMap));
    }
  }

  public void RequestMeshData(HeightMap heightMap, int lod, Action<MeshData> callback)
  {
    ThreadStart threadStart = delegate
    {
      MeshDataThread(heightMap, lod, callback);
    };
    new Thread(threadStart).Start();
  }

  void MeshDataThread(HeightMap heightMap, int lod, Action<MeshData> callback)
  {
    MeshData meshData = MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, lod);
    lock (meshDataThreadInfoQueue)
    {
      meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
    }
  }

  void Update()
  {
    //In the unity's main thread we check if the both the heightMapThreadInfoQueue and heightMapThreadInfoQueue is > 0, if yes, we then dequeue them from their respective queue and 
    // here we excute the respective callback with their respective parameter(heightMap or MeshData accordingly)

    if (heightMapThreadInfoQueue.Count > 0)
    {
      for (int i = 0; i < heightMapThreadInfoQueue.Count; i++)
      {
        lock (heightMapThreadInfoQueue)
        {
          MapThreadInfo<HeightMap> threadInfo = heightMapThreadInfoQueue.Dequeue();
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

  float[,] GenerateFalloffMap(float[,] noiseMap)
  {
    if (falloffMap == null)
    {
      falloffMap = FalloffGenerator.GenerateFalloffMap(meshSettings.numVertsPerLine);
    }

    for (int y = 0; y < meshSettings.numVertsPerLine + 2; y++)
    {
      for (int x = 0; x < meshSettings.numVertsPerLine + 2; x++)
      {
        noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
      }
    }

    return noiseMap;
  }

  void OnValidate()
  {
    if (meshSettings != null)
    {
      meshSettings.OnValuesUpdated -= OnValuesUpdated;
      meshSettings.OnValuesUpdated += OnValuesUpdated;
    }
    if (heightMapSettings != null)
    {
      heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
      heightMapSettings.OnValuesUpdated += OnValuesUpdated;
    }
    // if (textureData != null)
    // {
    //   heightMapSettings.OnValuesUpdated -= OnTextureValuesUpdated;
    //   heightMapSettings.OnValuesUpdated += OnTextureValuesUpdated;
    // }
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
