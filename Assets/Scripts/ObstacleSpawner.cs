using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Obstacle Settings")]
    public GameObject[] obstaclePrefabs;  // Array to hold your obstacle prefabs
    public Transform[] spawnPoints;       // Array to hold spawn positions (lanes)
    
    [Header("Spawn Timing")]
    public float spawnInterval = 2f;      // Time between spawns (in seconds)
    public float spawnDistance = 50f;     // How far ahead to spawn obstacles
    
    [Header("References")]
    public Transform player;              // Reference to the player
    
    private float nextSpawnTime = 0f;     // When to spawn next obstacle
    private List<GameObject> activeObstacles = new List<GameObject>(); // Track spawned obstacles
    
    void Start()
    {
        // Start spawning obstacles
        nextSpawnTime = Time.time + spawnInterval;
    }
    
    void Update()
    {
        // Only spawn if the game is not paused
        if (Time.timeScale > 0)
        {
            // Check if it's time to spawn a new obstacle
            if (Time.time >= nextSpawnTime)
            {
                SpawnObstacle();
                nextSpawnTime = Time.time + spawnInterval;
            }
            
            // Clean up obstacles that are behind the player
            CleanupOldObstacles();
        }
    }
    
    void SpawnObstacle()
    {
        // Make sure we have obstacle prefabs and spawn points
        if (obstaclePrefabs.Length == 0 || spawnPoints.Length == 0)
        {
            Debug.LogWarning("No obstacle prefabs or spawn points assigned!");
            return;
        }
        
        // Randomly select an obstacle prefab
        int randomObstacleIndex = Random.Range(0, obstaclePrefabs.Length);
        GameObject obstacleToSpawn = obstaclePrefabs[randomObstacleIndex];
        
        // Randomly select a spawn point (lane)
        int randomSpawnPointIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[randomSpawnPointIndex];
        
        // Calculate spawn position (ahead of player)
        Vector3 spawnPosition = spawnPoint.position;
        spawnPosition.z = player.position.z + spawnDistance;
        
        // Spawn the obstacle
        GameObject newObstacle = Instantiate(obstacleToSpawn, spawnPosition, spawnPoint.rotation);
        
        // Add to our tracking list
        activeObstacles.Add(newObstacle);
        
        Debug.Log($"Spawned {obstacleToSpawn.name} at lane {randomSpawnPointIndex}");
    }
    
    void CleanupOldObstacles()
    {
        // Remove obstacles that are too far behind the player
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