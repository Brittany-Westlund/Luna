// SproutAndLightManager.cs
using UnityEngine;

public class SproutAndLightManager : MonoBehaviour
{
    [HideInInspector] public bool isHeld    = false;
    [HideInInspector] public bool isPlanted = false;

    // Made a public property so other scripts can read it
    public bool isPlayerNearby { get; private set; } = false;

    [Header("Growth Settings")]
    [Tooltip("Seconds from planting until mature")]
    public float growDuration = 5f;
    private float growTimer = 0f;
    private bool  isMature  = false;

    [Header("Light‑Mote Settings")]
    [Tooltip("Prefab of the light mote to spawn once mature")]
    public GameObject lightMotePrefab;
    [Tooltip("Seconds between motes once mature")]
    public float      moteInterval = 3f;
    private float     moteTimer    = 0f;

    [Header("Visual Effects")]
    [Tooltip("Effect to show while growing")]
    public GameObject growthEffect;
    [Tooltip("Effect to show when mature")]
    public GameObject glowEffect;

    [Header("Spore Hint Icon")]
    [Tooltip("Icon shown above sprout when player can spore it")]
    public GameObject sporeHintIcon;

    void Awake()
    {
        if (sporeHintIcon != null)
            sporeHintIcon.SetActive(false);
    }

    void Update()
    {
        // Growth logic
        if (isPlanted && !isHeld)
        {
            if (!isMature)
            {
                growTimer += Time.deltaTime;
                if (growTimer >= growDuration)
                {
                    isMature = true;
                    if (growthEffect != null)
                        growthEffect.SetActive(false);
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

        // Visuals
        if (growthEffect != null)
            growthEffect.SetActive(isPlanted && !isMature && !isHeld);
        if (glowEffect != null)
            glowEffect.SetActive(isMature && !isHeld);

        // Spore hint
        if (sporeHintIcon != null)
            sporeHintIcon.SetActive(isPlayerNearby && !isPlanted);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
        else if (other.CompareTag("Spore"))
        {
            ReceiveSpore(other.gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }

    private void ReceiveSpore(GameObject spore)
    {
        isPlanted = true;
        ResetOnGrowth();

        if (sporeHintIcon != null)
            sporeHintIcon.SetActive(false);

        Destroy(spore);
    }

    /// <summary>
    /// Public so LunaSporeSystem can call it
    /// to restart the growth cycle after planting.
    /// </summary>
    public void ResetOnGrowth()
    {
        growTimer = 0f;
        moteTimer = 0f;
        isMature  = false;

        if (growthEffect != null)
            growthEffect.SetActive(true);
        if (glowEffect != null)
            glowEffect.SetActive(false);
    }
}
