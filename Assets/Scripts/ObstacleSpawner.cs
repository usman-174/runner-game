using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Obstacle Settings")]
    public GameObject[] obstaclePrefabs;
    public Transform[] spawnPoints;

    [Header("Spawn Timing")]
    public float spawnInterval = 3f; // Increased from 2f to 3f
    public float spawnDistance = 50f;

    [Header("Spawn Chance")]
    [Range(0f, 1f)]
    public float spawnProbability = 0.6f; // Only 60% chance to spawn obstacle

    [Header("References")]
    public Transform player;

    private float nextSpawnTime = 0f;
    private List<GameObject> activeObstacles = new List<GameObject>();

    void Start()
    {
        nextSpawnTime = Time.time + spawnInterval;
    }

    void Update()
    {
        if (Time.timeScale > 0)
        {
            if (Time.time >= nextSpawnTime)
            {
                if (Random.value <= spawnProbability)
                {
                    SpawnObstacle();
                }
                nextSpawnTime = Time.time + spawnInterval;
            }

            CleanupOldObstacles();
        }
    }

    void SpawnObstacle()
    {
        if (obstaclePrefabs.Length == 0 || spawnPoints.Length == 0)
        {
            Debug.LogWarning("No obstacle prefabs or spawn points assigned!");
            return;
        }

        int randomObstacleIndex = Random.Range(0, obstaclePrefabs.Length);
        GameObject obstacleToSpawn = obstaclePrefabs[randomObstacleIndex];

        int randomSpawnPointIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[randomSpawnPointIndex];

        Vector3 spawnPosition = spawnPoint.position;
        spawnPosition.z = player.position.z + spawnDistance;

        // Check if the obstacle is a brick and adjust Y position
        if (obstacleToSpawn.name.ToLower().Contains("brick"))
        {
            spawnPosition.y = 2.9f;
        }

        GameObject newObstacle = Instantiate(obstacleToSpawn, spawnPosition, spawnPoint.rotation);

        activeObstacles.Add(newObstacle);

        Debug.Log($"Spawned {obstacleToSpawn.name} at lane {randomSpawnPointIndex}");
    }

    void CleanupOldObstacles()
    {
        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            if (activeObstacles[i] == null)
            {
                activeObstacles.RemoveAt(i);
            }
            else if (activeObstacles[i].transform.position.z < player.position.z - 30f)
            {
                Destroy(activeObstacles[i]);
                activeObstacles.RemoveAt(i);
            }
        }
    }
}