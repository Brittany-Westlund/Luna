using UnityEngine;
using System.Collections;
using System.Linq;

[RequireComponent(typeof(SpriteRenderer))]
public class LunariaGlowFromSparkles_Radius : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Radius around the Lunaria to detect sparkles.")]
    public float activationRadius = 2.5f;

    [Tooltip("Optional layer filter for sparkles.")]
    public LayerMask sparkleLayer; // assign in inspector to your Sparkle layer

    [Tooltip("If true, will only react to sparkles that are currently active in the hierarchy.")]
    public bool requireActiveSparkles = true;

    [Tooltip("Optional linger delay after sparkles leave range (in seconds).")]
    public float lingerDuration = 1f;

    [Header("Glow Settings")]
    [Tooltip("Speed of fade in/out transitions.")]
    public float fadeSpeed = 2f;

    [Range(0f, 1f)] public float maxAlpha = 1f;

    [Tooltip("Color of the glow when active.")]
    public Color glowColor = new Color(1.15f, 1.15f, 1.3f, 1f);

    [Header("Debug")]
    public bool debugLogs = false;
    public bool drawDebugGizmos = true;

    // internal
    private SpriteRenderer _sr;
    private float _originalAlpha;
    private Color _originalColor;
    private bool _isGlowing;
    private Coroutine _fadeRoutine;
    private float _lastSparkleTime;

    void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        _originalColor = _sr.color;
        _originalAlpha = _sr.color.a;
    }

    void Update()
    {
        bool sparkleNearby = CheckForSparklesNearby();
        if (sparkleNearby)
        {
            _lastSparkleTime = Time.time;
        }

        bool shouldGlow = Time.time - _lastSparkleTime < lingerDuration;

        if (shouldGlow != _isGlowing)
        {
            _isGlowing = shouldGlow;
            StartFade(_isGlowing);
            if (debugLogs)
                Debug.Log($"[{name}] → sparkle glow {(_isGlowing ? "ON" : "OFF")}");
        }
    }

    bool CheckForSparklesNearby()
    {
        Collider2D[] hits = sparkleLayer.value == 0
            ? Physics2D.OverlapCircleAll(transform.position, activationRadius)
            : Physics2D.OverlapCircleAll(transform.position, activationRadius, sparkleLayer);

        if (hits == null || hits.Length == 0) return false;

        foreach (var hit in hits)
        {
            if (!hit) continue;

            // use layer match as primary; tag fallback for safety
            bool isSparkle = hit.CompareTag("Sparkles") || ((1 << hit.gameObject.layer) & sparkleLayer) != 0;
            if (!isSparkle) continue;

            // only count if active
            if (!requireActiveSparkles || hit.gameObject.activeInHierarchy)
                return true;
        }

        return false;
    }

    void StartFade(bool toGlow)
    {
        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeToAlpha(toGlow ? maxAlpha : _originalAlpha, toGlow));
    }

    IEnumerator FadeToAlpha(float target, bool toGlow)
    {
        Color start = _sr.color;
        Color end = toGlow ? glowColor : _originalColor;

        while (Mathf.Abs(_sr.color.a - target) > 0.01f)
        {
            Color c = Color.Lerp(_sr.color, end, Time.deltaTime * fadeSpeed);
            c.a = Mathf.Lerp(_sr.color.a, target, Time.deltaTime * fadeSpeed);
            _sr.color = c;
            yield return null;
        }

        end.a = target;
        _sr.color = end;
    }

    void OnDrawGizmosSelected()
    {
        if (!drawDebugGizmos) return;
        Gizmos.color = new Color(1f, 1f, 0.6f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, activationRadius);
    }
}
