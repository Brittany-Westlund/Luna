using UnityEngine;
using System.Collections;

public class MystRestTransitionAuto : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer mystRenderer;
    public GameObject sparkles;

    [Header("Moonbow Objects")]
    public SpriteRenderer moonbowRenderer;
    public GameObject moonbowSparkles;

    [Header("Optional Light Source (keeps Moonbow active)")]
    public bool requireLightSource = false;
    public float lightCheckRadius = 2f;
    public LayerMask lightLayerMask;

    [Header("Fade Settings")]
    public float fadeSpeed = 1.5f;
    [Range(0f, 1f)] public float visibleAlpha = 1f;
    [Range(0f, 1f)] public float hiddenAlpha = 0f;

    [Header("Timing")]
    public float moonbowDelay = 0.8f;
    public float mistReturnDelay = 1.5f;
    public float moonbowHoldDuration = 3f;

    [Header("Detection Settings")]
    public float activationRadius = 2.5f;
    public string lunaTag = "Player";
    public bool debugLogs = false;

    [Header("Moonbow Positioning")]
    public float moonbowSitOffset = 0.4f;

    private LunaRest _lunaRest;
    private bool _transitionComplete;
    private bool _holdingMoonbow;

    void Start()
    {
        if (moonbowRenderer != null)
        {
            SetAlpha(moonbowRenderer, 0f);
            moonbowRenderer.gameObject.SetActive(false);
        }

        if (moonbowSparkles != null)
            moonbowSparkles.SetActive(false);
    }

    void Update()
    {
        if (_lunaRest == null)
        {
            GameObject luna = GameObject.FindGameObjectWithTag(lunaTag);
            if (luna != null)
                _lunaRest = luna.GetComponent<LunaRest>();
        }
        if (_lunaRest == null) return;

        float dist = Vector2.Distance(transform.position, _lunaRest.transform.position);
        bool lunaInRange = dist <= activationRadius;
        bool sparkling = sparkles != null && sparkles.activeInHierarchy;

        bool shouldTransition = lunaInRange && sparkling && _lunaRest.isResting;

        if (shouldTransition && !_transitionComplete)
        {
            StartCoroutine(TransitionToMoonbow());
            _transitionComplete = true;
        }
        else if (!shouldTransition && _transitionComplete && !_lunaRest.isResting)
        {
            StartCoroutine(HoldMoonbowThenFadeBack());
            _transitionComplete = false;
        }
    }

    // --- TRANSITION INTO MOONBOW ---
    private IEnumerator TransitionToMoonbow()
    {
        if (debugLogs) Debug.Log($"🌙 {name}: Myst → Moonbow");

        if (sparkles != null)
            sparkles.SetActive(false);

        yield return StartCoroutine(FadeSprite(mystRenderer, hiddenAlpha));
        yield return new WaitForSeconds(moonbowDelay);

        if (moonbowRenderer != null)
            moonbowRenderer.gameObject.SetActive(true);
        if (moonbowSparkles != null)
            moonbowSparkles.SetActive(true);

        yield return StartCoroutine(FadeSprite(moonbowRenderer, visibleAlpha));

        // Apply sit offset ONCE during transition
        Vector3 pos = _lunaRest.transform.position;
        pos.y -= moonbowSitOffset;
        _lunaRest.transform.position = pos;

        if (debugLogs) Debug.Log("🌙 Moonbow active");
    }

    // --- HOLD MOONBOW AFTER REST ---
    private IEnumerator HoldMoonbowThenFadeBack()
    {
        if (debugLogs)
            Debug.Log($"💤 Holding Moonbow for {moonbowHoldDuration}s");

        _holdingMoonbow = true;
        float elapsed = 0f;

        while (elapsed < moonbowHoldDuration)
        {
            if (requireLightSource && IsNearLightSource())
            {
                if (debugLogs) Debug.Log("✨ Light source nearby — Moonbow stays.");
                yield break; // Do NOT fade back
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        _holdingMoonbow = false;
        StartCoroutine(TransitionBackToMyst());
    }

    // --- TRANSITION BACK TO MYST ---
    private IEnumerator TransitionBackToMyst()
    {
        if (debugLogs) Debug.Log("💤 Moonbow → Myst");

        yield return StartCoroutine(FadeSprite(moonbowRenderer, hiddenAlpha));

        if (moonbowSparkles != null)
            moonbowSparkles.SetActive(false);
        if (moonbowRenderer != null)
            moonbowRenderer.gameObject.SetActive(false);

        yield return new WaitForSeconds(mistReturnDelay);

        if (sparkles != null)
            sparkles.SetActive(true);

        yield return StartCoroutine(FadeSprite(mystRenderer, visibleAlpha));
    }

    // --- LIGHT CHECK ---
    private bool IsNearLightSource()
    {
        return Physics2D.OverlapCircle(transform.position, lightCheckRadius, lightLayerMask);
    }

    // --- FADE HELPER ---
    private IEnumerator FadeSprite(SpriteRenderer sr, float targetAlpha)
    {
        if (sr == null) yield break;

        Color c = sr.color;

        while (Mathf.Abs(c.a - targetAlpha) > 0.01f)
        {
            c.a = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * fadeSpeed);
            sr.color = c;
            yield return null;
        }

        c.a = targetAlpha;
        sr.color = c;
    }

    private void SetAlpha(SpriteRenderer sr, float alpha)
    {
        if (sr == null) return;
        Color c = sr.color;
        c.a = alpha;
        sr.color = c;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.7f, 0.9f, 1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, activationRadius);

        if (requireLightSource)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.25f);
            Gizmos.DrawWireSphere(transform.position, lightCheckRadius);
        }
    }
}



