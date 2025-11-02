using UnityEngine;
using System.Collections;
using System.Linq;

[RequireComponent(typeof(SpriteRenderer))]
public class LunariaGlowFromLightSources_Array : MonoBehaviour
{
    [Header("Light Sources (any of these can trigger a glow)")]
    public Transform[] possibleLightSources;

    [Header("Glow Settings")]
    public float activationRadius = 2.5f;
    public float fadeSpeed = 2f;
    [Range(0f, 1f)] public float maxAlpha = 1f;
    public Color glowColor = new Color(1.15f, 1.15f, 1.3f, 1f);

    [Header("Debug")]
    public bool debugLogs = false;

    private SpriteRenderer _sr;
    private float _originalAlpha;
    private Color _originalColor;
    private bool _isGlowing;
    private Coroutine _fadeRoutine;
    private float _recentActivationTimer;

    public bool IsGlowing => _isGlowing;

    void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        _originalColor = _sr.color;
        _originalAlpha = _sr.color.a;
    }

    void Update()
    {
        if (_recentActivationTimer > 0f)
            _recentActivationTimer -= Time.deltaTime;

        bool anyNearby = CheckForAnyLightSources();

        // grace window keeps glow stable when sparkles toggle briefly
        if (!anyNearby && _recentActivationTimer > 0f)
            anyNearby = true;

        if (anyNearby != _isGlowing)
        {
            _isGlowing = anyNearby;
            StartFade(_isGlowing);
            if (_isGlowing)
                _recentActivationTimer = 0.2f; // 200ms buffer
            if (debugLogs)
                Debug.Log($"[{name}] → {(_isGlowing ? "glow ON" : "glow OFF")}");
        }
    }

    private bool CheckForAnyLightSources()
    {
        foreach (var src in possibleLightSources)
        {
            if (src == null) continue;
            if (!src.gameObject.activeInHierarchy) continue;

            float dist = Vector2.Distance(transform.position, src.position);
            if (dist <= activationRadius)
                return true;
        }
        return false;
    }

    private void StartFade(bool toGlow)
    {
        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeToAlpha(toGlow ? maxAlpha : _originalAlpha, toGlow));
    }

    private IEnumerator FadeToAlpha(float target, bool toGlow)
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
        Gizmos.color = new Color(0.7f, 0.9f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, activationRadius);
    }
}
