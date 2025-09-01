// SproutAndLightManager.cs
using UnityEngine;

public class SproutAndLightManager : MonoBehaviour
{
    [HideInInspector] public bool isHeld = false;
    [HideInInspector] public bool isPlanted = false;
    public bool isPlayerNearby { get; private set; }

    [Header("Growth Settings")]
    public float growDuration = 5f;
    private float growTimer = 0f;
    private bool isMature = false;

    [Header("Light‑Mote Settings")]
    public GameObject lightMotePrefab;
    public float moteInterval = 3f;
    private float moteTimer = 0f;

    [Header("Visual Effects")]
    public GameObject growthEffect;
    public GameObject glowEffect;

    [Header("Spore Hint Icon")]
    public GameObject sporeHintIcon;

    [Header("Light‑Hint Icon")]
    public GameObject lightHintIcon;

    void Awake()
    {
        if (sporeHintIcon != null)  sporeHintIcon.SetActive(false);
        if (lightHintIcon != null)  lightHintIcon.SetActive(false);
    }

    void Update()
    {
        // Growth / mote logic
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

        // Visual toggles
        if (growthEffect != null)
            growthEffect.SetActive(isPlanted && !isMature && !isHeld);
        if (glowEffect != null)
            glowEffect.SetActive(isMature   && !isHeld);

        // Spore‑plant hint (unplanted sprouts)
        if (sporeHintIcon != null)
            sporeHintIcon.SetActive(isPlayerNearby && !isPlanted);

        // Light‑up hint (ready‑to‑light sprouts)
        if (lightHintIcon != null)
            lightHintIcon.SetActive(isPlayerNearby && isMature);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerNearby = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerNearby = false;
    }

    /// <summary>Called by LunaSporeSystem when planting.</summary>
    public void PlantSpore()
    {
        isPlanted = true;
        ResetOnGrowth();
        if (sporeHintIcon != null) sporeHintIcon.SetActive(false);
    }

    /// <summary>Reset growth so the timer starts over.</summary>
    public void ResetOnGrowth()
    {
        growTimer = 0f;
        moteTimer = 0f;
        isMature  = false;
        if (growthEffect != null) growthEffect.SetActive(true);
        if (glowEffect != null)   glowEffect.SetActive(false);
    }
}
