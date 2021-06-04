using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TextureData : UpdateableData
{

  // public void ApplyToMaterial(Material material)
  // {
  //   // material.SetInt("_baseColorCount", baseColors.Length);
  //   // material.SetColorArray("_baseColors", baseColors);
  //   // material.SetFloatArray("_baseStartHeight", baseStartHeight);
  // }

  public void UpdateMeshHeight(Material material, float minHeight, float maxHeight)
  {
    material.SetFloat("_minHeight", minHeight);
    material.SetFloat("_maxHeight", maxHeight);
  }
}
