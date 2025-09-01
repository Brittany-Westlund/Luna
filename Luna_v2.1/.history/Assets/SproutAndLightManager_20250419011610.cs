using UnityEngine;

public class SproutAndLightManager : MonoBehaviour
{
    [HideInInspector] public bool isHeld = false;
    public bool isPlayerNearby { get; private set; }

    [Header("Growth Settings")]
    [Tooltip("Time (seconds) to go from initialScale → maxScale")]
    public float growDuration = 5f;
    [Tooltip("How many times larger at full growth")]
    public float maxScaleMultiplier = 1.5f;
    [Tooltip("When to start showing lightHintIcon (e.g. 0.95 = 95%)")]
    [Range(0f,1f)]
    public float lightHintThreshold = 0.95f;

    [Header("Light‑Mote (optional)")]
    public GameObject lightMotePrefab;
    public float      moteInterval = 3f;

    [Header("Visuals")]
    [Tooltip("Child object with LitFlowerB sprite – turned on when you GiveLight()")]
    public GameObject glowEffect;
    [Tooltip("Prefab/icon to show “press R to spore‐grow”")]
    public GameObject sporeHintIcon;
    [Tooltip("Child icon to show “press R to light” when mature")]
    public GameObject lightHintIcon;

    // internals
    private Vector3 initialScale;
    private Vector3 targetScale;
    private float   growTimer  = 0f;
    private float   moteTimer  = 0f;
    private bool    isLit      = false;

    void Awake()
    {
        // cache scales
        initialScale = transform.localScale;
        targetScale  = initialScale * maxScaleMultiplier;

        // reset everything
        transform.localScale = initialScale;
        growTimer = moteTimer = 0f;
        isLit = false;
        if (glowEffect   != null) glowEffect.SetActive(false);
        if (sporeHintIcon!= null) sporeHintIcon.SetActive(false);
        if (lightHintIcon!= null) lightHintIcon.SetActive(false);
    }

    void Update()
    {
        // ——— Growth & scale lerp ———
        if (growTimer < growDuration && !isHeld)
        {
            growTimer += Time.deltaTime;
            float t = Mathf.Clamp01(growTimer / growDuration);
            transform.localScale = Vector3.Lerp(initialScale, targetScale, t);
        }
        else if (growTimer >= growDuration && lightMotePrefab != null)
        {
            // optional mote spawn once mature
            moteTimer += Time.deltaTime;
            if (moteTimer >= moteInterval)
            {
                Instantiate(lightMotePrefab, transform.position, Quaternion.identity);
                moteTimer = 0f;
            }
        }

        // ——— Hint icons ———
        float growPct = Mathf.Clamp01(growTimer / growDuration);
        bool canGrow   = growPct < 1f;
        bool canBeLit  = (growPct >= lightHintThreshold) && !isLit;

        if (sporeHintIcon != null)
            sporeHintIcon.SetActive(isPlayerNearby && canGrow);

        if (lightHintIcon != null)
            lightHintIcon.SetActive(isPlayerNearby && canBeLit);

        // ensure glow only on “lit”
        if (glowEffect != null)
            glowEffect.SetActive(isLit);
    }

    // ——— Trigger only on Player proximity ———
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

    // ——— Called by your luna system when you plant a spore here ———
    public void PlantSpore()
    {
        // reset growth
        growTimer = moteTimer = 0f;
        isLit = false;
        transform.localScale = initialScale;
        if (glowEffect   != null) glowEffect.SetActive(false);
        if (sporeHintIcon!= null) sporeHintIcon.SetActive(false);
        if (lightHintIcon!= null) lightHintIcon.SetActive(false);
    }

    // ——— Call this when the player gives it light (after maturity) ———
    public void GiveLight()
    {
        float growPct = Mathf.Clamp01(growTimer / growDuration);
        if (growPct >= lightHintThreshold && !isLit)
        {
            isLit = true;
            if (glowEffect != null) glowEffect.SetActive(true);
        }
    }
}
