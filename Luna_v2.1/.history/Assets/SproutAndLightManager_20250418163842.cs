// SproutAndLightManager.cs
using UnityEngine;

public class SproutAndLightManager : MonoBehaviour
{
    [HideInInspector] public bool isHeld = false;
    [HideInInspector] public bool isPlanted = false;

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

    [Header("Spore Hint")]
    [Tooltip("Icon shown above sprout when you can spore it")]
    public GameObject sporeHintIcon;
    private bool isPlayerNearby = false;

    void Awake()
    {
        // ensure hint is off initially
        if (sporeHintIcon != null)
            sporeHintIcon.SetActive(false);
    }

    void Update()
    {
        // handle growth
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

        // update visuals
        if (growthEffect != null) growthEffect.SetActive(isPlanted && !isMature && !isHeld);
        if (glowEffect != null)   glowEffect.SetActive(isMature  && !isHeld);

        // update spore hint
        if (sporeHintIcon != null)
            sporeHintIcon.SetActive(isPlayerNearby && !isPlanted);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // when player walks in, show hint if not yet planted
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
        // consume a spore projectile/item
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

    /// <summary>
    /// Called when a Spore hits this sprout.
    /// </summary>
    private void ReceiveSpore(GameObject spore)
    {
        // start growth
        isPlanted = true;
        ResetOnGrowth();

        // hide hint
        if (sporeHintIcon != null)
            sporeHintIcon.SetActive(false);

        // destroy the spore object
        Destroy(spore);
    }

    /// <summary>
    /// Reset timers and visual effects so the sprout can begin growing anew.
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
