using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
  public Renderer textureRenderer;
  public MeshFilter meshfilterer;
  public MeshRenderer meshRenderer;

  public void DrawTexture(Texture2D texture)
  {
    textureRenderer.sharedMaterial.SetTexture("_MainTex", texture);
    textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
  }

  public void DrawMesh(MeshData meshData, Texture2D texture)
  {
    meshfilterer.sharedMesh = meshData.CreateMesh();
    meshRenderer.sharedMaterial.SetTexture("_BaseMap", texture);
  }
}
