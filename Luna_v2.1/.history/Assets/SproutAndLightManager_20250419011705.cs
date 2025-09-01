using UnityEngine;

public class SproutAndLightManager : MonoBehaviour
{
    [HideInInspector] public bool isHeld    = false;
    [HideInInspector] public bool isPlanted = false; // <-- added back
    public bool isPlayerNearby { get; private set; }

    [Header("Growth Settings")]
    public float growDuration       = 5f;
    public float maxScaleMultiplier = 1.5f;
    [Range(0f,1f)]
    public float lightHintThreshold = 0.95f;

    [Header("Light‑Mote Settings (optional)")]
    public GameObject lightMotePrefab;
    public float      moteInterval = 3f;

    [Header("Visuals")]
    public GameObject glowEffect;     // your LitFlowerB child
    public GameObject sporeHintIcon;  // “press R to grow” icon prefab
    public GameObject lightHintIcon;  // “press R to light” child icon

    // internal state
    Vector3 initialScale;
    Vector3 targetScale;
    float   growTimer = 0f;
    float   moteTimer = 0f;
    bool    isLit     = false;

    void Awake()
    {
        // cache scales
        initialScale = transform.localScale;
        targetScale  = initialScale * maxScaleMultiplier;

        // reset visuals and timers
        transform.localScale = initialScale;
        growTimer = moteTimer = 0f;
        isLit = false;
        if (glowEffect    != null) glowEffect.SetActive(false);
        if (sporeHintIcon != null) sporeHintIcon.SetActive(false);
        if (lightHintIcon != null) lightHintIcon.SetActive(false);
    }

    void Update()
    {
        // — Growth & scaling —
        if (!isHeld && growTimer < growDuration)
        {
            growTimer += Time.deltaTime;
            float t = Mathf.Clamp01(growTimer / growDuration);
            transform.localScale = Vector3.Lerp(initialScale, targetScale, t);
        }
        else if (growTimer >= growDuration && lightMotePrefab != null)
        {
            moteTimer += Time.deltaTime;
            if (moteTimer >= moteInterval)
            {
                Instantiate(lightMotePrefab, transform.position, Quaternion.identity);
                moteTimer = 0f;
            }
        }

        // — Hint icons —
        float pct = Mathf.Clamp01(growTimer / growDuration);
        bool canGrow  = pct < 1f;
        bool canLight = pct >= lightHintThreshold && !isLit;

        if (sporeHintIcon != null)
            sporeHintIcon.SetActive(isPlayerNearby && canGrow);

        if (lightHintIcon != null)
            lightHintIcon.SetActive(isPlayerNearby && canLight);

        // ensure glow only when lit
        if (glowEffect != null)
            glowEffect.SetActive(isLit);
    }

    // — Player proximity via trigger —
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

    /// <summary>
    /// Called by LunaSporeSystem when you plant a spore here.
    /// </summary>
    public void PlantSpore()
    {
        isPlanted   = true;            // allow external scripts to see it’s planted
        growTimer   = 0f;
        moteTimer   = 0f;
        isLit       = false;
        transform.localScale = initialScale;
        if (glowEffect    != null) glowEffect.SetActive(false);
        if (sporeHintIcon != null) sporeHintIcon.SetActive(false);
        if (lightHintIcon != null) lightHintIcon.SetActive(false);
    }

    /// <summary>
    /// Called when the player gives light after maturity.
    /// </summary>
    public void GiveLight()
    {
        float pct = Mathf.Clamp01(growTimer / growDuration);
        if (pct >= lightHintThreshold && !isLit)
        {
            isLit = true;
            if (glowEffect != null) glowEffect.SetActive(true);
        }
    }
}
