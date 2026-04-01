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

    [Header("Moonbow Colliders")]
    public Collider2D[] moonbowColliders;
    public bool autoFindMoonbowColliders = true;

    [Header("Book Reveal")]
    public string locationId;
    public BookControllerSimple bookController;
    public bool revealPageOnMoonbowAppear = true;

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

    [Header("Silvermist Standing Activation")]
    [Tooltip("If Luna stays within activationRadius for at least this many seconds, the moonbow activates.")]
    public float standInMistDuration = 2f;

    [Header("Moonbow Positioning")]
    public float moonbowSitOffset = 0.4f;
    public bool applySitOffsetOnRestOnly = true;

    [Header("Stay Active Behavior")]
    [Tooltip("If true, the moonbow stays active as long as Luna remains in range after activation.")]
    public bool stayActiveWhileLunaInRange = true;

    private LunaRest _lunaRest;

    private bool _transitionComplete = false;
    private bool _holdingMoonbow = false;
    private bool _moonbowShouldBeSolid = false;

    private float _timeInRange = 0f;

    private Coroutine _transitionRoutine;
    private Coroutine _returnRoutine;

    private void Start()
    {
        ResolveBookController();
        ResolveMoonbowColliders();

        if (moonbowRenderer != null)
        {
            SetAlpha(moonbowRenderer, hiddenAlpha);
            moonbowRenderer.gameObject.SetActive(false);
        }

        if (moonbowSparkles != null)
            moonbowSparkles.SetActive(false);

        if (mystRenderer != null)
            SetAlpha(mystRenderer, visibleAlpha);

        _moonbowShouldBeSolid = false;
        ApplyMoonbowColliderStateImmediate(false);
        _timeInRange = 0f;
    }

    private void Update()
    {
        if (_lunaRest == null)
        {
            GameObject luna = GameObject.FindGameObjectWithTag(lunaTag);
            if (luna != null)
                _lunaRest = luna.GetComponent<LunaRest>();
        }

        if (bookController == null)
            ResolveBookController();

        UpdateTimeInRange();

        bool shouldTransition = ShouldBeInMoonbowState();

        if (shouldTransition && !_transitionComplete)
        {
            StartTransitionIn();
        }
        else if (!shouldTransition && _transitionComplete && !_holdingMoonbow)
        {
            StartTransitionOutWithHold();
        }
    }

    private void LateUpdate()
    {
        ApplyMoonbowColliderStateImmediate(_moonbowShouldBeSolid);
    }

    private void UpdateTimeInRange()
    {
        if (_lunaRest == null)
        {
            _timeInRange = 0f;
            return;
        }

        float dist = Vector2.Distance(transform.position, _lunaRest.transform.position);
        bool lunaInRange = dist <= activationRadius;

        if (lunaInRange)
            _timeInRange += Time.deltaTime;
        else
            _timeInRange = 0f;
    }

    private bool LunaInRange()
    {
        if (_lunaRest == null)
            return false;

        float dist = Vector2.Distance(transform.position, _lunaRest.transform.position);
        return dist <= activationRadius;
    }

    private bool ShouldBeInMoonbowState()
    {
        if (_lunaRest == null)
            return false;

        bool lunaInRange = LunaInRange();
        bool stoodLongEnough = _timeInRange >= standInMistDuration;

        if (!lunaInRange)
            return false;

        // Activate once Luna has stood in the mist long enough.
        if (!_transitionComplete)
            return stoodLongEnough;

        // After activation, optionally keep the moonbow alive while Luna remains in range.
        if (stayActiveWhileLunaInRange)
            return true;

        return stoodLongEnough;
    }

    private void ResolveBookController()
    {
        if (bookController == null)
            bookController = FindObjectOfType<BookControllerSimple>();
    }

    private void ResolveMoonbowColliders()
    {
        if (moonbowColliders != null && moonbowColliders.Length > 0)
        {
            if (debugLogs)
                Debug.Log($"{name}: Using manually assigned moonbow colliders: {moonbowColliders.Length}");
            return;
        }

        if (!autoFindMoonbowColliders)
            return;

        if (moonbowRenderer == null)
            return;

        moonbowColliders = moonbowRenderer.GetComponentsInChildren<Collider2D>(true);

        if (debugLogs)
        {
            int count = moonbowColliders != null ? moonbowColliders.Length : 0;
            Debug.Log($"{name}: Auto-resolved {count} moonbow collider(s).");
        }
    }

    private void ApplyMoonbowColliderStateImmediate(bool active)
    {
        if (moonbowColliders == null)
            return;

        for (int i = 0; i < moonbowColliders.Length; i++)
        {
            if (moonbowColliders[i] == null)
                continue;

            if (moonbowColliders[i].enabled != active)
            {
                moonbowColliders[i].enabled = active;

                if (debugLogs)
                    Debug.Log($"{name}: Collider '{moonbowColliders[i].name}' -> {active}");
            }
        }
    }

    private void SetMoonbowSolid(bool solid)
    {
        _moonbowShouldBeSolid = solid;
        ApplyMoonbowColliderStateImmediate(solid);

        if (debugLogs)
            Debug.Log($"{name}: Moonbow solid state -> {solid}");
    }

    private void StartTransitionIn()
    {
        StopAllManagedCoroutines();
        _transitionRoutine = StartCoroutine(TransitionToMoonbow());
    }

    private void StartTransitionOutWithHold()
    {
        StopAllManagedCoroutines();
        _returnRoutine = StartCoroutine(HoldMoonbowThenFadeBack());
    }

    private void StopAllManagedCoroutines()
    {
        if (_transitionRoutine != null)
        {
            StopCoroutine(_transitionRoutine);
            _transitionRoutine = null;
        }

        if (_returnRoutine != null)
        {
            StopCoroutine(_returnRoutine);
            _returnRoutine = null;
        }

        _holdingMoonbow = false;
    }

    private IEnumerator TransitionToMoonbow()
    {
        _transitionComplete = true;

        if (debugLogs)
            Debug.Log($"🌙 {name}: TransitionToMoonbow()");

        SetMoonbowSolid(false);

        if (sparkles != null)
            sparkles.SetActive(false);

        yield return StartCoroutine(FadeSprite(mystRenderer, hiddenAlpha));
        yield return new WaitForSeconds(moonbowDelay);

        if (moonbowRenderer != null)
            moonbowRenderer.gameObject.SetActive(true);

        if (moonbowSparkles != null)
            moonbowSparkles.SetActive(true);

        yield return StartCoroutine(FadeSprite(moonbowRenderer, visibleAlpha));

        SetMoonbowSolid(true);

        if (!applySitOffsetOnRestOnly)
        {
            if (_lunaRest != null)
            {
                Vector3 pos = _lunaRest.transform.position;
                pos.y -= moonbowSitOffset;
                _lunaRest.transform.position = pos;
            }
        }

        TryRevealPage();

        _transitionRoutine = null;

        if (debugLogs)
            Debug.Log($"🌙 {name}: Moonbow active");
    }

    private void TryRevealPage()
    {
        if (!revealPageOnMoonbowAppear)
            return;

        if (string.IsNullOrEmpty(locationId))
        {
            if (debugLogs)
                Debug.LogWarning($"{name}: No locationId set, so no page reveal was attempted.");
            return;
        }

        ResolveBookController();

        if (bookController == null)
        {
            Debug.LogWarning($"{name}: Could not find BookControllerSimple, so page reveal could not occur.");
            return;
        }

        bool revealed = bookController.RevealNextFromLocation(locationId);

        if (debugLogs)
            Debug.Log($"🌙 {name}: RevealNextFromLocation({locationId}) -> {revealed}");
    }

    private IEnumerator HoldMoonbowThenFadeBack()
    {
        _holdingMoonbow = true;

        if (debugLogs)
            Debug.Log($"💤 {name}: holding Moonbow for {moonbowHoldDuration}s");

        float elapsed = 0f;

        while (elapsed < moonbowHoldDuration)
        {
            if (stayActiveWhileLunaInRange && LunaInRange())
            {
                elapsed = 0f;
            }

            if (requireLightSource && IsNearLightSource())
            {
                if (debugLogs)
                    Debug.Log($"✨ {name}: light source nearby, Moonbow stays.");

                elapsed = 0f;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        _holdingMoonbow = false;
        yield return StartCoroutine(TransitionBackToMyst());

        _returnRoutine = null;
    }

    private IEnumerator TransitionBackToMyst()
    {
        if (debugLogs)
            Debug.Log($"💤 {name}: TransitionBackToMyst()");

        SetMoonbowSolid(false);

        yield return StartCoroutine(FadeSprite(moonbowRenderer, hiddenAlpha));

        if (moonbowSparkles != null)
            moonbowSparkles.SetActive(false);

        if (moonbowRenderer != null)
            moonbowRenderer.gameObject.SetActive(false);

        yield return new WaitForSeconds(mistReturnDelay);

        if (sparkles != null)
            sparkles.SetActive(true);

        yield return StartCoroutine(FadeSprite(mystRenderer, visibleAlpha));

        _transitionComplete = false;
        _timeInRange = 0f;
    }

    private bool IsNearLightSource()
    {
        return Physics2D.OverlapCircle(transform.position, lightCheckRadius, lightLayerMask);
    }

    private IEnumerator FadeSprite(SpriteRenderer sr, float targetAlpha)
    {
        if (sr == null)
            yield break;

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
        if (sr == null)
            return;

        Color c = sr.color;
        c.a = alpha;
        sr.color = c;
    }

    private void OnDisable()
    {
        StopAllManagedCoroutines();

        SetMoonbowSolid(false);

        _holdingMoonbow = false;
        _transitionComplete = false;
        _timeInRange = 0f;
    }

    private void OnDrawGizmosSelected()
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