// Enhanced PowerUpManager.cs with improved transparency handling
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance { get; private set; }

    [Header("Power-Up Durations")]
    public float coinDoublerDuration = 10f;
    public float stealthModeDuration = 8f;

    [Header("Stealth Settings")]
    [Range(0f, 1f)]
    public float stealthTransparency = 0.5f; // How transparent the player becomes
    
    // Current power-up states
    private bool isCoinDoublerActive = false;
    private bool isStealthModeActive = false;
    private int currentCoinMultiplier = 1;
    
    // Player reference for stealth mode
    private GameObject player;
    
    // Store original material properties for restoration
    private Dictionary<Material, MaterialProperties> originalMaterials = new Dictionary<Material, MaterialProperties>();
    
    // Track active coroutines
    private Coroutine coinDoublerCoroutine;
    private Coroutine stealthModeCoroutine;

    private struct MaterialProperties
    {
        public Color originalColor;
        public float originalMode;
        public int originalSrcBlend;
        public int originalDstBlend;
        public int originalZWrite;
        public int originalRenderQueue;
        public bool alphaTestOn;
        public bool alphaBlendOn;
        public bool alphaPremultiplyOn;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Find player reference
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            StoreMaterialProperties();
        }
    }

    void StoreMaterialProperties()
    {
        if (player == null) return;

        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            foreach (Material mat in renderer.materials)
            {
                if (!originalMaterials.ContainsKey(mat))
                {
                    MaterialProperties props = new MaterialProperties();
                    props.originalColor = mat.color;
                    
                    // Store original shader properties if they exist
                    if (mat.HasProperty("_Mode"))
                        props.originalMode = mat.GetFloat("_Mode");
                    if (mat.HasProperty("_SrcBlend"))
                        props.originalSrcBlend = mat.GetInt("_SrcBlend");
                    if (mat.HasProperty("_DstBlend"))
                        props.originalDstBlend = mat.GetInt("_DstBlend");
                    if (mat.HasProperty("_ZWrite"))
                        props.originalZWrite = mat.GetInt("_ZWrite");
                    
                    props.originalRenderQueue = mat.renderQueue;
                    props.alphaTestOn = mat.IsKeywordEnabled("_ALPHATEST_ON");
                    props.alphaBlendOn = mat.IsKeywordEnabled("_ALPHABLEND_ON");
                    props.alphaPremultiplyOn = mat.IsKeywordEnabled("_ALPHAPREMULTIPLY_ON");
                    
                    originalMaterials[mat] = props;
                }
            }
        }
    }

    public void ActivateCoinDoubler()
    {
        // Stop existing coroutine if running
        if (coinDoublerCoroutine != null)
        {
            StopCoroutine(coinDoublerCoroutine);
        }
        coinDoublerCoroutine = StartCoroutine(CoinDoublerCoroutine());
    }

    public void ActivateStealthMode()
    {
        // Stop existing coroutine if running
        if (stealthModeCoroutine != null)
        {
            StopCoroutine(stealthModeCoroutine);
        }
        stealthModeCoroutine = StartCoroutine(StealthModeCoroutine());
    }

    IEnumerator CoinDoublerCoroutine()
    {
        isCoinDoublerActive = true;
        currentCoinMultiplier = 2;
        Debug.Log("🪙 Coin Doubler Activated! All coins worth 2x for " + coinDoublerDuration + " seconds");

        yield return new WaitForSeconds(coinDoublerDuration);

        isCoinDoublerActive = false;
        currentCoinMultiplier = 1;
        coinDoublerCoroutine = null;
        Debug.Log("💰 Coin Doubler Deactivated!");
    }

    IEnumerator StealthModeCoroutine()
    {
        isStealthModeActive = true;
        Debug.Log("👻 Stealth Mode Activated! Passing through obstacles for " + stealthModeDuration + " seconds");
        
        // Disable collision with obstacles
        DisableObstacleCollisions();
        
        // Make player semi-transparent
        MakePlayerTransparent(true);

        yield return new WaitForSeconds(stealthModeDuration);

        isStealthModeActive = false;
        stealthModeCoroutine = null;
        Debug.Log("🚫 Stealth Mode Deactivated!");
        
        // Re-enable collision with obstacles
        EnableObstacleCollisions();
        
        // Restore player transparency
        MakePlayerTransparent(false);
    }

    void DisableObstacleCollisions()
    {
        if (player == null) return;

        // Disable collisions with all obstacles using tags
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        foreach (GameObject obstacle in obstacles)
        {
            Collider obstacleCollider = obstacle.GetComponent<Collider>();
            Collider playerCollider = player.GetComponent<Collider>();
            
            if (obstacleCollider != null && playerCollider != null)
            {
                Physics.IgnoreCollision(playerCollider, obstacleCollider, true);
            }
        }
        
        Debug.Log($"Disabled collision with {obstacles.Length} obstacles");
    }

    void EnableObstacleCollisions()
    {
        if (player == null) return;

        // Re-enable collisions with all obstacles
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        foreach (GameObject obstacle in obstacles)
        {
            Collider obstacleCollider = obstacle.GetComponent<Collider>();
            Collider playerCollider = player.GetComponent<Collider>();
            
            if (obstacleCollider != null && playerCollider != null)
            {
                Physics.IgnoreCollision(playerCollider, obstacleCollider, false);
            }
        }
        
        Debug.Log($"Re-enabled collision with {obstacles.Length} obstacles");
    }

    void MakePlayerTransparent(bool transparent)
    {
        if (player == null) return;

        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            foreach (Material mat in renderer.materials)
            {
                if (transparent)
                {
                    // Make semi-transparent using the stealthTransparency value
                    Color color = mat.color;
                    color.a = stealthTransparency;
                    mat.color = color;
                    
                    // Change to transparent rendering mode for Standard shader
                    if (mat.HasProperty("_Mode"))
                    {
                        mat.SetFloat("_Mode", 3); // Transparent mode
                        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        mat.SetInt("_ZWrite", 0);
                        mat.DisableKeyword("_ALPHATEST_ON");
                        mat.EnableKeyword("_ALPHABLEND_ON");
                        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        mat.renderQueue = 3000;
                    }
                }
                else
                {
                    // Restore original material properties
                    if (originalMaterials.ContainsKey(mat))
                    {
                        MaterialProperties props = originalMaterials[mat];
                        
                        // Restore original color
                        mat.color = props.originalColor;
                        
                        // Restore original shader properties
                        if (mat.HasProperty("_Mode"))
                        {
                            mat.SetFloat("_Mode", props.originalMode);
                            mat.SetInt("_SrcBlend", props.originalSrcBlend);
                            mat.SetInt("_DstBlend", props.originalDstBlend);
                            mat.SetInt("_ZWrite", props.originalZWrite);
                        }
                        
                        // Restore keywords
                        if (props.alphaTestOn)
                            mat.EnableKeyword("_ALPHATEST_ON");
                        else
                            mat.DisableKeyword("_ALPHATEST_ON");
                            
                        if (props.alphaBlendOn)
                            mat.EnableKeyword("_ALPHABLEND_ON");
                        else
                            mat.DisableKeyword("_ALPHABLEND_ON");
                            
                        if (props.alphaPremultiplyOn)
                            mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                        else
                            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        
                        mat.renderQueue = props.originalRenderQueue;
                    }
                }
            }
        }
    }

    // Public getters
    public bool IsCoinDoublerActive => isCoinDoublerActive;
    public bool IsStealthModeActive => isStealthModeActive;
    public int GetCoinMultiplier() => currentCoinMultiplier;
    
    public float GetCoinDoublerRemainingTime()
    {
        return isCoinDoublerActive ? coinDoublerDuration : 0f;
    }
    
    public float GetStealthRemainingTime()
    {
        return isStealthModeActive ? stealthModeDuration : 0f;
    }
}