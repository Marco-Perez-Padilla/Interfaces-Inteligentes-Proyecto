using System.Collections.Generic;
using UnityEngine;

/**
 * @file PathGenerator.cs
 * @brief Orquestador del sistema de caminos procedural.
 *
 * Responsabilidades:
 * - Inicializar grid
 * - Generar camino principal (DFS con backtracking)
 * - Generar subrutas (Fase 1)
 * - Aplicar altura (Fase 2)
 */
[ExecuteAlways]
public class PathGenerator : MonoBehaviour
{
  // ======================================================
  // REFERENCES
  // ======================================================

  [Header("References")]
  public Transform cartTransform;

  // ======================================================
  // GRID
  // ======================================================

  [Header("Grid")]
  public int width = 10;
  public int height = 10;
  public float spacing = 2f;

  // ======================================================
  // PATH
  // ======================================================

  [Header("Path")]
  public int maxMainLength = 40;
  public int minSubLength = 4;

  // ======================================================
  // HEIGHT
  // ======================================================

  [Header("Height")]
  public float maxHeight = 6f; /// Altura máxima absoluta
  [Range(0f, 1f)] public float climbChance = 0.25f; // Probabilidad de cambio vertical
  public int flatStartLength = 5; // Nodos iniciales planos
  public int flatSubStartLength = 2; // Nodos iniciales planos en subrutas

  // ======================================================
  // SUBPATH RULES  
  // ======================================================

  [Header("SubPath Rules")]
  public int primordialNodes = 3;
  public int subPathCooldown = 5;

  // ======================================================
  // RANDOM
  // ======================================================

  [Header("Random Seed")]
  public bool useRandomSeed = false;
  public int seed = 12345;

  // ======================================================
  // EDITOR
  // ======================================================

  [Header("Editor")]
  public bool regenerateInEditor = true;

  // ======================================================
  // DATA
  // ======================================================

  public PathGraph graph = new();

  private Grid2D grid;

  // ======================================================
  // UNITY
  // ======================================================

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

  // ======================================================
  // GENERATION
  // ======================================================

  public void Generate()
  {
    if (cartTransform == null)
    {
      Debug.LogWarning("PathGenerator: Cart Transform no asignado.");
      return;
    }

    graph = new PathGraph();
    Random.InitState(seed);

    // Grid base
    grid = new Grid2D(transform.position, width, height, spacing);

    // =========================
    // MAIN PATH
    // =========================

    MainPathGenerator mainGen =
        new MainPathGenerator(grid, cartTransform);

    graph.mainPath = mainGen.Generate(seed, maxMainLength);

    if (graph.mainPath == null || graph.mainPath.Count < 2)
    {
      Debug.LogWarning("PathGenerator: Main path inválido.");
      return;
    }

    // Altura main path 
    HeightModulator.ApplyToMain(
        graph.mainPath,
        spacing,
        maxHeight,
        climbChance,
        flatStartLength
    );

    // =========================
    // SUB PATHS 
    // =========================

    SubPathGenerator subGen = new SubPathGenerator(grid);

    int subId = 1;
    for (int i = 3; i < graph.mainPath.Count - 3; i += 5)
    {
      var sub = subGen.Generate(
          graph.mainPath[i],
          graph.mainPath,
          subId++,
          minSubLength
      );

      if (sub != null && sub.Count > 0)
        graph.subPaths.Add(sub);
    }

    // Altura subrutas
    foreach (var sub in graph.subPaths)
    {
      HeightModulator.ApplyToSubPath(
          sub,
          flatSubStartLength,
          spacing,
          maxHeight,
          climbChance * 1.2f
      );
    }

    DecisionResolver.Resolve(graph);

    SubPathCooldownResolver.Apply(
      graph.mainPath,
      primordialNodes,
      subPathCooldown
    );
  }

  // ======================================================
  // DEBUG
  // ======================================================

  public IEnumerable<Vector3> GetGridPoints()
  {
    return grid != null ? grid.points : null;
  }
}
