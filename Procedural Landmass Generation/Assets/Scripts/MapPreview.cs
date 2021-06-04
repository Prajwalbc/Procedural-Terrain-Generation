using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPreview : MonoBehaviour
{
  //plane
  public Renderer planeTextureRender;

  //mesh
  public MeshFilter meshFilter;
  public MeshRenderer meshRenderer;

  public enum DrawMode { NoiseMap, FalloffMap, Mesh };
  public DrawMode drawMode;

  public MeshSettings meshSettings;
  public HeightMapSettings heightMapSettings;
  public TextureData textureData;

  public Material terrainMaterial;

  float[,] falloffMap;

  [Range(0, MeshSettings.numSupportedLODs - 1)]
  public int editorPreviewLOD;
  public bool autoUpdate;

  //preview of mesh and plane
  public void DrawMapInEditor()
  {
    textureData.UpdateMeshHeight(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

    HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, Vector2.zero);

    //preview on plane
    if (drawMode == DrawMode.NoiseMap)
    {
      DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap));
    }
    else if (drawMode == DrawMode.FalloffMap)
    {
      DrawTexture(TextureGenerator.TextureFromHeightMap(new HeightMap(FalloffGenerator.GenerateFalloffMap(meshSettings.numVertsPerLine), 0, 1)));
    }
    //preview on mesh
    else if (drawMode == DrawMode.Mesh)
    {
      DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editorPreviewLOD));
    }
  }

  //texture preview on plane
  public void DrawTexture(Texture2D texture)
  {
    // planeTextureRender.sharedMaterial.SetTexture("_MainTex", texture);
    planeTextureRender.sharedMaterial.mainTexture = texture;
    planeTextureRender.transform.localScale = new Vector3(texture.width, 1, texture.height) / 10f;

    planeTextureRender.gameObject.SetActive(true);
    meshFilter.gameObject.SetActive(false);
  }

  //texture preview on mesh
  public void DrawMesh(MeshData meshData)
  {
    meshFilter.sharedMesh = meshData.CreateMesh();

    planeTextureRender.gameObject.SetActive(false);
    meshFilter.gameObject.SetActive(true);
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

  void OnValuesUpdated()
  {
    if (Application.isEditor)
    {
      DrawMapInEditor();
    }
  }

  //not needed as shader graph takes care of it
  // void OnTextureValuesUpdated()
  // {
  //   textureData.ApplyToMaterial(terrainMaterial);
  // }

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
}
