using System.Collections.Generic;
using UnityEngine;

/**
 * @file PathGenerator.cs
 * @brief Orquestador del sistema de caminos procedural.
 *
 * Responsabilidades:
 * - Inicializar grid
 * - Generar camino principal (DFS con backtracking)
 * - Resolver reglas de nodos primordiales y cooldown
 * - Generar subrutas (fase topológica)
 * - Aplicar altura (fase visual)
 * - Limitar pendientes máximas (≤ 45°)
 * - Resolver decisiones jugables
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
  public float maxHeight = 6f;                 // Altura máxima absoluta
  [Range(0f, 1f)] public float climbChance = 0.25f;
  public int flatStartLength = 5;              // Main path plano inicial
  public int flatSubStartLength = 2;            // Subruta plana inicial

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

  [Header("Generated Data")]
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

    // -------------------------
    // RESET
    // -------------------------
    graph = new PathGraph();
    Random.InitState(seed);

    // -------------------------
    // GRID BASE
    // -------------------------
    grid = new Grid2D(transform.position, width, height, spacing);

    // -------------------------
    // MAIN PATH
    // -------------------------
    MainPathGenerator mainGen =
        new MainPathGenerator(grid, cartTransform, graph);

    graph.mainPath = mainGen.Generate(seed, maxMainLength);

    if (graph.mainPath == null || graph.mainPath.Count < 2)
    {
      Debug.LogWarning("PathGenerator: Main path inválido.");
      return;
    }

    // -------------------------
    // ALTURA MAIN PATH
    // -------------------------
    HeightModulator.ApplyToMain(
        graph.mainPath,
        spacing,
        maxHeight,
        climbChance,
        flatStartLength
    );

    // =====================================================
    // REGLAS DE GAMEPLAY (ANTES DE SUBRUTAS)
    // =====================================================

    SubPathCooldownResolver.Apply(
        graph.mainPath,
        primordialNodes,
        subPathCooldown
    );

    // -------------------------
    // SUB PATHS (FASE TOPOLOGÍA)
    // -------------------------
    SubPathGenerator subGen = new SubPathGenerator(grid, graph);

    for (int i = primordialNodes; i < graph.mainPath.Count - 3; i += 5)
    {
      PathNode pi = graph.mainPath[i];

      // Respeta cooldown y primordiales
      if (!pi.canStartSubPath)
        continue;

      PathNode pj =
          graph.mainPath[Random.Range(0, graph.mainPath.Count)];

      var sub = subGen.Generate(pi, pj, minSubLength);

      if (sub != null && sub.Count > 0)
        graph.subPaths.Add(sub);
    }

    // -------------------------
    // ALTURA SUBRUTAS
    // -------------------------
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

    // =====================================================
    // LIMITADOR DE PENDIENTES (≤ 45°)
    // =====================================================
    SlopeLimiter.Apply(graph, spacing);

    // -------------------------
    // RESOLUCIÓN FINAL
    // -------------------------
    DecisionResolver.Resolve(graph);
  }

  // ======================================================
  // DEBUG
  // ======================================================

  public IEnumerable<Vector3> GetGridPoints()
  {
    return grid != null ? grid.points : null;
  }
}
