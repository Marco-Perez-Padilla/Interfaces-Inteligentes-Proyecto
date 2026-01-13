using UnityEngine;

public class RandomPiecePicker : MonoBehaviour
{
    [SerializeField] GameObject[] pieces;

    public static RandomPiecePicker instance;

    private void Awake()
    {
        instance = this;
    }

    public GameObject GetRandomPiece()
    {
        int randomIndex = Random.Range(0, pieces.Length - 1);

        return GameObject.Instantiate(pieces[randomIndex]);

    }

}
