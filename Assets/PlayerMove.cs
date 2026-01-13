using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    Piece currentPiece;

    Vector3 currentPoint;
    Vector3 nextPoint;
    int stepsCounter;

    [SerializeField] Piece firstPiece;
    [SerializeField] float moveSpeed;
    [SerializeField] float turnSpeed;
    [SerializeField] float turnThreshold = 0.5f;

    float inStepProgress;
    Quaternion targetRotation;
    bool turning;

    void Start()
    {
        currentPiece = firstPiece;

        currentPoint = currentPiece.GetFirstPoint();
        nextPoint = currentPiece.GetPoint(1);

        transform.position = currentPoint;

        targetRotation = Quaternion.LookRotation(nextPoint - currentPoint);
        transform.rotation = targetRotation;
        turning = false;
    }

    void Update()
    {
        if (turning)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );

            if (Quaternion.Angle(transform.rotation, targetRotation) <= turnThreshold)
            {
                transform.rotation = targetRotation;
                turning = false;
            }

            return;
        }

        inStepProgress += moveSpeed * Time.deltaTime;
        transform.position = Vector3.Lerp(currentPoint, nextPoint, inStepProgress);

        if (inStepProgress >= 1f)
        {
            AdvanceStep();
        }
    }

    void AdvanceStep()
    {
        inStepProgress = 0f;
        stepsCounter++;

        if (stepsCounter >= currentPiece.PathLength() - 1)
        {
            GenerateNewPiece();
            return;
        }

        currentPoint = nextPoint;
        nextPoint = currentPiece.GetPoint(stepsCounter + 1);

        targetRotation = Quaternion.LookRotation(nextPoint - currentPoint);
        turning = true;
    }

    void GenerateNewPiece()
    {
        inStepProgress = 0f;
        stepsCounter = 0;

        GameObject newPiece = RandomPiecePicker.instance.GetRandomPiece();

        Vector3 direction = (nextPoint - currentPoint).normalized;

        newPiece.transform.forward = new Vector3(direction.x, 0, direction.z);
        newPiece.transform.position =
            nextPoint +
            newPiece.transform.forward * 2.5f +
            newPiece.transform.up * 2.5f;

        currentPiece = newPiece.GetComponent<Piece>();

        currentPoint = currentPiece.GetFirstPoint();
        nextPoint = currentPiece.GetPoint(1);

        targetRotation = Quaternion.LookRotation(nextPoint - currentPoint);
        turning = true;
    }
}
