using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CentralizedSpawnManager : MonoBehaviour
{
    [Header("Lane Setup")]
    public Transform[] lanes;              // Your lane transforms (left, center, right)
    public Transform player;               // Reference to player

    [Header("Obstacle Settings")]
    public GameObject[] obstaclePrefabs;
    public float obstacleSpawnDistance = 50f;
    public float obstacleSpawnInterval = 3f;
    [Range(0f, 1f)]
    public float obstacleSpawnProbability = 0.6f;

    [Header("Coin Settings")]
    public GameObject coinPrefab;
    public GameObject diamondPrefab;       // Optional: for special coins
    public float coinSpawnDistance = 30f;
    public float coinSpawnInterval = 2f;
    [Range(0f, 1f)]
    public float diamondSpawnChance = 0.1f; // 10% chance for diamond instead of coin

    [Header("Power-Up Settings")]
    public GameObject[] powerUpPrefabs;        // Array of different power-up prefabs
    public float powerUpSpawnDistance = 40f;
    public float powerUpSpawnInterval = 15f;   // Spawn every 15 seconds
    [Range(0f, 1f)]
    public float powerUpSpawnChance = 0.3f;    // 30% chance when interval hits

    [Header("Safety Rules")]
    public int maxConsecutiveObstacles = 2; // Max obstacles in a row per lane
    public float obstacleLength = 5f;       // How long an obstacle "blocks" the lane
    public bool alwaysKeepOneLaneClear = true; // Guarantee at least one lane is always clear

    // Internal tracking
    private float nextObstacleSpawnTime;
    private float nextCoinSpawnTime;
    private float nextPowerUpSpawnTime;
    private List<GameObject> activeObstacles = new List<GameObject>();
    private List<GameObject> activeCoins = new List<GameObject>();
    private List<GameObject> activePowerUps = new List<GameObject>();

    // Lane occupancy tracking
    private Dictionary<int, List<float>> laneObstaclePositions = new Dictionary<int, List<float>>();
    private Dictionary<int, int> laneObstacleCount = new Dictionary<int, int>();

    void Start()
    {
        nextObstacleSpawnTime = Time.time + obstacleSpawnInterval;
        nextCoinSpawnTime = Time.time + coinSpawnInterval;
        nextPowerUpSpawnTime = Time.time + powerUpSpawnInterval;

        // Initialize lane tracking
        for (int i = 0; i < lanes.Length; i++)
        {
            laneObstaclePositions[i] = new List<float>();
            laneObstacleCount[i] = 0;
        }
    }

    void Update()
    {
        if (Time.timeScale > 0)
        {
            // Handle obstacle spawning
            if (Time.time >= nextObstacleSpawnTime)
            {
                if (Random.value <= obstacleSpawnProbability)
                {
                    SpawnObstacle();
                }
                nextObstacleSpawnTime = Time.time + obstacleSpawnInterval;
            }

            // Handle coin spawning
            if (Time.time >= nextCoinSpawnTime)
            {
                SpawnCoins();
                nextCoinSpawnTime = Time.time + coinSpawnInterval;
            }

            // Handle power-up spawning
            if (Time.time >= nextPowerUpSpawnTime)
            {
                if (Random.value <= powerUpSpawnChance)
                {
                    SpawnPowerUp();
                }
                nextPowerUpSpawnTime = Time.time + powerUpSpawnInterval;
            }

            // Cleanup old objects
            CleanupOldObjects();
            UpdateLaneOccupancy();
        }
    }

    void SpawnObstacle()
    {
        if (obstaclePrefabs.Length == 0 || lanes.Length == 0)
        {
            Debug.LogWarning("No obstacle prefabs or lanes assigned!");
            return;
        }

        List<int> availableLanes = GetAvailableLanesForObstacles();

        if (availableLanes.Count == 0)
        {
            Debug.Log("No available lanes for obstacles - skipping spawn");
            return;
        }

        int selectedLane = availableLanes[Random.Range(0, availableLanes.Count)];
        int randomObstacleIndex = Random.Range(0, obstaclePrefabs.Length);
        GameObject obstacleToSpawn = obstaclePrefabs[randomObstacleIndex];

        Vector3 spawnPosition = lanes[selectedLane].position;
        spawnPosition.z = player.position.z + obstacleSpawnDistance;

        // Special handling for brick obstacles
        if (obstacleToSpawn.name.ToLower().Contains("brick"))
        {
            spawnPosition.y = 2.9f;
        }

        GameObject newObstacle = Instantiate(obstacleToSpawn, spawnPosition, lanes[selectedLane].rotation);
        activeObstacles.Add(newObstacle);

        // Track this obstacle
        laneObstaclePositions[selectedLane].Add(spawnPosition.z);
        laneObstacleCount[selectedLane]++;

        Debug.Log($"Spawned {obstacleToSpawn.name} in lane {selectedLane}");
    }

    void SpawnCoins()
    {
        if (coinPrefab == null || lanes.Length == 0)
        {
            Debug.LogWarning("No coin prefab or lanes assigned!");
            return;
        }

        List<int> safeLanes = GetSafeLanesForCoins();

        if (safeLanes.Count == 0)
        {
            Debug.Log("No safe lanes for coins - skipping spawn");
            return;
        }

        // Decide spawn pattern
        SpawnPattern pattern = DetermineSpawnPattern(safeLanes);

        switch (pattern)
        {
            case SpawnPattern.Single:
                SpawnSingleCoin(safeLanes);
                break;
            case SpawnPattern.Multiple:
                SpawnMultipleCoins(safeLanes);
                break;
            case SpawnPattern.Trail:
                SpawnCoinTrail(safeLanes);
                break;
        }
    }

    void SpawnPowerUp()
    {
        if (powerUpPrefabs.Length == 0 || lanes.Length == 0)
        {
            Debug.LogWarning("No power-up prefabs or lanes assigned!");
            return;
        }

        // Get safe lanes (avoid obstacles)
        List<int> safeLanes = GetSafeLanesForPowerUps();
        
        if (safeLanes.Count == 0)
        {
            Debug.Log("No safe lanes for power-ups - skipping spawn");
            return;
        }

        // Select random lane and power-up
        int selectedLane = safeLanes[Random.Range(0, safeLanes.Count)];
        int randomPowerUpIndex = Random.Range(0, powerUpPrefabs.Length);
        GameObject powerUpToSpawn = powerUpPrefabs[randomPowerUpIndex];

        Vector3 spawnPosition = lanes[selectedLane].position;
        spawnPosition.z = player.position.z + powerUpSpawnDistance;
        spawnPosition.y += -1.6f; // Slightly elevated for visibility

        GameObject newPowerUp = Instantiate(powerUpToSpawn, spawnPosition, Quaternion.identity);
        activePowerUps.Add(newPowerUp);

        Debug.Log($"Spawned {powerUpToSpawn.name} in lane {selectedLane}");
    }

    void SpawnSingleCoin(List<int> safeLanes)
    {
        int selectedLane = safeLanes[Random.Range(0, safeLanes.Count)];
        GameObject coinToSpawn = ShouldSpawnDiamond() ? diamondPrefab : coinPrefab;

        Vector3 spawnPosition = lanes[selectedLane].position;
        spawnPosition.z = player.position.z + coinSpawnDistance;
        spawnPosition.y -= 2f; // Decrease Y position by 2

        GameObject newCoin = Instantiate(coinToSpawn, spawnPosition, Quaternion.identity);
        activeCoins.Add(newCoin);
    }

    void SpawnMultipleCoins(List<int> safeLanes)
    {
        // Spawn coins in multiple safe lanes
        int coinsToSpawn = Mathf.Min(safeLanes.Count, Random.Range(2, 4));

        for (int i = 0; i < coinsToSpawn; i++)
        {
            if (i < safeLanes.Count)
            {
                GameObject coinToSpawn = ShouldSpawnDiamond() ? diamondPrefab : coinPrefab;

                Vector3 spawnPosition = lanes[safeLanes[i]].position;
                spawnPosition.z = player.position.z + coinSpawnDistance;
                spawnPosition.y -= 2f; // Decrease Y position by 2

                GameObject newCoin = Instantiate(coinToSpawn, spawnPosition, Quaternion.identity);
                activeCoins.Add(newCoin);
            }
        }
    }

    void SpawnCoinTrail(List<int> safeLanes)
    {
        // Create a trail of coins in one lane
        int selectedLane = safeLanes[Random.Range(0, safeLanes.Count)];
        int trailLength = Random.Range(3, 6);

        for (int i = 0; i < trailLength; i++)
        {
            Vector3 spawnPosition = lanes[selectedLane].position;
            spawnPosition.z = player.position.z + coinSpawnDistance + (i * 2f);
            spawnPosition.y -= 2f; // Decrease Y position by 2

            GameObject newCoin = Instantiate(coinPrefab, spawnPosition, Quaternion.identity);
            activeCoins.Add(newCoin);
        }
    }

    List<int> GetAvailableLanesForObstacles()
    {
        List<int> availableLanes = new List<int>();

        for (int i = 0; i < lanes.Length; i++)
        {
            // Check if lane has too many consecutive obstacles
            if (laneObstacleCount[i] >= maxConsecutiveObstacles)
                continue;

            // Check if there's already an obstacle too close
            float spawnZ = player.position.z + obstacleSpawnDistance;
            bool tooClose = false;

            foreach (float obstacleZ in laneObstaclePositions[i])
            {
                if (Mathf.Abs(obstacleZ - spawnZ) < obstacleLength)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                availableLanes.Add(i);
            }
        }

        // Safety check: if keeping one lane clear is enabled
        if (alwaysKeepOneLaneClear && availableLanes.Count >= lanes.Length)
        {
            // Remove one random lane to keep it clear
            availableLanes.RemoveAt(Random.Range(0, availableLanes.Count));
        }

        return availableLanes;
    }

    List<int> GetSafeLanesForCoins()
    {
        List<int> safeLanes = new List<int>();
        float coinSpawnZ = player.position.z + coinSpawnDistance;

        for (int i = 0; i < lanes.Length; i++)
        {
            bool isSafe = true;

            // Check if any obstacle would block this coin
            foreach (float obstacleZ in laneObstaclePositions[i])
            {
                if (Mathf.Abs(obstacleZ - coinSpawnZ) < obstacleLength)
                {
                    isSafe = false;
                    break;
                }
            }

            if (isSafe)
            {
                safeLanes.Add(i);
            }
        }

        return safeLanes;
    }

    List<int> GetSafeLanesForPowerUps()
    {
        List<int> safeLanes = new List<int>();
        float powerUpSpawnZ = player.position.z + powerUpSpawnDistance;

        for (int i = 0; i < lanes.Length; i++)
        {
            bool isSafe = true;

            // Check if any obstacle would block this power-up
            foreach (float obstacleZ in laneObstaclePositions[i])
            {
                if (Mathf.Abs(obstacleZ - powerUpSpawnZ) < obstacleLength + 5f) // Extra buffer
                {
                    isSafe = false;
                    break;
                }
            }

            if (isSafe)
            {
                safeLanes.Add(i);
            }
        }

        return safeLanes;
    }

    SpawnPattern DetermineSpawnPattern(List<int> safeLanes)
    {
        if (safeLanes.Count == 1)
            return SpawnPattern.Single;

        float rand = Random.value;

        if (rand < 0.4f) // 40% chance
            return SpawnPattern.Single;
        else if (rand < 0.8f) // 40% chance
            return SpawnPattern.Multiple;
        else // 20% chance
            return SpawnPattern.Trail;
    }

    bool ShouldSpawnDiamond()
    {
        return diamondPrefab != null && Random.value < diamondSpawnChance;
    }

    void UpdateLaneOccupancy()
    {
        float playerZ = player.position.z;

        // Remove obstacles that are behind the player
        for (int lane = 0; lane < lanes.Length; lane++)
        {
            laneObstaclePositions[lane].RemoveAll(z => z < playerZ - 10f);
            laneObstacleCount[lane] = laneObstaclePositions[lane].Count;
        }
    }

    void CleanupOldObjects()
    {
        float playerZ = player.position.z;

        // Cleanup obstacles
        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            if (activeObstacles[i] == null)
            {
                activeObstacles.RemoveAt(i);
            }
            else if (activeObstacles[i].transform.position.z < playerZ - 30f)
            {
                Destroy(activeObstacles[i]);
                activeObstacles.RemoveAt(i);
            }
        }

        // Cleanup coins
        for (int i = activeCoins.Count - 1; i >= 0; i--)
        {
            if (activeCoins[i] == null)
            {
                activeCoins.RemoveAt(i);
            }
            else if (activeCoins[i].transform.position.z < playerZ - 30f)
            {
                Destroy(activeCoins[i]);
                activeCoins.RemoveAt(i);
            }
        }

        // Cleanup power-ups
        for (int i = activePowerUps.Count - 1; i >= 0; i--)
        {
            if (activePowerUps[i] == null)
            {
                activePowerUps.RemoveAt(i);
            }
            else if (activePowerUps[i].transform.position.z < playerZ - 30f)
            {
                Destroy(activePowerUps[i]);
                activePowerUps.RemoveAt(i);
            }
        }
    }

    // Helper enum for spawn patterns
    enum SpawnPattern
    {
        Single,
        Multiple,
        Trail
    }
}