using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game manager for a Roll-a-Ball game.
/// Responsibilities:
/// - holds score and collectible counts
/// - exposes simple public API for player scripts to call (AddScore, CollectItem)
/// - provides simple camera follow (optional)
/// - exposes simple events for UI or other systems to subscribe to
/// </summary>
public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    [Tooltip("Current player score (read-only at runtime)")]
    public int score;
    [Tooltip("How many collectibles exist in the level (set in Inspector)")]
    public int totalCollectibles;
    [Tooltip("How many collectibles the player has collected so far (read-only at runtime)")]
    public int collectedCount;
    [Tooltip("If true the game is currently paused")]
    public bool isPaused;

    [Header("References")]
    [Tooltip("Assign the player transform so the camera follow can use it (optional)")]
    public Transform player;
    [Tooltip("Camera used for the simple follow. If none assigned, Camera.main will be used when needed.")]
    public Camera gameCamera;

    [Header("Camera Follow")]
    [Tooltip("Enable a simple camera follow handled by the GameManager (optional)")]
    public bool cameraFollow = true;
    [Tooltip("Offset applied to the camera relative to the player when following")]
    public Vector3 cameraOffset = new Vector3(0f, 8f, -10f);
    [Tooltip("How quickly the camera interpolates to the target position (higher = snappier)")]
    [Range(0f, 20f)]
    public float cameraSmoothSpeed = 6f;

    // Events other systems (UI, audio, etc.) can subscribe to
    public event Action OnGameStarted;
    public event Action<bool> OnGamePaused; // parameter = isPaused
    public event Action<int> OnScoreChanged; // parameter = new score
    public event Action<int, int> OnCollectibleCollected; // parameters = collectedCount, totalCollectibles

    void Awake()
    {
        // Basic singleton enforcement
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Note: not calling DontDestroyOnLoad by default to keep behavior simple for now
    }

    void Start()
    {
        if (gameCamera == null)
            gameCamera = Camera.main;
    }

    void LateUpdate()
    {
        // Simple camera follow (only positional follow; rotation left to camera setup)
        if (cameraFollow && player != null)
        {
            if (gameCamera == null)
                gameCamera = Camera.main;

            if (gameCamera != null)
            {
                Vector3 targetPos = player.position + cameraOffset;
                gameCamera.transform.position = Vector3.Lerp(gameCamera.transform.position, targetPos, Time.deltaTime * cameraSmoothSpeed);
            }
        }
    }

    /// <summary>
    /// Starts or resets the logical game state. Does not load scenes.
    /// </summary>
    public void StartGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        score = 0;
        collectedCount = 0;
        OnGameStarted?.Invoke();
        OnScoreChanged?.Invoke(score);
        OnCollectibleCollected?.Invoke(collectedCount, totalCollectibles);
    }

    /// <summary>
    /// Toggles pause state. Uses Time.timeScale to pause gameplay.
    /// </summary>
    public void PauseToggle()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        OnGamePaused?.Invoke(isPaused);
    }

    /// <summary>
    /// Restart the current active scene.
    /// </summary>
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Add points to the score and notify listeners.
    /// </summary>
    public void AddScore(int amount)
    {
        if (amount == 0) return;
        score += amount;
        OnScoreChanged?.Invoke(score);
    }

    /// <summary>
    /// Call when the player collects a collectible.
    /// Increments the collected counter and notifies listeners.
    /// </summary>
    public void CollectItem()
    {
        collectedCount++;
        OnCollectibleCollected?.Invoke(collectedCount, totalCollectibles);
    }

    // Optional: helper to safely check if Instance exists before calling
    public static bool Exists() => Instance != null;
}
