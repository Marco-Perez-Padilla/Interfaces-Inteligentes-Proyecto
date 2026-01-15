using UnityEngine;

public class Piece : MonoBehaviour
{

    [SerializeField] Vector3[] path;


    private void OnDrawGizmos()
    {
        foreach (Vector3 point in path)
        {
            Gizmos.color = Color.red;

            Gizmos.DrawSphere(transform.position + point, 0.1f);
        }

        for (int i = 0; i < path.Length - 1; i++)
        {

            Vector3 currrentPoint = GetPoint(i);
            Vector3 nextPoint = GetPoint(i + 1);


            Gizmos.color = Color.green;
            Gizmos.DrawLine(currrentPoint, nextPoint);
        }
    }

    public Vector3 GetPoint(int index)
    {
        return transform.TransformPoint(path[index]);
    }
    public Vector3 GetLastPoint()
    {
        return GetPoint(path.Length - 1);
    }
    public Vector3 GetFirstPoint()
    {
        return GetPoint(0);
    }
    public int PathLength()
    {
        return path.Length;
    }

}
