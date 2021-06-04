using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TerrainChunk
{
  public event System.Action<TerrainChunk, bool> OnVisibilityChanged;

  const float colliderGenerationDstThreshold = 5;

  public Vector2 coord;

  GameObject meshObject;
  Vector2 sampleCenter;
  Bounds bounds;

  MeshRenderer meshRenderer;
  MeshFilter meshFilter;
  MeshCollider meshCollider;

  LODInfo[] detailLevels;
  LODMesh[] lodMeshes;
  int colliderLODIndex;
  float maxViewDst;

  HeightMap heightMap;
  bool heightMapReceived;
  int previousLODIndex = -1;
  bool hasSetCollider;

  HeightMapSettings heightMapSettings;
  MeshSettings meshSettings;

  Transform viewer;

  //Constructor
  public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Transform viewer, Material material)
  {
    this.coord = coord;
    this.heightMapSettings = heightMapSettings;
    this.meshSettings = meshSettings;
    this.detailLevels = detailLevels;
    this.colliderLODIndex = colliderLODIndex;
    this.viewer = viewer;

    sampleCenter = coord * meshSettings.meshWorldSize / meshSettings.meshScale;
    Vector2 position = coord * meshSettings.meshWorldSize;
    bounds = new Bounds(position, Vector2.one * meshSettings.meshWorldSize);

    meshObject = new GameObject("Terrain Chunk");
    meshRenderer = meshObject.AddComponent<MeshRenderer>();
    meshFilter = meshObject.AddComponent<MeshFilter>();
    meshCollider = meshObject.AddComponent<MeshCollider>();
    meshRenderer.material = material;

    meshObject.transform.position = new Vector3(position.x, 0, position.y);
    meshObject.transform.parent = parent;

    SetVisible(false);

    lodMeshes = new LODMesh[detailLevels.Length];
    for (int i = 0; i < detailLevels.Length; i++)
    {
      lodMeshes[i] = new LODMesh(detailLevels[i].lod);
      lodMeshes[i].updateCallback += UpdateTerrainChunk;
      if (i == colliderLODIndex)
      {
        lodMeshes[i].updateCallback += UpdateCollisionMesh;
      }
    }

    maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
  }

  public void Load()
  {
    ThreadedDataRequester.RequestData(() => HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, sampleCenter), OnHeightMapReceived);
  }

  void OnHeightMapReceived(object heightMapObject)
  {
    this.heightMap = (HeightMap)heightMapObject;
    heightMapReceived = true;

    UpdateTerrainChunk();
  }

  Vector3 viewerPosition
  {
    get
    {
      return new Vector2(viewer.position.x, viewer.position.z);
    }
  }

  public void UpdateTerrainChunk()
  {
    if (heightMapReceived)
    {
      float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));

      bool visible = viewerDstFromNearestEdge <= maxViewDst;
      bool wasVisible = IsVisible();

      if (visible)
      {
        int lodIndex = 0;

        for (int i = 0; i < detailLevels.Length - 1; i++)
        {
          if (viewerDstFromNearestEdge > detailLevels[i].visibleDstThreshold)
          {
            lodIndex = i + 1;
          }
          else
          {
            break;
          }
        }

        if (lodIndex != previousLODIndex)
        {
          LODMesh lodMesh = lodMeshes[lodIndex];
          if (lodMesh.hasMesh)
          {
            previousLODIndex = lodIndex;
            meshFilter.mesh = lodMesh.mesh;
          }
          else if (!lodMesh.hasRequestedMesh)
          {
            lodMesh.RequestMesh(heightMap, meshSettings);
          }
        }
      }
      if (wasVisible != visible)
      {
        SetVisible(visible);
        if (OnVisibilityChanged != null)
        {
          OnVisibilityChanged(this, visible);
        }
      }
    }
  }

  public void UpdateCollisionMesh()
  {
    if (!hasSetCollider)
    {
      float sqrtDstFromViewerToEdge = bounds.SqrDistance(viewerPosition);
      if (sqrtDstFromViewerToEdge < colliderGenerationDstThreshold * colliderGenerationDstThreshold)
      {
        if (sqrtDstFromViewerToEdge < detailLevels[colliderLODIndex].sqrVisibleDstThreshold)
        {
          if (!lodMeshes[colliderLODIndex].hasRequestedMesh)
          {
            lodMeshes[colliderLODIndex].RequestMesh(heightMap, meshSettings);
          }
        }

        if (lodMeshes[colliderLODIndex].hasMesh)
        {
          meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
          hasSetCollider = true;
        }
      }
    }
  }

  public void SetVisible(bool visible)
  {
    meshObject.SetActive(visible);
  }

  public bool IsVisible()
  {
    return meshObject.activeSelf;
  }

}

class LODMesh
{

  public Mesh mesh;
  public bool hasRequestedMesh;
  public bool hasMesh;
  int lod;
  public event System.Action updateCallback;

  public LODMesh(int lod)
  {
    this.lod = lod;
  }

  public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings)
  {
    hasRequestedMesh = true;
    ThreadedDataRequester.RequestData(() => MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, lod), OnMeshDataReceived);
  }

  void OnMeshDataReceived(object meshDataObject)
  {
    mesh = ((MeshData)meshDataObject).CreateMesh();
    hasMesh = true;

    updateCallback();
  }
}

