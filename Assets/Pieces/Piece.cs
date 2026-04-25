using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Piece", menuName = "Scriptable Objects/Piece")]
public class Piece : ScriptableObject
{
    [System.Serializable]
    private class Variant
    {
        public GameObject prefab;
        public float chance = 1;
    }

    [SerializeField] List<Variant> variants = new List<Variant>();

    public GameObject Instantiate()
    {
        float total = 0f;

        foreach (Variant variant in variants)
            if (variant.prefab != null)
                total += variant.chance;

        float randomValue = Random.value * total;

        for (int i = 0; i < variants.Count; i++)
        {
            if (variants[i].prefab == null) continue;

            if (randomValue < variants[i].chance)
                return Instantiate(variants[i].prefab);

            randomValue -= variants[i].chance;
        }

        return null;
    }
}