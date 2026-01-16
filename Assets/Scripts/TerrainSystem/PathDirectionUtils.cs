using UnityEngine;

public static class PathDirectionUtils
{
  public static PathPieceType GetTurnPiece(
      Vector3 prev,
      Vector3 curr,
      Vector3 next)
  {
    Vector3 a = (curr - prev).normalized;
    Vector3 b = (next - curr).normalized;

    float vertical = next.y - curr.y;
    if (vertical > 0.1f)
    {
      float angleUp = SignedAngleXZ(a, b);
      if (Mathf.Abs(angleUp) < 5f) return PathPieceType.Up;
      return angleUp > 0
          ? PathPieceType.UpRight
          : PathPieceType.UpLeft;
    }
    if (vertical < -0.1f)
    {
      float angleDown = SignedAngleXZ(a, b);
      if (Mathf.Abs(angleDown) < 5f) return PathPieceType.Down;
      return angleDown > 0
          ? PathPieceType.DownRight
          : PathPieceType.DownLeft;
    }

    float angle = SignedAngleXZ(a, b);
    if (Mathf.Abs(angle) < 5f)
      return PathPieceType.Straight;

    return angle > 0
        ? PathPieceType.Right
        : PathPieceType.Left;
  }

  public static float SignedAngleXZ(Vector3 a, Vector3 b)
  {
    a.y = 0;
    b.y = 0;
    return Vector3.SignedAngle(a, b, Vector3.up);
  }
}
