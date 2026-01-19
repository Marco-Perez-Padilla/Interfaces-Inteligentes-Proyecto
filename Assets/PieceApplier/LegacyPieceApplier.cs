using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class LegacyPieceApplier : MonoBehaviour
{

    List<PathNode> mainPath;

    [SerializeField] PathGenerator pathGenerator;
    [SerializeField] GameObject pieces;
    PathGraph pathGraph;
    
    [Header("Pieces")]
    [SerializeField] GameObject straightPiece;
    [Space]
    [SerializeField] GameObject leftPiece;
    [SerializeField] GameObject rightPiece;
    [Space]
    [SerializeField] GameObject forkLeftRight;
    [SerializeField] GameObject forkLeftStraight;
    [SerializeField] GameObject forkRightStraight;
    [SerializeField] GameObject forkTriple;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Corridors")]
    [SerializeField] GameObject upCorridor;
    [SerializeField] GameObject downCorridor;
    [SerializeField] GameObject straightCorridor;

    List<Vector3> alreadyAppliedPieces;

    void Start()
    {
        pathGraph = pathGenerator.graph;
        mainPath = pathGraph.mainPath;
        alreadyAppliedPieces = new List<Vector3>();

        ApplyPiecesToPath(mainPath, Color.white);

        //ApplyPiecesToPath(pathGraph.subPaths[1], Color.blue);
        //ApplyPiecesToPath(pathGraph.subPaths[2], Color.green);
        
        foreach (List<PathNode> currentPath in pathGraph.subPaths)
        {
            ApplyPiecesToPath(currentPath, Color.red);
        }
    }

    GameObject GetPieceBasedOnDirection(Vector3 previousPosition, Vector3 currentPosition, Vector3 nextPosition)
    {

        previousPosition.y = 0;
        currentPosition.y = 0;
        nextPosition.y = 0;

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

    GameObject GetForkPieceBasedOnDirection(Vector3 previousPosition, Vector3 currentPosition, Vector3 nextPosition, Vector3 nextPositionA, Vector3 nextPositionB)
    {

        previousPosition.y = 0;
        currentPosition.y = 0;
        nextPosition.y = 0;
        nextPositionA.y = 0;
        nextPositionB.y = 0;

        Vector3 prev = (currentPosition - previousPosition).normalized;

        int left = 0;
        int right = 0;
        int straight = 0;

        void Classify(Vector3 next)
        {
            if (next == Vector3.zero) return; // means no way out

            float angle = Vector3.SignedAngle(prev, (next - currentPosition).normalized, Vector3.up);

            if (angle < -0.01f) left++;
            else if (angle > 0.01f) right++;
            else straight++;
        }

        Classify(nextPosition);
        Classify(nextPositionA);
        Classify(nextPositionB);

        if (left == 1 && right == 1 && straight == 1)
            return forkTriple;   // triple bifurcation

        if (left == 1 && right == 1)
            return forkLeftRight;

        if (left == 1 && straight == 1)
            return forkLeftStraight;

        if (right == 1 && straight == 1)
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

    bool IsCurrentNodeAlreadyApplied(PathNode currentNode)
    {
        foreach (Vector3 currentAppliedPiecePosition in alreadyAppliedPieces)
        {
            if (Approximately(currentAppliedPiecePosition, currentNode.position))
            {
                return true;
            }
        }

        return false;
    }


    PathNode ApplyFirstPiece(PathNode currentNode)
    {
        PathNode nextNode = currentNode.connections[0];

        if (IsCurrentNodeAlreadyApplied(currentNode))
        {
            return nextNode;
        }
        alreadyAppliedPieces.Add(currentNode.position);

        Vector3 position = currentNode.position;

        int numberOfSubPaths = currentNode.connections.Count - 2;
        float scaleFactor = 1f / 5f;

        Vector3 direction = (nextNode.position - position).normalized;
        direction.y = 0;

        GameObject currentPiece;

        Vector3 fakePrevious = position - direction;

        if (numberOfSubPaths == 1)
        {
            currentPiece = GetForkPieceBasedOnDirection(
                fakePrevious, // fake previous
                position,
                nextNode.position,
                currentNode.connections[2].position,
                Vector3.zero
            );
        }
        else if (numberOfSubPaths == 2)
        {
            currentPiece = GetForkPieceBasedOnDirection(
                position - direction,
                position,
                nextNode.position,
                currentNode.connections[2].position,
                currentNode.connections[3].position
            );
        }
        else
        {
            currentPiece = GetPieceBasedOnDirection(
                position - direction,
                position,
                nextNode.position
            );
        }

        GameObject pieceObject = Instantiate(currentPiece);
        pieceObject.transform.parent = pieces.transform;
        pieceObject.transform.position = position;
        pieceObject.transform.forward = direction;
        pieceObject.transform.localScale = Vector3.one * scaleFactor;


        // corridor
        GameObject corridor = Instantiate(GetCorridorBasedOnDirection(position, nextNode.position));
        corridor.transform.position = position + direction;
        corridor.transform.forward = direction;
        corridor.transform.localScale = Vector3.one * scaleFactor;

        return nextNode;
    }

    private void ApplyPiecesToPath(List<PathNode> path, Color color)
    {
        ApplyFirstPiece(path[0]);

        PathNode currentNode = path[1];

        float scaleFactor = pathGenerator.spacing / 2 / 5;
        for (int i = 1; i < path.Count; i++)
        {

            currentNode = path[i];
            PathNode prevNode = currentNode.connections[0];
            PathNode nextNode = currentNode.connections[1];

            Vector3 position = currentNode.position;


            if (IsCurrentNodeAlreadyApplied(currentNode))
            {
                continue;
            }
            alreadyAppliedPieces.Add(currentNode.position);

            Vector3 direction = (position - prevNode.position).normalized;
            direction.y = 0;

            int numberOfWayouts = currentNode.connections.Count - 1;
            GameObject currentPiece;

            if (numberOfWayouts == 1)
            {
                currentPiece = GetPieceBasedOnDirection(prevNode.position, position, nextNode.position);
            }
            else if (numberOfWayouts == 2)
            {
                currentPiece = GetForkPieceBasedOnDirection(
                    prevNode.position,
                    position,
                    nextNode.position,
                    currentNode.connections[2].position,
                    Vector3.zero);
            }
            else
            {
                currentPiece = GetForkPieceBasedOnDirection(
                       prevNode.position,
                       position,
                       nextNode.position,
                       currentNode.connections[2].position,
                    currentNode.connections[3].position);


            }
            GameObject pieceObject = GameObject.Instantiate(currentPiece);
            pieceObject.transform.parent = pieces.transform;
            pieceObject.transform.forward = new Vector3(direction.x, 0, direction.z);
            pieceObject.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
            pieceObject.transform.position = position;

            foreach(PathNode connection in currentNode.connections)
            {


                if (IsCurrentNodeAlreadyApplied(connection))
                {
                    continue;
                }

                string name = i + "/" + path.Count;
                PlaceCorridor(scaleFactor, connection, position, name);
            }


        }
    }

    private void PlaceCorridor(float scaleFactor, PathNode nextNode, Vector3 position, string name)
    {
        GameObject currentCorridor = GetCorridorBasedOnDirection(position, nextNode.position);
        Vector3 corridorDirection = (nextNode.position - position).normalized;
        Vector3 middlePosition = position + (nextNode.position - position) * 0.5f;
        corridorDirection.y = 0;
        middlePosition.y = position.y;

        GameObject corridorObject = GameObject.Instantiate(currentCorridor);
        corridorObject.name = name;
        corridorObject.transform.parent = pieces.transform;
        corridorObject.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
        corridorObject.transform.forward = new Vector3(corridorDirection.x, 0, corridorDirection.z);
        corridorObject.transform.position = middlePosition;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Dictionary<Vector3, int> drawCount = new Dictionary<Vector3, int>();

        void DrawLabel(Vector3 pos, string text)
        {
            if (!drawCount.ContainsKey(pos))
                drawCount[pos] = 0;

            float yOffset = 1f * drawCount[pos];
            Handles.Label(pos + Vector3.up * yOffset, text);
            drawCount[pos]++;
        }

        int i = 0;
        foreach (PathNode node in mainPath)
        {
            DrawLabel(node.position, i + "/" + mainPath.Count);
            i++;
        }

        int j = 0;
        foreach (List<PathNode> currentPath in pathGraph.subPaths)
        {
            j++;
            i = 0;
            foreach (PathNode node in currentPath)
            {
                DrawLabel(node.position, i + "/" + currentPath.Count);
                i++;
            }
        }
    }

}

