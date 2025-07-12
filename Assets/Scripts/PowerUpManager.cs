using UnityEngine;
using System.Collections;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance { get; private set; }

    [Header("Power-Up Durations")]
    public float coinDoublerDuration = 10f;

    // Current power-up states
    private bool isCoinDoublerActive = false;
    private int currentCoinMultiplier = 1;

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

    public void ActivateCoinDoubler()
    {
        // If already active, extend the duration by restarting the coroutine
        if (isCoinDoublerActive)
        {
            StopCoroutine(CoinDoublerCoroutine());
        }
        StartCoroutine(CoinDoublerCoroutine());
    }

    IEnumerator CoinDoublerCoroutine()
    {
        isCoinDoublerActive = true;
        currentCoinMultiplier = 2;
        Debug.Log("🪙 Coin Doubler Activated! All coins worth 2x for " + coinDoublerDuration + " seconds");

        yield return new WaitForSeconds(coinDoublerDuration);

        isCoinDoublerActive = false;
        currentCoinMultiplier = 1;
        Debug.Log("💰 Coin Doubler Deactivated!");
    }

    // Public getters
    public bool IsCoinDoublerActive => isCoinDoublerActive;
    public int GetCoinMultiplier() => currentCoinMultiplier;
    public float GetRemainingTime()
    {
        // You can implement this if you want to show remaining time in UI
        return isCoinDoublerActive ? coinDoublerDuration : 0f;
    }
}