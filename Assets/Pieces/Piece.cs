using UnityEngine;

[CreateAssetMenu(fileName = "Piece", menuName = "Scriptable Objects/Piece")]
public class Piece : ScriptableObject
{
    public GameObject original;
    [SerializeField] float originalChance = 0.8f;
    public GameObject variant0;
    [SerializeField] float variant0Chance = 0.2f;

    public GameObject Instantiate()
    {
        float randomValue = Random.value;

        if (randomValue <= originalChance || variant0 == null)
            return GameObject.Instantiate(original);
        else
            return GameObject.Instantiate(variant0);
    }
}