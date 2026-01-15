using System.Collections.Generic;
using UnityEngine;

/**
 * @file PathNode.cs
 * @brief Nodo lógico único del sistema de caminos.
 *
 * REGLA FUNDAMENTAL:
 * - Existe UN SOLO PathNode por posición X/Y/Z en todo el grafo.
 * - La posición define la identidad del nodo.
 */
public class PathNode
{
  /** Posición mundial exacta */
  public Vector3 position;

  /** Tipo dominante del camino */
  public PathType pathType;

  /** Conexiones navegables */
  public List<PathNode> connections = new();

  /** Punto seguro (DP) */
  public bool isDP;

  /** Punto de salida de subruta (Pi) */
  public bool isPi;

  /** Nodo de fusión con el main path */
  public bool isMergeNode;

  /** Nodo con decisión jugable */
  public bool isDecisionNode;
  public List<PathNode> decisionExits = new();

  /** Nodo primordial */
  public bool isPrimordial;
  public bool canStartSubPath;
  public bool canReceiveSubPath;
}
