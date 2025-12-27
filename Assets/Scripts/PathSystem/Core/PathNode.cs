using System.Collections.Generic;
using UnityEngine; 

/** 
 * @file PathNode.cs 
 * @brief Nodo lógico del sistema de caminos. 
 * Un PathNode representa un punto navegable del grafo. 
 * No contiene lógica de movimiento ni decisiones de jugador. 
 */
public class PathNode 
{ 
  /** @brief Posición mundial del nodo. */ 
  public Vector3 position;
  /** @brief Tipo de camino al que pertenece. */ 
  public PathType pathType;
  /** @brief Identificador del camino (0 = main, 1..N = subrutas). */ 
  public int pathId; 
  /** @brief Conexiones navegables desde este nodo. */ 
  public List<PathNode> connections = new(); 
  /** @brief Marca de punto seguro (DP). */ 
  public bool isDP; 
  /** @brief Marca de punto de salida de subruta (Pi). */ 
  public bool isPi; 

  /** @brief Marca de nodo de fusión (Pf). */ 
  public bool isMergeNode;
}