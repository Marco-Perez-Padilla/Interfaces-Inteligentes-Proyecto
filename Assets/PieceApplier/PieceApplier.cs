using UnityEngine;
using System.Collections.Generic;

public class PieceApplier : MonoBehaviour
{

    List<PathNode> mainPath;

    [SerializeField] PathGenerator pathGenerator;
    PathGraph pathGraph;
    [SerializeField] GameObject straightPiece;
    [SerializeField] GameObject leftPiece;
    [SerializeField] GameObject rightPiece;
    [SerializeField] GameObject upPiece;
    [SerializeField] GameObject downPiece;

    [SerializeField] GameObject upLeftPiece;
    [SerializeField] GameObject upRightPiece;
    [SerializeField] GameObject downRightPiece;
    [SerializeField] GameObject downLeftPiece;

    [SerializeField] GameObject forkLeftRight;
    [SerializeField] GameObject forkLeftStraight;
    [SerializeField] GameObject forkRightStraight;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] GameObject upCorridor;
    [SerializeField] GameObject downCorridor;
    [SerializeField] GameObject straightCorridor;

    GameObject GetPieceBasedOnDirection(Vector3 previousPosition, Vector3 currentPosition, Vector3 nextPosition)
    {
        Vector3 prevSegment = (currentPosition - previousPosition).normalized;
        Vector3 nextSegment = (nextPosition - currentPosition).normalized;


        float angle = Vector3.SignedAngle(prevSegment, nextSegment, Vector3.up);

        // straight
        if (Mathf.Approximately(angle, 0))
            return straightPiece;
        // right turn
        if (angle > 0)
            return rightPiece;

        // left turn
        if (angle < 0)
            return leftPiece;
        
        return new GameObject();
    }

    GameObject GetForkPieceBasedOnDirection(
        Vector3 previousPosition,
        Vector3 currentPosition,
        Vector3 nextPositionA,
        Vector3 nextPositionB)
    {
        Vector3 prevSegment = (currentPosition - previousPosition).normalized;
        Vector3 nextA = (nextPositionA - currentPosition).normalized;
        Vector3 nextB = (nextPositionB - currentPosition).normalized;

        float angleA = Vector3.SignedAngle(prevSegment, nextA, Vector3.up);
        float angleB = Vector3.SignedAngle(prevSegment, nextB, Vector3.up);

        bool ALeft = angleA < -0.01f;
        bool ARight = angleA > 0.01f;
        bool AStraight = Mathf.Abs(angleA) <= 0.01f;

        bool BLeft = angleB < -0.01f;
        bool BRight = angleB > 0.01f;
        bool BStraight = Mathf.Abs(angleB) <= 0.01f;

        if ((ALeft && BRight) || (ARight && BLeft))
            return forkLeftRight;

        if ((ALeft && BStraight) || (AStraight && BLeft))
            return forkLeftStraight;

        if ((ARight && BStraight) || (AStraight && BRight))
            return forkRightStraight;

        return null;
    }

    GameObject GetCorridorBasedOnDirection(Vector3 currentPosition, Vector3 nextPosition)
    {
        if (nextPosition.y > currentPosition.y)
        {
            return upCorridor;
        }

        if (nextPosition.y < currentPosition.y)
        {
            return downCorridor;
        }
        return straightCorridor;
    }
    bool Approximately(Vector3 a, Vector3 b, float epsilon = 0.01f)
    {
        return (a - b).sqrMagnitude <= epsilon * epsilon;
    }
    bool TryGetCurrentNodeSubPath(PathNode currentNode, out List<PathNode> subPath)
    {

        foreach (List<PathNode> currentSubgraph in pathGraph.subPaths)
        {
            if (Approximately(currentSubgraph[0].position, currentNode.position))
            {
                subPath = currentSubgraph;
                return true;
            }
        }
        subPath = null;
        return false;
    }

    void Start()
    {
        pathGraph = pathGenerator.graph;
        mainPath = pathGraph.mainPath;
        ApplyPiecesToPath(mainPath);
    }

    private void ApplyPiecesToPath(List<PathNode> path)
    {
        for (int i = 1; i < path.Count - 1; i++)
        {
            PathNode currentNode = path[i];
            PathNode prevNode = path[i - 1];
            PathNode nextNode = path[i + 1];

            Vector3 position = currentNode.position;

            List<PathNode> subPath;
            bool isCurrentNodeSubpath = TryGetCurrentNodeSubPath(currentNode, out subPath);

            Vector3 direction = (position - prevNode.position).normalized;
            float scaleFactor = (1 / 5f);
            if (isCurrentNodeSubpath)
            {
                GameObject forkPiece = GetForkPieceBasedOnDirection(prevNode.position, position, nextNode.position, subPath[1].position);

                GameObject forkPieceObject = Instantiate(forkPiece);
                forkPieceObject.transform.forward = new Vector3(direction.x, 0, direction.z);
                forkPieceObject.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
                forkPieceObject.transform.position = position;
                ApplyPiecesToPath(subPath);
            } else
            {

                GameObject currentPiece = GetPieceBasedOnDirection(prevNode.position, position, nextNode.position);
                //GameObject currentPiece = straightPiece;

                GameObject pieceObject = Instantiate(currentPiece);
                pieceObject.transform.forward = new Vector3(direction.x, 0, direction.z);
                pieceObject.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
                pieceObject.transform.position = position;
            }


            GameObject currentCorridor = GetCorridorBasedOnDirection(position, nextNode.position);
            Vector3 corridorDirection = (nextNode.position - position).normalized;
            corridorDirection.y = 0;
            GameObject corridorObject = Instantiate(currentCorridor);
            corridorObject.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
            corridorObject.transform.forward = new Vector3(corridorDirection.x, 0, corridorDirection.z);
            corridorObject.transform.position = position + corridorDirection.normalized;


        }
    }

    // Update is called once per frame
    void Update()
    {


    }
}
