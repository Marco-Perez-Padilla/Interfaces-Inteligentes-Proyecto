using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Dibuja en el editor una representación visual del collider trigger
/// asociado al TriggerNotificator del mismo GameObject.
///
/// El color se asigna automáticamente según el tipo de trigger detectado,
/// eliminando la necesidad de configurarlo a mano en cada prefab variante.
///
/// Criterio de detección (en orden de prioridad):
///   1. Componente en el mismo GameObject o en el padre:
///      - NPCChasingEvent  → Rojo
///      - NoiseDetector    → Verde
///      - NPCNoiseSpawner  → Amarillo
///   2. Nombre del GameObject (fallback si no hay componente):
///      - Contiene "chase"         → Rojo
///      - Contiene "noisedetector" → Verde
///      - Contiene "noiseplayer"   → Amarillo
///   3. Sin coincidencia           → Cian (desconocido)
///
/// No afecta al comportamiento en runtime. Solo visible en Scene View.
/// </summary>
[RequireComponent(typeof(TriggerNotificator))]
public class TriggerZoneVisualizer : MonoBehaviour
{
#if UNITY_EDITOR

  // =====================================================
  // COLORES POR TIPO
  // =====================================================

  /// <summary>Color de relleno para triggers de persecución (Chase).</summary>
  private static readonly Color CHASE_FILL   = new Color(1f,  0f,  0f,  0.20f);
  /// <summary>Color de borde para triggers de persecución (Chase).</summary>
  private static readonly Color CHASE_BORDER = new Color(1f,  0f,  0f,  0.90f);

  /// <summary>Color de relleno para triggers de detector de ruido.</summary>
  private static readonly Color NOISE_DETECTOR_FILL   = new Color(0f,  1f,  0.5f, 0.20f);
  /// <summary>Color de borde para triggers de detector de ruido.</summary>
  private static readonly Color NOISE_DETECTOR_BORDER = new Color(0f,  1f,  0.5f, 0.90f);

  /// <summary>Color de relleno para triggers de generador de ruido (Player).</summary>
  private static readonly Color NOISE_PLAYER_FILL   = new Color(1f,  0.92f, 0f,  0.20f);
  /// <summary>Color de borde para triggers de generador de ruido (Player).</summary>
  private static readonly Color NOISE_PLAYER_BORDER = new Color(1f,  0.92f, 0f,  0.90f);

  /// <summary>Color de relleno para triggers de tipo desconocido.</summary>
  private static readonly Color UNKNOWN_FILL   = new Color(0f,  1f,  1f,  0.20f);
  /// <summary>Color de borde para triggers de tipo desconocido.</summary>
  private static readonly Color UNKNOWN_BORDER = new Color(0f,  1f,  1f,  0.90f);

  // =====================================================
  // SETTINGS
  // =====================================================

  [Header("Visualización")]
  [Tooltip("Si true, muestra la etiqueta con el tipo de trigger en la Scene View.")]
  [SerializeField] private bool showLabel = true;

  // =====================================================
  // GIZMOS
  // =====================================================

  void OnDrawGizmos()
  {
    Collider col = GetComponent<Collider>();
    if (col == null)
      return;

    TriggerType triggerType = DetectTriggerType();
    ResolveColors(triggerType, out Color fillColor, out Color borderColor);

    DrawColliderGizmo(col, fillColor, borderColor);

    if (showLabel)
    {
      Handles.Label(
          transform.position + Vector3.up * 0.5f,
          triggerType.ToString(),
          EditorStyles.boldLabel
      );
    }
  }

  // =====================================================
  // DETECTION
  // =====================================================

  /// <summary>
  /// Tipos de trigger reconocidos por el visualizador.
  /// </summary>
  private enum TriggerType
  {
    /// <summary>Trigger que activa persecución directa del enemigo.</summary>
    Chase,
    /// <summary>Trigger que detecta ruido del jugador.</summary>
    NoiseDetector,
    /// <summary>Trigger que genera ruido desde el jugador.</summary>
    NoisePlayer,
    /// <summary>Tipo no reconocido.</summary>
    Unknown,
  }

