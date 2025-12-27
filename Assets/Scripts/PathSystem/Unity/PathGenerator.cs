using UnityEngine;

/**
 * @file PathGenerator.cs
 * @brief Orquestador del sistema de caminos (MAIN + SUB FASE 1).
 */
[ExecuteAlways]
public class PathGenerator : MonoBehaviour
{
  [Header("References")]
  public Transform cartTransform;

  [Header("Grid")]
  public int width = 10;
  public int height = 10;
  public float spacing = 2f;

  [Header("Path")]
  public int maxMainLength = 40;
  public int minSubLength = 4;

  [Header("Random Seed")]
  public bool useRandomSeed = false;
  public int seed = 12345;

  [Header("Editor")]
  public bool regenerateInEditor = true;

  public PathGraph graph = new();

  private Grid2D grid;

  void OnEnable()
  {
    Generate();
  }

#if UNITY_EDITOR
  void OnValidate()
  {
    if (!regenerateInEditor || Application.isPlaying)
      return;

    if (useRandomSeed)
      seed = System.Environment.TickCount;

    Generate();
    UnityEditor.SceneView.RepaintAll();
  }
#endif

  public void Generate()
  {
    if (cartTransform == null)
    {
      Debug.LogWarning("PathGenerator: Cart Transform no asignado.");
      return;
    }

    graph = new PathGraph();
    Random.InitState(seed);

    grid = new Grid2D(transform.position, width, height, spacing);

    // =========================
    // MAIN PATH (DFS VALIDADO)
    // =========================
    var mainGen = new MainPathGenerator(grid, cartTransform);
    graph.mainPath = mainGen.Generate(seed, maxMainLength);

    if (graph.mainPath == null || graph.mainPath.Count < 2)
    {
      Debug.LogWarning("PathGenerator: Main path inválido.");
      return;
    }

    // =========================
    // SUB PATHS (FASE 1)
    // =========================
    var subGen = new SubPathGenerator(grid);

    int subId = 1;
    for (int i = 3; i < graph.mainPath.Count - 3; i += 5)
    {
      var sub = subGen.Generate(
          graph.mainPath[i],
          graph.mainPath,
          subId++,
          minSubLength
      );

      if (sub != null)
        graph.subPaths.Add(sub);
    }
  }

  public System.Collections.Generic.IEnumerable<Vector3> GetGridPoints()
  {
    return grid != null ? grid.points : null;
  }
}
