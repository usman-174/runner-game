using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [Header("Coin Settings")]
    public GameObject coinPrefab;          // Assign your Coin prefab here
    public Transform player;               // Reference to the player transform
    public float spawnDistance = 30f;      // Distance ahead of the player to spawn coins
    public float spawnInterval = 2f;       // Time between spawns
    public Transform[] lanes;              // Lanes where coins can appear (e.g. left, center, right)

    private float nextSpawnTime;

    void Start()
    {
        nextSpawnTime = Time.time + spawnInterval;
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnCoin();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    void SpawnCoin()
    {
        if (lanes.Length == 0 || coinPrefab == null || player == null)
        {
            Debug.LogWarning("CoinSpawner: Missing references or lanes not assigned.");
            return;
        }

        // Select a random lane
        int randomLaneIndex = Random.Range(0, lanes.Length);
        Transform lane = lanes[randomLaneIndex];

        // Determine spawn position ahead of player in the selected lane
        Vector3 spawnPosition = new Vector3(
            lane.position.x,
            lane.position.y,
            player.position.z + spawnDistance
        );

        Instantiate(coinPrefab, spawnPosition, Quaternion.identity);
    }
}
