using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CentralizedSpawnManager : MonoBehaviour
{
    [Header("Obstacle Prefabs")]
    [SerializeField] private GameObject[] obstaclePrefabs;
    
    [Header("Power-up Prefabs")]
    [SerializeField] private GameObject[] powerUpPrefabs;
    
    [Header("Spawn Points")]
    [SerializeField] private Transform leftLane;
    [SerializeField] private Transform rightLane;
    [SerializeField] private Transform centerLane;
    
    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private float spawnDistance = 50f;
    [SerializeField] private Transform playerReference;
    [SerializeField] private Transform roadReference;
    [SerializeField] private float heightOffsetFromRoad = 0f;
    
    [Header("Power-up Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float powerUpSpawnChance = 0.2f; // Reduced frequency
    [SerializeField] private float powerUpHeightOffset = 2f;
    [SerializeField] private bool powerUpsFloatAboveRoad = true;
    
    [Header("Cleanup Settings")]
    [SerializeField] private float cleanupDistance = 30f;
    [SerializeField] private float cleanupInterval = 2f;
    
    [Header("Spawn Boundaries")]
    [SerializeField] private float leftBoundary = -3f;
    [SerializeField] private float rightBoundary = 3f;
    
   
    
    [Header("Power-up Safe Zone Settings")]
    [SerializeField] private float safeZoneDistanceFromObstacles = 15f; // Distance before/after obstacles
    [SerializeField] private float powerUpCooldown = 12f; // Time between power-ups
    [SerializeField] private float powerUpDelayAfterStart = 20f; // Wait before first power-up
    [SerializeField] private bool spawnInSafeZonesOnly = true;
    
    [Header("Advanced Settings")]
    [SerializeField] private bool randomizeInterval = false;
    [SerializeField] private Vector2 intervalRange = new Vector2(2f, 4f);
    [SerializeField] private bool useRoadReferenceForHeight = false;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    [SerializeField] private bool forceSpawnPowerUps = false;
    
    // Private variables
    private Transform playerTransform;
    private Coroutine spawnCoroutine;
    private Coroutine cleanupCoroutine;
    private List<GameObject> spawnedObstacles = new List<GameObject>();
    private List<GameObject> spawnedPowerUps = new List<GameObject>();
    
    // Power-up timing control
    private float lastPowerUpSpawnTime = -999f;
    private float gameStartTime;
    private Transform[] spawnPoints;
    
    // Track obstacle positions for safe zones
    private List<Vector3> recentObstaclePositions = new List<Vector3>();
    
    // Debug counters
    private int totalPowerUpAttempts = 0;
    private int successfulPowerUpSpawns = 0;
    
    void Start()
    {
        gameStartTime = Time.time;
        InitializeSpawnManager();
    }
    
    void InitializeSpawnManager()
    {
        // Find player if not assigned
        if (playerReference == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerReference = player.transform;
                if (showDebugInfo) Debug.Log("CentralizedSpawnManager: Player reference found automatically");
            }
            else
            {
                Debug.LogError("CentralizedSpawnManager: Player not found! Make sure player has 'Player' tag or assign manually.");
                return;
            }
        }
        
        playerTransform = playerReference;
        
        // Initialize spawn points array
        SetupSpawnPoints();
        
        // Find road if not assigned
        if (roadReference == null)
        {
            GameObject road = GameObject.FindGameObjectWithTag("Road");
            if (road == null)
            {
                road = GameObject.Find("Road");
            }
            
            if (road != null)
            {
                roadReference = road.transform;
                if (showDebugInfo) Debug.Log("CentralizedSpawnManager: Road reference found automatically");
            }
            else
            {
                Debug.LogWarning("CentralizedSpawnManager: Road reference not found! Using player position for height reference.");
            }
        }
        
        // Validate prefabs
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
        {
            Debug.LogWarning("CentralizedSpawnManager: No obstacle prefabs assigned!");
        }
        
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0)
        {
            Debug.LogError("CentralizedSpawnManager: No power-up prefabs assigned! Power-ups will not spawn.");
        }
        
        StartSpawning();
        StartCleanup();
        
        if (showDebugInfo)
        {
            Debug.Log($"CentralizedSpawnManager initialized with {spawnPoints?.Length ?? 0} spawn points");
            Debug.Log($"Power-up spawn chance: {powerUpSpawnChance * 100f}%, Safe zone distance: {safeZoneDistanceFromObstacles}");
        }
    }
    
    void SetupSpawnPoints()
    {
        List<Transform> validSpawnPoints = new List<Transform>();
        
        if (leftLane != null) validSpawnPoints.Add(leftLane);
        if (centerLane != null) validSpawnPoints.Add(centerLane);
        if (rightLane != null) validSpawnPoints.Add(rightLane);
        
        spawnPoints = validSpawnPoints.ToArray();
        
        if (spawnPoints.Length == 0)
        {
            Debug.LogError("CentralizedSpawnManager: No spawn points assigned! Please assign LeftLane, CenterLane, and/or RightLane in the inspector.");
        }
        else if (showDebugInfo)
        {
            Debug.Log($"Found {spawnPoints.Length} spawn points:");
            foreach (Transform point in spawnPoints)
            {
                Debug.Log($"- {point.name} at position {point.position}");
            }
        }
    }
    
    void StartSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        
        spawnCoroutine = StartCoroutine(SpawnItems());
    }
    
    void StartCleanup()
    {
        if (cleanupCoroutine != null)
        {
            StopCoroutine(cleanupCoroutine);
        }
        
        cleanupCoroutine = StartCoroutine(CleanupItems());
    }
    
    IEnumerator SpawnItems()
    {
        while (true)
        {
            if (playerTransform == null)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }
            
            // Always try to spawn an obstacle first (if available)
            if (obstaclePrefabs != null && obstaclePrefabs.Length > 0)
            {
                SpawnObstacle();
            }
            
            // Check if we should spawn a power-up (with timing restrictions)
            if (ShouldSpawnPowerUp())
            {
                TrySpawnPowerUp();
            }
            
            // Wait for next spawn cycle
            float waitTime = randomizeInterval ? 
                Random.Range(intervalRange.x, intervalRange.y) : 
                spawnInterval;
                
            yield return new WaitForSeconds(waitTime);
        }
    }
    
    bool ShouldSpawnPowerUp()
    {
        // Check basic conditions
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0) return false;
        if (spawnPoints == null || spawnPoints.Length == 0) return false;
        
        // Check if enough time has passed since game start
        if (Time.time - gameStartTime < powerUpDelayAfterStart) return false;
        
        // Check cooldown
        if (Time.time - lastPowerUpSpawnTime < powerUpCooldown) return false;
        
        // Check spawn chance
        float randomValue = Random.value;
        bool shouldSpawn = randomValue <= powerUpSpawnChance || forceSpawnPowerUps;
        
        if (showDebugInfo)
        {
            Debug.Log($"Power-up spawn check: Random={randomValue:F3}, Chance={powerUpSpawnChance:F3}, Should spawn={shouldSpawn}");
        }
        
        return shouldSpawn;
    }
    
    void SpawnObstacle()
    {
        GameObject prefabToSpawn = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        GameObject spawnedObstacle = Instantiate(prefabToSpawn);
        
        float originalY = spawnedObstacle.transform.position.y;
        Vector3 newPosition = CalculateObstacleSpawnPosition();
        
        if (useRoadReferenceForHeight)
        {
            newPosition.y = CalculateSpawnHeight();
        }
        else
        {
            newPosition.y = originalY + heightOffsetFromRoad;
        }
        
        if (newPosition.z <= playerTransform.position.z)
        {
            newPosition.z = playerTransform.position.z + spawnDistance + 20f;
        }
        
        spawnedObstacle.transform.position = newPosition;
        spawnedObstacles.Add(spawnedObstacle);
        
        // Track this obstacle position for safe zone calculations
        recentObstaclePositions.Add(newPosition);
        
        // Clean up old obstacle positions (keep only recent ones)
        CleanupOldObstaclePositions();
        
        if (showDebugInfo)
        {
            Debug.Log($"Spawned obstacle: {prefabToSpawn.name} at {newPosition}");
        }
    }
    
    void CleanupOldObstaclePositions()
    {
        // Remove obstacle positions that are too far behind the player
        for (int i = recentObstaclePositions.Count - 1; i >= 0; i--)
        {
            float distanceBehindPlayer = playerTransform.position.z - recentObstaclePositions[i].z;
            if (distanceBehindPlayer > cleanupDistance + safeZoneDistanceFromObstacles)
            {
                recentObstaclePositions.RemoveAt(i);
            }
        }
    }
    
    void TrySpawnPowerUp()
    {
        totalPowerUpAttempts++;
        
        if (showDebugInfo)
        {
            Debug.Log($"Attempting to spawn power-up (attempt #{totalPowerUpAttempts})");
        }
        
        // Try each spawn point to find a safe location
        List<Transform> availableSpawnPoints = new List<Transform>(spawnPoints);
        
        // Shuffle the spawn points for randomness
        for (int i = 0; i < availableSpawnPoints.Count; i++)
        {
            Transform temp = availableSpawnPoints[i];
            int randomIndex = Random.Range(i, availableSpawnPoints.Count);
            availableSpawnPoints[i] = availableSpawnPoints[randomIndex];
            availableSpawnPoints[randomIndex] = temp;
        }
        
        foreach (Transform spawnPoint in availableSpawnPoints)
        {
            Vector3 powerUpPosition = CalculatePowerUpPosition(spawnPoint);
            
            if (IsPositionInSafeZone(powerUpPosition))
            {
                SpawnPowerUpAt(powerUpPosition, spawnPoint);
                lastPowerUpSpawnTime = Time.time;
                successfulPowerUpSpawns++;
                
                if (showDebugInfo)
                {
                    Debug.Log($"✅ Successfully spawned power-up at {spawnPoint.name}! Total successful spawns: {successfulPowerUpSpawns}");
                }
                return;
            }
            else if (showDebugInfo)
            {
                Debug.Log($"❌ Position not safe for power-up at {spawnPoint.name} - {powerUpPosition}");
            }
        }
        
        if (showDebugInfo)
        {
            Debug.LogWarning($"Failed to find safe position for power-up at any spawn point");
        }
    }
    
    Vector3 CalculatePowerUpPosition(Transform spawnPoint)
    {
        Vector3 currentPlayerPos = playerTransform.position;
        
        // Calculate Z position ahead of player
        float extraDistance = 0f;
        Rigidbody playerRb = playerTransform.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            extraDistance = playerRb.linearVelocity.z * 2f;
        }
        
        float spawnZ = currentPlayerPos.z + spawnDistance + extraDistance + Random.Range(5f, 15f);
        
        // Use spawn point's X position exactly
        float spawnX = spawnPoint.position.x;
        
        // Calculate Y position
        float spawnY = CalculateSpawnHeight();
        if (powerUpsFloatAboveRoad)
        {
            spawnY += powerUpHeightOffset;
        }
        
        return new Vector3(spawnX, spawnY, spawnZ);
    }
    
    bool IsPositionInSafeZone(Vector3 position)
    {
        if (!spawnInSafeZonesOnly) return true;
        
        // Check distance from all recent obstacle positions
        foreach (Vector3 obstaclePos in recentObstaclePositions)
        {
            float distance = Vector3.Distance(position, obstaclePos);
            if (distance < safeZoneDistanceFromObstacles)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"Too close to obstacle: distance = {distance:F2}, required = {safeZoneDistanceFromObstacles}");
                }
                return false;
            }
        }
        
        // Check distance from other power-ups
        foreach (GameObject powerUp in spawnedPowerUps)
        {
            if (powerUp != null)
            {
                float distance = Vector3.Distance(position, powerUp.transform.position);
                if (distance < safeZoneDistanceFromObstacles * 0.5f) // Power-ups can be closer to each other
                {
                    if (showDebugInfo)
                    {
                        Debug.Log($"Too close to power-up {powerUp.name}: distance = {distance:F2}");
                    }
                    return false;
                }
            }
        }
        
        return true;
    }
    
    void SpawnPowerUpAt(Vector3 position, Transform spawnPoint)
    {
        GameObject prefabToSpawn = powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)];
        
        if (prefabToSpawn == null)
        {
            Debug.LogError("Selected power-up prefab is null!");
            return;
        }
        
        GameObject spawnedPowerUp = Instantiate(prefabToSpawn, position, Quaternion.identity);
        
        // Ensure the power-up has the correct script
        PowerUpPickup pickup = spawnedPowerUp.GetComponent<PowerUpPickup>();
        if (pickup == null)
        {
            Debug.LogWarning($"Power-up {prefabToSpawn.name} doesn't have PowerUpPickup script! Adding one...");
            pickup = spawnedPowerUp.AddComponent<PowerUpPickup>();
        }
        
        spawnedPowerUps.Add(spawnedPowerUp);
        
        Debug.Log($"✨ Spawned power-up: {prefabToSpawn.name} at {spawnPoint.name} - {position}");
    }
    
    Vector3 CalculateObstacleSpawnPosition()
    {
        Vector3 currentPlayerPos = playerTransform.position;
        
        float extraDistance = 0f;
        Rigidbody playerRb = playerTransform.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            extraDistance = playerRb.linearVelocity.z * 2f;
        }
        
        // For obstacles, use the original spawn boundaries (NOT lanes)
        float spawnX = Random.Range(leftBoundary, rightBoundary);
        
        Vector3 spawnPosition = new Vector3(
            spawnX,
            0f,
            currentPlayerPos.z + spawnDistance + extraDistance
        );
        
        return spawnPosition;
    }
    
    float CalculateSpawnHeight()
    {
        if (roadReference != null)
        {
            return roadReference.position.y + heightOffsetFromRoad;
        }
        else
        {
            RaycastHit hit;
            Vector3 rayStart = playerTransform.position + Vector3.up * 10f;
            
            if (Physics.Raycast(rayStart, Vector3.down, out hit, 20f))
            {
                return hit.point.y + heightOffsetFromRoad;
            }
            
            return playerTransform.position.y + heightOffsetFromRoad;
        }
    }
    
    IEnumerator CleanupItems()
    {
        while (true)
        {
            yield return new WaitForSeconds(cleanupInterval);
            
            if (playerTransform == null) continue;
            
            CleanupList(spawnedObstacles, "obstacle");
            CleanupList(spawnedPowerUps, "power-up");
            CleanupOldObstaclePositions();
        }
    }
    
    void CleanupList(List<GameObject> itemList, string itemType)
    {
        for (int i = itemList.Count - 1; i >= 0; i--)
        {
            if (itemList[i] == null)
            {
                itemList.RemoveAt(i);
                continue;
            }
            
            float distanceBehindPlayer = playerTransform.position.z - itemList[i].transform.position.z;
            
            if (distanceBehindPlayer > cleanupDistance)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"Cleaning up {itemType}: {itemList[i].name}");
                }
                
                Destroy(itemList[i]);
                itemList.RemoveAt(i);
            }
        }
    }
    
    // Public methods for testing
    [ContextMenu("Force Spawn PowerUp")]
    public void ForceSpawnPowerUp()
    {
        if (powerUpPrefabs != null && powerUpPrefabs.Length > 0 && spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Vector3 spawnPos = CalculatePowerUpPosition(randomSpawnPoint);
            SpawnPowerUpAt(spawnPos, randomSpawnPoint);
        }
    }
    
    [ContextMenu("Print Debug Stats")]
    public void PrintDebugStats()
    {
        Debug.Log($"=== Spawn Manager Debug Stats ===");
        Debug.Log($"Game time: {Time.time - gameStartTime:F1}s");
        Debug.Log($"Time since last power-up: {Time.time - lastPowerUpSpawnTime:F1}s");
        Debug.Log($"Power-up attempts: {totalPowerUpAttempts}");
        Debug.Log($"Successful spawns: {successfulPowerUpSpawns}");
        Debug.Log($"Success rate: {(totalPowerUpAttempts > 0 ? successfulPowerUpSpawns / (float)totalPowerUpAttempts * 100f : 0):F1}%");
        Debug.Log($"Active obstacles: {GetActiveObstacleCount()}");
        Debug.Log($"Active power-ups: {GetActivePowerUpCount()}");
        Debug.Log($"Tracked obstacle positions: {recentObstaclePositions.Count}");
        Debug.Log($"Available spawn points: {spawnPoints?.Length ?? 0}");
    }
    
    public void SetPowerUpSpawnChance(float newChance)
    {
        powerUpSpawnChance = Mathf.Clamp01(newChance);
        Debug.Log($"Power-up spawn chance changed to: {powerUpSpawnChance * 100f}%");
    }
    
    public int GetActiveObstacleCount()
    {
        spawnedObstacles.RemoveAll(obstacle => obstacle == null);
        return spawnedObstacles.Count;
    }
    
    public int GetActivePowerUpCount()
    {
        spawnedPowerUps.RemoveAll(powerUp => powerUp == null);
        return spawnedPowerUps.Count;
    }
}