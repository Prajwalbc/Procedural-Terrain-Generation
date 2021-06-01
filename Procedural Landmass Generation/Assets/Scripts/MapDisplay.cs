using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
  //plane
  public Renderer PlaneTextureRender;

  //mesh
  public MeshFilter meshFilter;
  public MeshRenderer meshRenderer;

  //texture preview on plane
  public void DrawTexture(Texture2D texture)
  {
    PlaneTextureRender.sharedMaterial.mainTexture = texture;
    // PlaneTextureRender.sharedMaterial.SetTexture("_MainTex", texture);
    PlaneTextureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
  }

  //preview of texture with meshData on mesh
  public void DrawMesh(MeshData meshData, Texture2D texture)
  {
    meshFilter.sharedMesh = meshData.CreateMesh();
    meshRenderer.sharedMaterial.mainTexture = texture;
    // meshRenderer.sharedMaterial.SetTexture("_BaseMap", texture);
  }
}
