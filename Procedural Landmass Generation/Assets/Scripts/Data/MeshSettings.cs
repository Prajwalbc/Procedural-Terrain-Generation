using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class MeshSettings : UpdateableData
{
  public const int numSupportedLODs = 5;
  public const int numSupportedChunkSizes = 9;
  public const int numSupportedFlatshadedChunkSizes = 3;
  public static readonly int[] supportedChunkSizes = { 48, 72, 96, 120, 144, 168, 192, 216, 240 };

  public float meshScale = 2.5f;
  public bool useFlatShading;

  [Range(0, numSupportedChunkSizes - 1)]
  public int chunkSizeIndex;
  [Range(0, numSupportedFlatshadedChunkSizes - 1)]
  public int flatshadedChunkSizeIndex;

  //num verts per line of mesh rendered at LOD = 0. Includes the 2 extra verts that are excluded from the final mesh, but are used for calculating normals
  public int numVertsPerLine
  {
    get
    {
      return supportedChunkSizes[(useFlatShading) ? flatshadedChunkSizeIndex : chunkSizeIndex] + 1;
    }
  }

  public float meshWorldSize
  {
    get
    {
      //width is numVertsPerLine - 1 but must also exclude 2 extra verts so -3
      return (numVertsPerLine - 3) * meshScale;
    }
  }
}
