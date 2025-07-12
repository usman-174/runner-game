using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiamondSpawner : MonoBehaviour
{
    public GameObject diamondPrefab; // Diamond prefab reference
    public int numberOfDiamonds = 5; // Kitne diamonds spawn karne hain

    [Header("Spawn Area Settings")]
    public float minX = -2f;
    public float maxX = 2f;
    public float startZ = 30f;  // Start spawning after this Z distance
    public float distanceBetweenDiamonds = 20f; // Minimum distance between diamonds
    public float yPosition = -2f; // Adjust karo road height ke hisaab se

    void Start()
    {
        SpawnDiamonds();
    }

    void SpawnDiamonds()
    {
        float currentZ = startZ;

        for (int i = 0; i < numberOfDiamonds; i++)
        {
            Vector3 spawnPos = new Vector3(
                Random.Range(minX, maxX), // X position random lanes ke lye
                yPosition,
                currentZ // Z position calculated below
            );

            Instantiate(diamondPrefab, spawnPos, Quaternion.identity);

            // ➡️ Use Random.Range here for random distance
            currentZ += Random.Range(distanceBetweenDiamonds, distanceBetweenDiamonds * 3f);
        }
    }

}
