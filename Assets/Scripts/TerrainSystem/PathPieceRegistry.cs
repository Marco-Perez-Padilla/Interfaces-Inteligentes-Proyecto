using UnityEngine;
using System.Collections.Generic;

/**
  * @file PathPieceRegistry.cs
  * @brief Catálogo de prefabs usados por el sistema de caminos.
  *
  * Permite cambiar todo el arte sin tocar código.
  */
[CreateAssetMenu(menuName = "Path/Piece Registry")]
public class PathPieceRegistry : ScriptableObject
{
  [System.Serializable]
  public struct Entry
  {
    public PathPieceType type;
    public GameObject prefab;
  }

  [SerializeField] List<Entry> entries = new();

  Dictionary<PathPieceType, GameObject> map;

  void OnEnable()
  {
    map = new Dictionary<PathPieceType, GameObject>();
    foreach (var e in entries)
      if (!map.ContainsKey(e.type))
        map.Add(e.type, e.prefab);
  }

  public GameObject Get(PathPieceType type)
  {
    if (map == null) OnEnable();
    map.TryGetValue(type, out var prefab);
    return prefab;
  }
}
