using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
  public const float maxViewDst = 450;
  public Transform viewer;
  public Material mapMaterial;

  public static Vector2 viewerPosition;
  static MapGenerator mapGenerator;
  int chunkSize;
  int chunksVisibleInViewDst;

  Dictionary<Vector2, TerrainChunk> terrainChunkDict = new Dictionary<Vector2, TerrainChunk>();
  List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

  void Start()
  {
    mapGenerator = FindObjectOfType<MapGenerator>();
    chunkSize = MapGenerator.mapChunkSize - 1;
    chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);
  }

  void Update()
  {
    viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
    UpdateVisibleChunks();
  }

  void UpdateVisibleChunks()
  {

    for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
    {
      terrainChunksVisibleLastUpdate[i].SetVisible(false);
    }
    terrainChunksVisibleLastUpdate.Clear();

    // (0,0) center, (-1,0) left, (1,0) right, and so on,
    // topleft, topcenter, topright
    // bottomleft, bottomcenter, bottomright
    int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
    int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

    for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
    {
      for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
      {
        Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX
        + xOffset, currentChunkCoordY + yOffset);

        if (terrainChunkDict.ContainsKey(viewedChunkCoord))
        {
          terrainChunkDict[viewedChunkCoord].UpdateTerrainChunk();
          if (terrainChunkDict[viewedChunkCoord].isVisible())
          {
            terrainChunksVisibleLastUpdate.Add(terrainChunkDict[viewedChunkCoord]);
          }
        }
        else
        {
          terrainChunkDict.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform, mapMaterial));
        }
      }
    }
  }

  public class TerrainChunk
  {
    GameObject meshObject;
    Vector2 position;
    Bounds bounds;

    MapData mapData;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    public TerrainChunk(Vector2 coord, int size, Transform parent, Material material)
    {
      position = coord * size;
      bounds = new Bounds(position, Vector2.one * size);
      Vector3 positionV3 = new Vector3(position.x, 0, position.y);

      meshObject = new GameObject("Terrain Chunk");
      meshRenderer = meshObject.AddComponent<MeshRenderer>();
      meshFilter = meshObject.AddComponent<MeshFilter>();
      meshRenderer.material = material;

      meshObject.transform.position = positionV3;
      // meshObject.transform.localScale = Vector3.one * size / 10f;
      meshObject.transform.parent = parent;
      SetVisible(false);

      //new thread is created and OnMapDataRecieved is taken as the callback
      mapGenerator.RequestMapData(OnMapDataRecieved);
    }

    //on callback excuetion after dequeuing
    void OnMapDataRecieved(MapData mapData)
    {
      //new thread is created mapData and OnMeshDataRecived(callback) are the arguements
      mapGenerator.RequestMeshData(mapData, OnMeshDataRecived);
    }

    void OnMeshDataRecived(MeshData meshData)
    {
      //finally a mesh is created
      meshFilter.mesh = meshData.CreateMesh();
    }

    public void UpdateTerrainChunk()
    {
      float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
      bool visible = viewerDstFromNearestEdge <= maxViewDst;
      SetVisible(visible);
    }

    public void SetVisible(bool visible)
    {
      meshObject.SetActive(visible);
    }

    public bool isVisible()
    {
      return meshObject.activeSelf;
    }
  }
}