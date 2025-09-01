using UnityEngine;

public class SproutAndLightManager : MonoBehaviour
{
    [HideInInspector] public bool isHeld    = false;
    [HideInInspector] public bool isPlanted = false;
    public bool isPlayerNearby { get; private set; }

    [Header("Growth (scales sprite)")]
    public float growDuration       = 5f;           // how long until it’s fully grown
    public float maxScaleMultiplier = 1.5f;         // how big it gets at maturity
    private float   growTimer       = 0f;
    private bool    isMature        = false;
    private Vector3 initialScale;
    private Vector3 targetScale;

    [Header("Visual Effects")]
    [Tooltip("Child object with the LitFlowerB sprite")]
    public GameObject glowEffect;

    [Header("Hint Icons")]
    [Tooltip("Icon prefab to show “press R to plant here”")]
    public GameObject sporeHintIcon;
    [Tooltip("Child icon to show “press R to light me”")]
    public GameObject lightHintIcon;

    void Awake()
    {
        // remember your original scale
        initialScale = transform.localScale;
        targetScale  = initialScale * maxScaleMultiplier;

        // start with the right visuals off
        transform.localScale      = initialScale;
        growTimer                 = 0f;
        isMature                  = false;
        if (glowEffect   != null) glowEffect.SetActive(false);
        if (sporeHintIcon!= null) sporeHintIcon.SetActive(false);
        if (lightHintIcon!= null) lightHintIcon.SetActive(false);
    }

    void Update()
    {
        // ——— handle growth/scaling ———
        if (isPlanted && !isHeld)
        {
            growTimer += Time.deltaTime;
            float t = Mathf.Clamp01(growTimer / growDuration);
            transform.localScale = Vector3.Lerp(initialScale, targetScale, t);

            if (t >= 1f && !isMature)
            {
                isMature = true;
                if (glowEffect != null) glowEffect.SetActive(true);
            }
        }

        // ——— hint‑icon toggles ———
        if (sporeHintIcon != null)
            sporeHintIcon.SetActive(isPlayerNearby && !isPlanted);

        if (lightHintIcon != null)
            lightHintIcon.SetActive(isPlayerNearby && isMature);

        // ensure glow hides if held or un‑mature
        if ((!isMature || isHeld) && glowEffect != null)
            glowEffect.SetActive(false);
    }

    // ——— classic trigger for player proximity ———
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
    /// Called by LunaSporeSystem when you press R to plant a spore here.
    /// Resets growth and hides the spore‑hint.
    /// </summary>
    public void PlantSpore()
    {
        isPlanted  = true;
        growTimer  = 0f;
        isMature   = false;
        transform.localScale = initialScale;
        if (glowEffect    != null) glowEffect.SetActive(false);
        if (sporeHintIcon != null) sporeHintIcon.SetActive(false);
    }
}