  /// <summary>
  /// Detecta el tipo de trigger buscando primero por componente
  /// (en este GameObject y en su padre) y luego por nombre como fallback.
  /// </summary>
  /// <returns>El tipo de trigger detectado.</returns>
  TriggerType DetectTriggerType()
  {
    // ── Detección por componente ──────────────────────────────────────
    // Busca en este GameObject y también en el padre, porque el script
    // de comportamiento puede estar en cualquiera de los dos niveles.
    if (GetComponentInParent<NPCChasingEvent>() != null
     || GetComponent<NPCChasingEvent>() != null)
      return TriggerType.Chase;

    if (GetComponentInParent<NoiseDetector>() != null
     || GetComponent<NoiseDetector>() != null)
      return TriggerType.NoiseDetector;

    if (GetComponentInParent<NPCNoiseSpawner>() != null
     || GetComponent<NPCNoiseSpawner>() != null)
      return TriggerType.NoisePlayer;

    // ── Fallback por nombre ───────────────────────────────────────────
    string lowerName = gameObject.name.ToLowerInvariant();

    if (lowerName.Contains("chase"))
      return TriggerType.Chase;

    if (lowerName.Contains("noisedetector"))
      return TriggerType.NoiseDetector;

    if (lowerName.Contains("noiseplayer"))
      return TriggerType.NoisePlayer;

    return TriggerType.Unknown;
  }

  /// <summary>
  /// Resuelve los colores de relleno y borde según el tipo de trigger.
  /// </summary>
  /// <param name="triggerType">Tipo de trigger detectado.</param>
  /// <param name="fillColor">Color de relleno resultante.</param>
  /// <param name="borderColor">Color de borde resultante.</param>
  void ResolveColors(TriggerType triggerType, out Color fillColor, out Color borderColor)
  {
    switch (triggerType)
    {
      case TriggerType.Chase:
        fillColor   = CHASE_FILL;
        borderColor = CHASE_BORDER;
        break;

      case TriggerType.NoiseDetector:
        fillColor   = NOISE_DETECTOR_FILL;
        borderColor = NOISE_DETECTOR_BORDER;
        break;

      case TriggerType.NoisePlayer:
        fillColor   = NOISE_PLAYER_FILL;
        borderColor = NOISE_PLAYER_BORDER;
        break;

      default:
        fillColor   = UNKNOWN_FILL;
        borderColor = UNKNOWN_BORDER;
        break;
    }
  }

  // =====================================================
  // DRAW
  // =====================================================

  /// <summary>
  /// Dibuja el gizmo adaptado al tipo de collider presente en el GameObject.
  /// Soporta BoxCollider y SphereCollider.
  /// </summary>
  /// <param name="col">Collider del que extraer forma y dimensiones.</param>
  /// <param name="fillColor">Color de relleno del gizmo.</param>
  /// <param name="borderColor">Color del borde del gizmo.</param>
  void DrawColliderGizmo(Collider col, Color fillColor, Color borderColor)
  {
    Matrix4x4 originalMatrix = Gizmos.matrix;
    Gizmos.matrix = transform.localToWorldMatrix;

    if (col is BoxCollider boxCollider)
    {
      Gizmos.color = fillColor;
      Gizmos.DrawCube(boxCollider.center, boxCollider.size);

      Gizmos.color = borderColor;
      Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
    }
    else if (col is SphereCollider sphereCollider)
    {
      Gizmos.color = fillColor;
      Gizmos.DrawSphere(sphereCollider.center, sphereCollider.radius);

      Gizmos.color = borderColor;
      Gizmos.DrawWireSphere(sphereCollider.center, sphereCollider.radius);
    }

    Gizmos.matrix = originalMatrix;
  }

#endif
}