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

    [Header("General Alpha Settings")]
    [Range(0f, 1f)] public float visibleAlpha = 1f;
    [Range(0f, 1f)] public float hiddenAlpha = 0f;

    [Header("Separate Fade Speeds")]
    public float mistFadeOutSpeed = 4f;
    public float mistFadeInSpeed = 5f;
    public float moonbowFadeInSpeed = 2.25f;
    public float moonbowFadeOutSpeed = 2.5f;

    [Header("Transition Overlap")]
    [Tooltip("How long after mist starts fading out before moonbow starts fading in.")]
    public float moonbowFadeInLeadDelay = 0.03f;

    [Tooltip("How long after moonbow starts fading out before mist starts fading back in.")]
    public float mistFadeInLeadDelay = 0.03f;

    [Header("Stability / Visibility Options")]
    [Tooltip("If true, the mist never fades all the way out.")]
    public bool neverFullyHideMist = false;

    [Tooltip("Minimum alpha the mist can fade to when neverFullyHideMist is true.")]
    [Range(0f, 1f)] public float mistMinimumAlpha = 0.2f;

    [Tooltip("If true, sparkles stay visible instead of disappearing during transitions.")]
    public bool keepSparklesVisibleAlways = true;

    [Tooltip("If true, moonbow sparkles stay visible whenever the moonbow is in use.")]
    public bool keepMoonbowSparklesVisibleAlways = true;

    [Tooltip("If true, the moonbow object is never deactivated, only faded. Usually more stable.")]
    public bool neverDeactivateMoonbowObject = true;

    [Tooltip("If true, the mist object is never deactivated, only faded. Usually more stable.")]
    public bool neverDeactivateMystObject = true;

    [Header("Timing")]
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

    [Header("Failsafe Refresh")]
    [Tooltip("How often to re-assert visual/collider state even if nothing changed.")]
    public float failsafeRefreshInterval = 0.2f;

    private LunaRest _lunaRest;
    private Transform _lunaTransform;

    private bool _transitionComplete = false;
    private bool _holdingMoonbow = false;
    private bool _moonbowShouldBeSolid = false;
    private bool _isTransitioningIn = false;
    private bool _isTransitioningOut = false;
    private bool _pageRevealDoneThisCycle = false;

    private float _timeInRange = 0f;
    private float _failsafeTimer = 0f;

    private Coroutine _transitionRoutine;
    private Coroutine _returnRoutine;

    private void Start()
    {
        ResolveBookController();
        ResolveLuna();
        ResolveMoonbowColliders();

        InitializeVisualState();
        ApplyMoonbowColliderStateImmediate(false);

        _moonbowShouldBeSolid = false;
        _timeInRange = 0f;
        _failsafeTimer = 0f;
    }

    private void Update()
    {
        ResolveLuna();

        if (bookController == null)
            ResolveBookController();

        if (moonbowColliders == null || moonbowColliders.Length == 0)
            ResolveMoonbowColliders();

        UpdateTimeInRange();

        bool shouldTransition = ShouldBeInMoonbowState();

        if (shouldTransition && !_transitionComplete && !_isTransitioningIn)
        {
            StartTransitionIn();
        }
        else if (!shouldTransition && _transitionComplete && !_holdingMoonbow && !_isTransitioningOut)
        {
            StartTransitionOutWithHold();
        }

        _failsafeTimer += Time.deltaTime;
        if (_failsafeTimer >= failsafeRefreshInterval)
        {
            _failsafeTimer = 0f;
            EnforceCurrentState();
        }
    }

    private void LateUpdate()
    {
        ApplyMoonbowColliderStateImmediate(_moonbowShouldBeSolid);
    }

    private void ResolveLuna()
    {
        if (_lunaRest != null && _lunaTransform != null)
            return;

        GameObject luna = GameObject.FindGameObjectWithTag(lunaTag);
        if (luna != null)
        {
            _lunaTransform = luna.transform;

            if (_lunaRest == null)
                _lunaRest = luna.GetComponent<LunaRest>();
        }
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

        if (!autoFindMoonbowColliders || moonbowRenderer == null)
            return;

        moonbowColliders = moonbowRenderer.GetComponentsInChildren<Collider2D>(true);

        if (debugLogs)
        {
            int count = moonbowColliders != null ? moonbowColliders.Length : 0;
            Debug.Log($"{name}: Auto-resolved {count} moonbow collider(s).");
        }
    }

    private void InitializeVisualState()
    {
        if (mystRenderer != null)
        {
            if (!neverDeactivateMystObject)
                mystRenderer.gameObject.SetActive(true);

            SetAlpha(mystRenderer, visibleAlpha);
        }

        if (sparkles != null)
        {
            sparkles.SetActive(true);
        }

        if (moonbowRenderer != null)
        {
            moonbowRenderer.gameObject.SetActive(true);
            SetAlpha(moonbowRenderer, hiddenAlpha);

            if (!neverDeactivateMoonbowObject && Mathf.Approximately(hiddenAlpha, 0f))
                moonbowRenderer.gameObject.SetActive(false);
        }

        if (moonbowSparkles != null)
        {
            moonbowSparkles.SetActive(false);
        }
    }

    private void UpdateTimeInRange()
    {
        if (_lunaTransform == null)
        {
            _timeInRange = 0f;
            return;
        }

        bool lunaInRange = LunaInRange();

        if (lunaInRange)
            _timeInRange += Time.deltaTime;
        else
            _timeInRange = 0f;
    }

    private bool LunaInRange()
    {
        if (_lunaTransform == null)
            return false;

        float dist = Vector2.Distance(transform.position, _lunaTransform.position);
        return dist <= activationRadius;
    }

    private bool ShouldBeInMoonbowState()
    {
        if (_lunaTransform == null)
            return false;

        bool lunaInRange = LunaInRange();
        bool stoodLongEnough = _timeInRange >= standInMistDuration;

        if (!lunaInRange)
            return false;

        if (!_transitionComplete)
            return stoodLongEnough;

        if (stayActiveWhileLunaInRange)
            return true;

        return stoodLongEnough;
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
        _isTransitioningIn = false;
        _isTransitioningOut = false;
    }

    private IEnumerator TransitionToMoonbow()
    {
        _isTransitioningIn = true;
        _transitionComplete = true;
        _pageRevealDoneThisCycle = false;

        if (debugLogs)
            Debug.Log($"🌙 {name}: TransitionToMoonbow()");

        SetMoonbowSolid(false);

        if (mystRenderer != null && !mystRenderer.gameObject.activeSelf)
            mystRenderer.gameObject.SetActive(true);

        if (moonbowRenderer != null)
            moonbowRenderer.gameObject.SetActive(true);

        if (sparkles != null)
            sparkles.SetActive(true);

        if (moonbowSparkles != null)
            moonbowSparkles.SetActive(true);

        float mistTargetAlpha = neverFullyHideMist ? Mathf.Max(hiddenAlpha, mistMinimumAlpha) : hiddenAlpha;

        float currentDelay = 0f;

        while (true)
        {
            bool mistDone = true;
            bool moonbowDone = true;

            if (mystRenderer != null)
            {
                float newMistAlpha = Mathf.MoveTowards(mystRenderer.color.a, mistTargetAlpha, mistFadeOutSpeed * Time.deltaTime);
                SetAlpha(mystRenderer, newMistAlpha);
                mistDone = Mathf.Approximately(newMistAlpha, mistTargetAlpha);
            }

            currentDelay += Time.deltaTime;

            if (currentDelay >= moonbowFadeInLeadDelay)
            {
                if (moonbowRenderer != null)
                {
                    float newMoonbowAlpha = Mathf.MoveTowards(moonbowRenderer.color.a, visibleAlpha, moonbowFadeInSpeed * Time.deltaTime);
                    SetAlpha(moonbowRenderer, newMoonbowAlpha);
                    moonbowDone = Mathf.Approximately(newMoonbowAlpha, visibleAlpha);
                }
            }
            else
            {
                moonbowDone = false;
            }

            if (mistDone && moonbowDone)
                break;

            yield return null;
        }

        SetMoonbowSolid(true);

        if (!applySitOffsetOnRestOnly && _lunaTransform != null)
        {
            Vector3 pos = _lunaTransform.position;
            pos.y -= moonbowSitOffset;
            _lunaTransform.position = pos;
        }

        if (!_pageRevealDoneThisCycle)
        {
            TryRevealPage();
            _pageRevealDoneThisCycle = true;
        }

        _isTransitioningIn = false;
        _transitionRoutine = null;

        EnforceCurrentState();

        if (debugLogs)
            Debug.Log($"🌙 {name}: Moonbow active");
    }

    private IEnumerator HoldMoonbowThenFadeBack()
    {
        _isTransitioningOut = true;
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

        _isTransitioningOut = false;
        _returnRoutine = null;
    }

    private IEnumerator TransitionBackToMyst()
    {
        if (debugLogs)
            Debug.Log($"💤 {name}: TransitionBackToMyst()");

        SetMoonbowSolid(false);

        if (mystRenderer != null && !mystRenderer.gameObject.activeSelf)
            mystRenderer.gameObject.SetActive(true);

        if (moonbowRenderer != null && !moonbowRenderer.gameObject.activeSelf)
            moonbowRenderer.gameObject.SetActive(true);

        if (sparkles != null)
            sparkles.SetActive(true);

        float mistTargetAlpha = visibleAlpha;
        float moonbowTargetAlpha = hiddenAlpha;

        float currentDelay = 0f;

        while (true)
        {
            bool mistDone = true;
            bool moonbowDone = true;

            if (moonbowRenderer != null)
            {
                float newMoonbowAlpha = Mathf.MoveTowards(moonbowRenderer.color.a, moonbowTargetAlpha, moonbowFadeOutSpeed * Time.deltaTime);
                SetAlpha(moonbowRenderer, newMoonbowAlpha);
                moonbowDone = Mathf.Approximately(newMoonbowAlpha, moonbowTargetAlpha);
            }

            currentDelay += Time.deltaTime;

            if (currentDelay >= mistFadeInLeadDelay)
            {
                if (mystRenderer != null)
                {
                    float newMistAlpha = Mathf.MoveTowards(mystRenderer.color.a, mistTargetAlpha, mistFadeInSpeed * Time.deltaTime);
                    SetAlpha(mystRenderer, newMistAlpha);
                    mistDone = Mathf.Approximately(newMistAlpha, mistTargetAlpha);
                }
            }
            else
            {
                mistDone = false;
            }

            if (moonbowDone && mistDone)
                break;

            yield return null;
        }

        if (moonbowSparkles != null)
        {
            if (keepMoonbowSparklesVisibleAlways && moonbowRenderer != null && moonbowRenderer.color.a > 0.01f)
                moonbowSparkles.SetActive(true);
            else
                moonbowSparkles.SetActive(false);
        }

        if (moonbowRenderer != null && !neverDeactivateMoonbowObject && moonbowRenderer.color.a <= 0.01f)
            moonbowRenderer.gameObject.SetActive(false);

        _transitionComplete = false;
        _timeInRange = 0f;
        _pageRevealDoneThisCycle = false;

        EnforceCurrentState();
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

    private void EnforceCurrentState()
    {
        if (_transitionComplete || _isTransitioningIn || _holdingMoonbow || _isTransitioningOut)
        {
            if (moonbowRenderer != null && !moonbowRenderer.gameObject.activeSelf)
                moonbowRenderer.gameObject.SetActive(true);

            if (sparkles != null && keepSparklesVisibleAlways)
                sparkles.SetActive(true);

            if (moonbowSparkles != null && keepMoonbowSparklesVisibleAlways)
            {
                if (moonbowRenderer != null && moonbowRenderer.color.a > 0.01f)
                    moonbowSparkles.SetActive(true);
            }

            if (neverFullyHideMist && mystRenderer != null)
            {
                Color c = mystRenderer.color;
                if (c.a < mistMinimumAlpha)
                {
                    c.a = mistMinimumAlpha;
                    mystRenderer.color = c;
                }
            }
        }
        else
        {
            if (sparkles != null)
                sparkles.SetActive(true);

            if (moonbowSparkles != null && moonbowRenderer != null && moonbowRenderer.color.a <= 0.01f)
                moonbowSparkles.SetActive(false);

            if (moonbowRenderer != null && !neverDeactivateMoonbowObject && moonbowRenderer.color.a <= 0.01f)
                moonbowRenderer.gameObject.SetActive(false);
        }

        ApplyMoonbowColliderStateImmediate(_moonbowShouldBeSolid);
    }

    private bool IsNearLightSource()
    {
        return Physics2D.OverlapCircle(transform.position, lightCheckRadius, lightLayerMask);
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
        _isTransitioningIn = false;
        _isTransitioningOut = false;
        _timeInRange = 0f;
        _pageRevealDoneThisCycle = false;
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