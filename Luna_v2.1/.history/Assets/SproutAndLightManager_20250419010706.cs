// SproutAndLightManager.cs
using UnityEngine;

public class SproutAndLightManager : MonoBehaviour
{
    [HideInInspector] public bool isHeld      = false;
    [HideInInspector] public bool isPlanted   = false;
    public bool isPlayerNearby { get; private set; }

    [Header("Growth Settings")]
    public float growDuration = 5f;
    private float growTimer  = 0f;
    private bool  isMature   = false;

    [Header("Light‑Mote Settings")]
    public GameObject lightMotePrefab;
    public float      moteInterval = 3f;
    private float     moteTimer    = 0f;

    [Header("Visual Effects")]
    public GameObject growthEffect;
    public GameObject glowEffect;

    [Header("Spore Hint Icon")]
    public GameObject sporeHintIcon;

    [Header("Light‑Hint Icon")]
    public GameObject lightHintIcon;

    [Header("Player‑Nearby Detection")]
    [Tooltip("How close the player must be to count as ‘nearby’")]
    public float playerNearbyRadius = 1f;
    private Transform _playerTransform;

    void Awake()
    {
        // Find the player by tag
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) _playerTransform = p.transform;
        else Debug.LogWarning($"[Sprout] No GameObject tagged 'Player' found in the scene!");

        // Initially hide both icons
        if (sporeHintIcon != null) sporeHintIcon.SetActive(false);
        if (lightHintIcon != null) lightHintIcon.SetActive(false);
    }

    void Update()
    {
        // ——— Player‑nearby via distance check ———
        if (_playerTransform != null)
        {
            float dist = Vector2.Distance(_playerTransform.position, transform.position);
            bool wasNearby = isPlayerNearby;
            isPlayerNearby = dist <= playerNearbyRadius;

            if (wasNearby != isPlayerNearby)
                Debug.Log($"[Sprout] {name} isPlayerNearby = {isPlayerNearby} (dist={dist:F2})");
        }

        // ——— Growth / mote logic ———
        if (isPlanted && !isHeld)
        {
            if (!isMature)
            {
                growTimer += Time.deltaTime;
                if (growTimer >= growDuration)
                {
                    isMature = true;
                    if (growthEffect != null) growthEffect.SetActive(false);
                }
            }
            else
            {
                moteTimer += Time.deltaTime;
                if (lightMotePrefab != null && moteTimer >= moteInterval)
                {
                    Instantiate(lightMotePrefab, transform.position, Quaternion.identity);
                    moteTimer = 0f;
                }
            }
        }

        // ——— Visual toggles ———
        if (growthEffect != null)
            growthEffect.SetActive(isPlanted && !isMature && !isHeld);
        if (glowEffect != null)
            glowEffect.SetActive(isMature && !isHeld);

        // ——— Hint icons ———
        if (sporeHintIcon != null)
            sporeHintIcon.SetActive(isPlayerNearby && !isPlanted);
        if (lightHintIcon != null)
            lightHintIcon.SetActive(isPlayerNearby && isMature);
    }

    /// <summary>
    /// Called by your spore system when Luna plants a spore here.
    /// </summary>
    public void PlantSpore()
    {
        isPlanted = true;
        ResetOnGrowth();
        if (sporeHintIcon != null) sporeHintIcon.SetActive(false);
    }

    /// <summary>
    /// Resets timers so the flower must grow again.
    /// </summary>
    public void ResetOnGrowth()
    {
        growTimer = 0f;
        moteTimer = 0f;
        isMature  = false;
        if (growthEffect != null) growthEffect.SetActive(true);
        if (glowEffect != null)   glowEffect.SetActive(false);
    }
}
