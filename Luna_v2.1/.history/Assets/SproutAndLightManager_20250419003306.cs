// SproutAndLightManager.cs
using UnityEngine;

public class SproutAndLightManager : MonoBehaviour
{
    [HideInInspector] public bool isHeld    = false;
    [HideInInspector] public bool isPlanted = false;

    // Now public so other scripts can read it
    public bool isPlayerNearby { get; private set; }

    [Header("Growth Settings")]
    public float growDuration = 5f;
    private float growTimer = 0f;
    private bool  isMature  = false;

    [Header("Light‑Mote Settings")]
    public GameObject lightMotePrefab;
    public float      moteInterval = 3f;
    private float     moteTimer    = 0f;

    [Header("Visual Effects")]
    public GameObject growthEffect;
    public GameObject glowEffect;

    [Header("Spore Hint Icon")]
    public GameObject sporeHintIcon;

    void Awake()
    {
        if (sporeHintIcon != null)
            sporeHintIcon.SetActive(false);
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

        // Visual toggles
        if (growthEffect != null)
            growthEffect.SetActive(isPlanted && !isMature && !isHeld);
        if (glowEffect != null)
            glowEffect.SetActive(isMature && !isHeld);

        // Spore hint
        if (sporeHintIcon != null)
            sporeHintIcon.SetActive(isPlayerNearby && !isPlanted);
    }

    // Player approaches
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
        // Accept a spore via trigger
        else if (other.CompareTag("Spore"))
        {
            ReceiveSpore(other.gameObject);
        }
    }

    // Player leaves
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }

    // In case your spore uses a non-trigger collider
    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.collider.CompareTag("Spore"))
        {
            ReceiveSpore(coll.collider.gameObject);
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

    /// <summary>Public so external systems (LunaSporeSystem) can reset growth.</summary>
    public void ResetOnGrowth()
    {
        growTimer = 0f;
        moteTimer = 0f;
        isMature  = false;
        if (growthEffect != null) growthEffect.SetActive(true);
        if (glowEffect != null)   glowEffect.SetActive(false);
    }
}
