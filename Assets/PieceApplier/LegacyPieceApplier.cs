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
    [SerializeField] Piece straightPiece;
    [Space]
    [SerializeField] Piece leftPiece;
    [SerializeField] Piece rightPiece;
    [Space]
    [SerializeField] Piece forkLeftRight;
    [SerializeField] Piece forkLeftStraight;
    [SerializeField] Piece forkRightStraight;
    [SerializeField] Piece forkTriple;
    [Space]
    [SerializeField] Piece endPiece;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Corridors")]
    [SerializeField] Piece upCorridor;
    [SerializeField] Piece downCorridor;
    [SerializeField] Piece straightCorridor;

    List<Vector3> alreadyAppliedPieces;
    private float scaleFactor;

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

    Piece GetPieceBasedOnDirection(Vector3 previousPosition, Vector3 currentPosition, Vector3 nextPosition)
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
        
        return new Piece();
    }

    Piece GetTwoForkPieceBasedOnDirection(Vector3 previousPosition, Vector3 currentPosition, Vector3 nextPositionA, Vector3 nextPositionB)
    {

        previousPosition.y = 0;
        currentPosition.y = 0;
        nextPositionA.y = 0;
        nextPositionB.y = 0;

        Vector3 prev = (currentPosition - previousPosition).normalized;

        int left = 0;
        int right = 0;
        int straight = 0;

        Debug.Log(previousPosition);
        Debug.Log(currentPosition);
        Debug.Log(nextPositionA);
        Debug.Log(nextPositionB);

        void Classify(Vector3 next)
        {

            float angle = Vector3.SignedAngle(prev, (next - currentPosition).normalized, Vector3.up);

            if (angle < -0.01f) left++;
            else if (angle > 0.01f) right++;
            else straight++;
        }

        Classify(nextPositionA);
        Classify(nextPositionB);

        if (left == 1 && right == 1)
            return forkLeftRight;

        if (left == 1 && straight == 1)
            return forkLeftStraight;

        if (right == 1 && straight == 1)
            return forkRightStraight;
        Debug.Log(left + " " + straight + " " + right);
        return forkTriple;
    }

    Piece GetCorridorBasedOnDirection(Vector3 currentPosition, Vector3 nextPosition)
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

        Vector3 direction = (nextNode.position - position).normalized;
        direction.y = 0;

        Piece currentPiece;

        Vector3 fakePrevious = position - direction;

        if (numberOfSubPaths == 1)
        {
            currentPiece = GetTwoForkPieceBasedOnDirection(
                fakePrevious, // fake previous
                position,
                nextNode.position,
                currentNode.connections[2].position
            );
        }
        else if (numberOfSubPaths == 2)
        {
            currentPiece = forkTriple;
        }
        else
        {
            currentPiece = GetPieceBasedOnDirection(
                position - direction,
                position,
                nextNode.position
            );
        }

        GameObject pieceObject = currentPiece.Instantiate();
        pieceObject.transform.parent = pieces.transform;
        pieceObject.transform.position = position;
        pieceObject.transform.forward = direction;

        Vector3 prevScale = pieceObject.transform.localScale;
        pieceObject.transform.localScale = new Vector3(
            scaleFactor * prevScale.x,
            scaleFactor * prevScale.y,
            scaleFactor * prevScale.z
        );

        // corridor
        GameObject corridor = GetCorridorBasedOnDirection(position, nextNode.position).Instantiate();
        Vector3 midPosition = (position + nextNode.position) / 2;
        midPosition.y = position.y;
        corridor.transform.position = midPosition;
        corridor.transform.forward = direction;
        Vector3 prevCorridorScale = corridor.transform.localScale;
        corridor.transform.localScale = new Vector3(
            scaleFactor * prevCorridorScale.x,
            scaleFactor * prevCorridorScale.y,
            scaleFactor * prevCorridorScale.z
        );

        return nextNode;
    }


    PathNode ApplyLastPiece(PathNode currentNode)
    {
        PathNode prevNode = currentNode.connections[0];

        alreadyAppliedPieces.Add(currentNode.position);

        Vector3 position = currentNode.position;

        int numberOfSubPaths = currentNode.connections.Count - 2;

        Vector3 direction = (position - prevNode.position).normalized;
        direction.y = 0;

        Piece currentPiece = endPiece;

        GameObject pieceObject = currentPiece.Instantiate();
        pieceObject.transform.parent = pieces.transform;
        pieceObject.transform.position = position;
        pieceObject.transform.forward = direction;
        Vector3 prevScale = pieceObject.transform.localScale;
        pieceObject.transform.localScale = new Vector3(scaleFactor * prevScale.x, scaleFactor * prevScale.y, scaleFactor * prevScale.z);

        return null;
    }

    private void ApplyPiecesToPath(List<PathNode> path, Color color)
    {
        scaleFactor = pathGenerator.spacing / 4 / 6;

        ApplyFirstPiece(path[0]);

        PathNode currentNode = path[1];

        for (int i = 1; i < path.Count; i++)
        {

            currentNode = path[i];
            PathNode prevNode = currentNode.connections[0];

            if (currentNode.connections.Count == 1)
            {
                ApplyLastPiece(currentNode);
                continue;
            }

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
            Piece currentPiece;
            

            if (numberOfWayouts == 1)
            {
                currentPiece = GetPieceBasedOnDirection(prevNode.position, position, nextNode.position);
                Debug.Log("single");
            }
            else if (numberOfWayouts == 2)
            {

                PathNode otherNextNode = currentNode.connections[2];
                //a veces el codigo de alvaro hace que un nodo que solo tiene una salida (o sea con forma de L) diga que tiene dos salidas con la misma posición
                //por lo que compruebo si pasa ese caso y hago como si hubiera una salida sola
                bool wayoutDuplicationError = otherNextNode.position == nextNode.position; 
                if (wayoutDuplicationError)
                {
                    currentPiece = GetPieceBasedOnDirection(prevNode.position, position, nextNode.position);
                } else
                {
                    currentPiece = GetTwoForkPieceBasedOnDirection(
                        prevNode.position,
                        position,
                        nextNode.position,
                        otherNextNode.position);
                }
                
            }
            else
            {
                currentPiece = forkTriple;
                Debug.Log("triple");
            }
            GameObject pieceObject = currentPiece.Instantiate();
            pieceObject.transform.parent = pieces.transform;
            pieceObject.transform.forward = new Vector3(direction.x, 0, direction.z);
            Vector3 prevScale = pieceObject.transform.localScale;
            pieceObject.transform.localScale = new Vector3(scaleFactor * prevScale.x, scaleFactor * prevScale.y, scaleFactor * prevScale.z);
            pieceObject.transform.position = position;

            foreach(PathNode currentConnection in currentNode.connections)
            {
                if (IsCurrentNodeAlreadyApplied(currentConnection))
                {
                    continue;
                }
                string name = i + "/" + path.Count;
                PlaceCorridor(currentConnection, position, name);
            }


        }
    }

    private void PlaceCorridor(PathNode nextNode, Vector3 position, string name)
    {
        Piece currentCorridor = GetCorridorBasedOnDirection(position, nextNode.position);
        Vector3 direction = (nextNode.position - position).normalized;
        //corridorDirection.y = 0;
        Vector3 corridorPosition = (nextNode.position + position) / 2;
        corridorPosition.y = position.y;

        GameObject corridorObject = currentCorridor.Instantiate();
        float previousDirection = corridorObject.transform.eulerAngles.y;
        //corridorObject.name = name;
        corridorObject.transform.parent = pieces.transform;
        Vector3 prevScale = corridorObject.transform.localScale;
        corridorObject.transform.localScale = new Vector3(scaleFactor * prevScale.x, scaleFactor * prevScale.y, scaleFactor * prevScale.z);
        corridorObject.transform.forward = new Vector3(direction.x, 0, direction.z);
        Vector3 currentRotation = corridorObject.transform.eulerAngles;
        corridorObject.transform.eulerAngles = new Vector3(currentRotation.x, currentRotation.y + previousDirection, currentRotation.z);
        Debug.Log(corridorObject.name + ": " + currentRotation + "---" + corridorObject.transform.eulerAngles);
        Debug.Log(previousDirection);
        corridorObject.transform.position = corridorPosition;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Dictionary<Vector3, int> drawCount = new Dictionary<Vector3, int>();

        void DrawLabel(Vector3 pos, string text)
        {
            if (!drawCount.ContainsKey(pos))
            {
                drawCount[pos] = 0;
            }

            float yOffset = (1f * drawCount[pos]) + 0.5f;
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

