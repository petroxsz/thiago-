using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Collectible : MonoBehaviour
{
    [Header("Collectible Settings")]
    [Tooltip("How many points this collectible gives when picked up")]
    public int points = 1;
    [Tooltip("If true the collectible GameObject will be destroyed when collected. If false it will be deactivated.")]
    public bool destroyOnCollect = true;

    [Header("Effects (optional)")]
    [Tooltip("Optional particle or VFX prefab to spawn when collected")]
    public GameObject collectEffect;
    [Tooltip("Optional sound played when collected")]
    public AudioClip collectSound;
    [Tooltip("Volume for the collect sound")]
    [Range(0f, 1f)]
    public float collectSoundVolume = 1f;

    [Header("Player Detection")]
    [Tooltip("If true, the collectible will detect the player by Tag == \"Player\" as well as by the PlayerController component. If false, it will only look for PlayerController component on the collider.")]
    public bool allowTagDetection = false;

    // Prevent double collection
    bool collected = false;

    void Start()
    {
        // Ensure collider is a trigger (recommended). This is a helpful runtime check, not an enforced change.
        var col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning($"Collectible '{name}' has a non-trigger Collider. It's recommended to set isTrigger = true.", this);
        }

        // Register with GameManager so totalCollectibles reflects the number present in the scene (if a GameManager exists)
        if (GameManager.Exists())
        {
            GameManager.Instance.totalCollectibles++;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (collected) return;

        bool isPlayer = false;

        // Prefer checking for PlayerController component (more robust)
        var pc = other.GetComponent<PlayerController>();
        if (pc != null)
            isPlayer = true;

        // Optional: fallback to tag check if allowed
        if (!isPlayer && allowTagDetection && other.CompareTag("Player"))
            isPlayer = true;

        if (!isPlayer)
            return;

        Collect(other.transform.position);
    }

    /// <summary>
    /// Executes collection logic: give score, notify GameManager, play effects and remove the item.
    /// </summary>
    void Collect(Vector3 contactPoint)
    {
        if (collected) return;
        collected = true;

        // Add score and notify GameManager
        if (GameManager.Exists())
        {
            GameManager.Instance.AddScore(points);
            GameManager.Instance.CollectItem();
        }

        // Spawn effect
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }

        // Play sound
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, contactPoint, collectSoundVolume);
        }

        // Remove or deactivate
        if (destroyOnCollect)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}

