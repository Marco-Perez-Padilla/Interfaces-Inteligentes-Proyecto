using System.Collections.Generic;
using UnityEngine;

/**
 * @file PathPieceType.cs
 * @brief Tipos lógicos de piezas de camino.
 *
 * El generador solo trabaja con estos valores,
 * nunca con prefabs directamente.
 */
public enum PathPieceType
{
  // Turns, straights, and slopes
  Straight,
  Left,
  Right,
  Up,
  UpRight,
  UpLeft,
  Down,
  DownRight,
  DownLeft,

  // Forks
  ForkLeftRight,
  ForkLeftStraight,
  ForkRightStraight,
  ForkTriple,

  // Corridors
  CorridorStraight,
  CorridorUp,
  CorridorDown
}
