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
    
    [SerializeField] GameObject forkTriple;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] GameObject upCorridor;
    [SerializeField] GameObject downCorridor;
    [SerializeField] GameObject straightCorridor;


    List<Vector3> alreadyAppliedPieces;

    PathNode lastNode;

    void Start()
    {
        pathGraph = pathGenerator.graph;
        mainPath = pathGraph.mainPath;
        alreadyAppliedPieces = new List<Vector3>();

        lastNode = mainPath[mainPath.Count - 1];

        ApplyPiecesToPath(mainPath, 0);

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

    int TryGetCurrentNodeSubPaths(PathNode node, out List<PathNode> subPathA, out int indexA, out List<PathNode> subPathB, out int indexB)
    {
        /*
        foreach (List<PathNode> currentSubgraph in pathGraph.subPaths)
        {
            if (Approximately(currentSubgraph[0].position, currentNode.position))
            {
                subPath = currentSubgraph;
                return true;
            }
        }
        subPath = null;
        return false;*/

        subPathA = null;
        subPathB = null;
        indexA = -1;
        indexB = -1;
        
        if (node.connections.Count <= 1)
        {

            return 0;
        }

        if (node.connections.Count == 3)
        {

            Debug.Log("una bifurcacion");
            foreach (List<PathNode> currentSubgraph in pathGraph.subPaths)
            {
                Debug.Log("siguiente subgrafo");

                for (int i = 0; i < currentSubgraph.Count; i++)
                {
                    PathNode currentNode = currentSubgraph[i];
                    Debug.Log("siguiente nodo");

                    if (Approximately(currentNode.position, node.position))
                    {
                        
                        subPathA = currentSubgraph;
                        subPathB = null;
                        indexA = i;
                        indexB = -1;

                        return 1;
                    }
                }

            }

        }

        if (node.connections.Count == 4)
        {
            foreach (List<PathNode> currentSubgraph in pathGraph.subPaths)
            {


                for (int i = 0; i < currentSubgraph.Count; i++)
                {

                    PathNode currentNode = currentSubgraph[i];

                    if (Approximately(currentNode.position, node.position))
                    {
                        if (subPathA == null)
                        {
                            subPathA = currentSubgraph;
                            indexA = i;
                        }

                        subPathB = currentSubgraph;
                        indexB = i;
                        Debug.Log("asda");
                        return 2;
                    }
                }

            }
        }

        return -1;

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

        alreadyAppliedPieces.Add(currentNode.position);

        return false;
    }

    private void ApplyPiecesToPath(List<PathNode> path, int start)
    {

        PathNode currentNode = path[start + 1];
        Debug.Log(currentNode.position);
        Debug.Log(lastNode.position);
        while (currentNode.position != lastNode.position)
        {
            currentNode = currentNode.connections[1];
            PathNode prevNode = currentNode.connections[0];
            PathNode nextNode = currentNode.connections[1];

            if (IsCurrentNodeAlreadyApplied(currentNode))
            {
                continue;
            }

            Vector3 position = currentNode.position;

            List<PathNode> subPathA;
            List<PathNode> subPathB;
            int indexA;
            int indexB;
            int numberOfSubPaths = TryGetCurrentNodeSubPaths(currentNode, out subPathA, out indexA, out subPathB, out indexB);

            Vector3 direction = (position - prevNode.position).normalized;
            float scaleFactor = (1 / 5f);

            if (numberOfSubPaths >= 1)
            {

                //solución horrible pero así se queda
                Vector3 positionB = Vector3.zero;

                if (subPathB != null)
                {
                    positionB = currentNode.connections[3].position;
                }
                //ahí termina la solución horrible

                Debug.Log(prevNode.position);
                Debug.Log(position);
                Debug.Log(nextNode.position);
                Debug.Log(subPathA);
                Debug.Log(positionB);


                GameObject forkPiece = GetForkPieceBasedOnDirection(
                    prevNode.position, 
                    position, 
                    nextNode.position,
                    currentNode.connections[2].position, 
                    positionB);
                GameObject forkPieceObject = Instantiate(forkPiece);

                forkPieceObject.transform.forward = new Vector3(direction.x, 0, direction.z);
                forkPieceObject.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
                forkPieceObject.transform.position = position;

                ApplyPiecesToPath(subPathA, indexA);
                if (numberOfSubPaths == 2)
                {
                    ApplyPiecesToPath(subPathB, indexB);
                }
            }
            else
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

