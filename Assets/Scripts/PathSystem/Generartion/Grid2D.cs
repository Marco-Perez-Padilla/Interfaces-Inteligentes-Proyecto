using System.Collections.Generic;
using UnityEngine;

/**
 * @file Grid2D.cs
 * @brief Grid 2D para generación topológica.
 */
public class Grid2D
{
  public readonly List<Vector3> points = new();
  private readonly float spacing;

  public Grid2D(Vector3 origin, int width, int height, float spacing)
  {
    this.spacing = spacing;

    for (int x = 0; x < width; x++)
    {
      for (int z = 0; z < height; z++)
      {
        points.Add(origin + new Vector3(x * spacing, 0f, z * spacing));
      }
    }
  }

  /** Devuelve vecinos ortogonales en X/Z. */
  public List<Vector3> GetNeighbours(Vector3 p)
  {
    List<Vector3> result = new();

    foreach (var q in points)
    {
      Vector3 d = q - p;
      bool x = Mathf.Abs(Mathf.Abs(d.x) - spacing) < 0.01f && Mathf.Abs(d.z) < 0.01f;
      bool z = Mathf.Abs(Mathf.Abs(d.z) - spacing) < 0.01f && Mathf.Abs(d.x) < 0.01f;

      if (x || z)
        result.Add(q);
    }
    return result;
  }
}