/* using UnityEngine;
using System.Collections;

public class MystRestTransitionAuto : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer mystRenderer;
    public GameObject sparkles;
    public SpriteRenderer moonbowRenderer;
    public GameObject moonbowSparkles;

    [Header("Fade Settings")]
    [Tooltip("How fast sprites fade in/out.")]
    public float fadeSpeed = 1.5f;
    [Range(0f, 1f)] public float visibleAlpha = 1f;
    [Range(0f, 1f)] public float hiddenAlpha = 0f;

    [Header("Timing")]
    [Tooltip("Delay between Myst fading out and Moonbow beginning to fade in.")]
    public float moonbowDelay = 0.8f;
    [Tooltip("Delay before Myst returns after Moonbow fades out.")]
    public float mistReturnDelay = 1.5f;

    [Header("Detection Settings")]
    public float activationRadius = 2.5f;
    public string lunaTag = "Player";
    public bool debugLogs = false;

    private LunaRest _lunaRest;
    private bool _transitionComplete;
    private Coroutine _fadeRoutine;

    void Start()
    {
        // Ensure Moonbow starts hidden and inactive
        if (moonbowRenderer != null)
        {
            SetAlpha(moonbowRenderer, 0f);
            moonbowRenderer.gameObject.SetActive(false);
        }

        if (moonbowSparkles != null)
            moonbowSparkles.SetActive(false);
    }

    void Update()
    {
        // Cache LunaRest
        if (_lunaRest == null)
        {
            GameObject luna = GameObject.FindGameObjectWithTag(lunaTag);
            if (luna != null)
                _lunaRest = luna.GetComponent<LunaRest>();
        }
        if (_lunaRest == null) return;

        // Check distance
        float dist = Vector2.Distance(transform.position, _lunaRest.transform.position);
        bool lunaInRange = dist <= activationRadius;

        // Check if Myst is sparkling
        bool sparkling = sparkles != null && sparkles.activeInHierarchy;
        bool shouldTransition = lunaInRange && sparkling && _lunaRest.isResting;

        if (shouldTransition && !_transitionComplete)
        {
            StartCoroutine(TransitionToMoonbow());
            _transitionComplete = true;
        }
        else if (!shouldTransition && _transitionComplete && !_lunaRest.isResting)
        {
            StartCoroutine(TransitionBackToMyst());
            _transitionComplete = false;
        }
    }

    private IEnumerator TransitionToMoonbow()
    {
        if (debugLogs)
            Debug.Log($"🌙 {name}: Luna resting in sparkling Myst — fading to Moonbow.");

        // Fade Myst and its sparkles out
        if (sparkles != null)
            sparkles.SetActive(false);
        yield return StartCoroutine(FadeSprite(mystRenderer, hiddenAlpha));

        // Wait for mystical delay before revealing Moonbow
        yield return new WaitForSeconds(moonbowDelay);

        // Enable and fade in Moonbow + sparkles together
        if (moonbowRenderer != null && !moonbowRenderer.gameObject.activeSelf)
            moonbowRenderer.gameObject.SetActive(true);
        if (moonbowSparkles != null && !moonbowSparkles.activeSelf)
            moonbowSparkles.SetActive(true);

        yield return StartCoroutine(FadeSprite(moonbowRenderer, visibleAlpha));

        if (debugLogs)
            Debug.Log($"🌙 {name}: Myst fully transformed into Moonbow.");
    }

    private IEnumerator TransitionBackToMyst()
    {
        if (debugLogs)
            Debug.Log($"💤 {name}: Luna stopped resting — fading back to Myst.");

        // Fade Moonbow out first
        yield return StartCoroutine(FadeSprite(moonbowRenderer, hiddenAlpha));

        // Disable Moonbow visuals
        if (moonbowSparkles != null)
            moonbowSparkles.SetActive(false);
        if (moonbowRenderer != null)
            moonbowRenderer.gameObject.SetActive(false);

        // ⏳ Wait before bringing Myst back
        yield return new WaitForSeconds(mistReturnDelay);

        // Fade Myst back in
        if (sparkles != null)
            sparkles.SetActive(true);
        yield return StartCoroutine(FadeSprite(mystRenderer, visibleAlpha));

        if (debugLogs)
            Debug.Log($"💤 {name}: Moonbow faded back to Myst after delay.");
    }

    private IEnumerator FadeSprite(SpriteRenderer sr, float targetAlpha)
    {
        if (sr == null) yield break;

        Color c = sr.color;
        while (Mathf.Abs(c.a - targetAlpha) > 0.01f)
        {
            c.a = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * fadeSpeed);
            sr.color = c;
            yield return null;
        }

        c.a = targetAlpha;
        sr.color = c;
    }

    private void SetAlpha(SpriteRenderer sr, float alpha)
    {
        if (sr == null) return;
        Color c = sr.color;
        c.a = alpha;
        sr.color = c;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.7f, 0.9f, 1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, activationRadius);
    }
}
*/