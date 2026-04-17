using UnityEngine;
using System.Collections;

public class MystMoonbowPlatformOnly : MonoBehaviour
{
    public enum MoonbowState
    {
        Myst,
        FadingToMoonbow,
        Moonbow,
        HoldingBeforeReturn,
        FadingToMyst
    }

    [Header("References")]
    public SpriteRenderer mystRenderer;
    public GameObject sparkles;

    [Header("Moonbow Objects")]
    public SpriteRenderer moonbowRenderer;
    public GameObject moonbowSparkles;

    [Header("Moonbow Colliders")]
    public Collider2D[] moonbowColliders;
    public bool autoFindMoonbowColliders = true;

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

    [Tooltip("If true, the moonbow object is never deactivated, only faded. Recommended.")]
    public bool neverDeactivateMoonbowObject = true;

    [Tooltip("If true, the mist object is never deactivated, only faded. Recommended.")]
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

    [Header("Stay Active Behavior")]
    [Tooltip("If true, the moonbow stays active as long as Luna remains in range after activation.")]
    public bool stayActiveWhileLunaInRange = true;

    [Header("Failsafe Refresh")]
    [Tooltip("How often to re-assert visual/collider state even if nothing changed.")]
    public float failsafeRefreshInterval = 0.2f;

    private Transform _lunaTransform;
    private MoonbowState _state = MoonbowState.Myst;

    private float _timeInRange = 0f;
    private float _holdTimer = 0f;
    private float _failsafeTimer = 0f;
    private float _transitionDelayTimer = 0f;

    private Coroutine _stateRoutine;

    private void Start()
    {
        ResolveLuna();
        ResolveMoonbowColliders();
        InitializeVisualState();
        SetMoonbowSolidImmediate(false);
        ForceStateVisuals(MoonbowState.Myst);
    }

    private void Update()
    {
        ResolveLuna();

        if (moonbowColliders == null || moonbowColliders.Length == 0)
            ResolveMoonbowColliders();

        UpdateTimeInRange();
        UpdateStateMachine();

        _failsafeTimer += Time.deltaTime;
        if (_failsafeTimer >= failsafeRefreshInterval)
        {
            _failsafeTimer = 0f;
            EnforceCurrentState();
        }
    }

    private void LateUpdate()
    {
        ApplyColliderStateForCurrentState();
    }

    private void ResolveLuna()
    {
        if (_lunaTransform != null)
            return;

        GameObject luna = GameObject.FindGameObjectWithTag(lunaTag);
        if (luna != null)
            _lunaTransform = luna.transform;
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
            mystRenderer.gameObject.SetActive(true);
            SetAlpha(mystRenderer, visibleAlpha);
        }

        if (sparkles != null)
            sparkles.SetActive(true);

        if (moonbowRenderer != null)
        {
            moonbowRenderer.gameObject.SetActive(true);
            SetAlpha(moonbowRenderer, hiddenAlpha);
        }

        if (moonbowSparkles != null)
            moonbowSparkles.SetActive(false);
    }

    private void UpdateTimeInRange()
    {
        if (_lunaTransform == null)
        {
            _timeInRange = 0f;
            return;
        }

        if (LunaInRange())
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

    private bool CanActivateMoonbow()
    {
        if (_lunaTransform == null)
            return false;

        return LunaInRange() && _timeInRange >= standInMistDuration;
    }

    private bool ShouldRemainMoonbow()
    {
        if (requireLightSource && IsNearLightSource())
            return true;

        if (stayActiveWhileLunaInRange && LunaInRange())
            return true;

        return false;
    }

    private void UpdateStateMachine()
    {
        switch (_state)
        {
            case MoonbowState.Myst:
                if (CanActivateMoonbow())
                    ChangeState(MoonbowState.FadingToMoonbow);
                break;

            case MoonbowState.FadingToMoonbow:
                break;

            case MoonbowState.Moonbow:
                if (!ShouldRemainMoonbow())
                    ChangeState(MoonbowState.HoldingBeforeReturn);
                break;

            case MoonbowState.HoldingBeforeReturn:
                if (ShouldRemainMoonbow())
                {
                    _holdTimer = 0f;
                    ChangeState(MoonbowState.Moonbow);
                }
                else
                {
                    _holdTimer += Time.deltaTime;
                    if (_holdTimer >= moonbowHoldDuration)
                        ChangeState(MoonbowState.FadingToMyst);
                }
                break;

            case MoonbowState.FadingToMyst:
                break;
        }
    }

    private void ChangeState(MoonbowState newState)
    {
        if (_state == newState)
            return;

        if (debugLogs)
            Debug.Log($"{name}: State {_state} -> {newState}");

        if (_stateRoutine != null)
        {
            StopCoroutine(_stateRoutine);
            _stateRoutine = null;
        }

        _state = newState;
        _transitionDelayTimer = 0f;

        switch (_state)
        {
            case MoonbowState.Myst:
                _holdTimer = 0f;
                ForceStateVisuals(MoonbowState.Myst);
                break;

            case MoonbowState.FadingToMoonbow:
                _holdTimer = 0f;
                _stateRoutine = StartCoroutine(FadeToMoonbowRoutine());
                break;

            case MoonbowState.Moonbow:
                _holdTimer = 0f;
                ForceStateVisuals(MoonbowState.Moonbow);
                break;

            case MoonbowState.HoldingBeforeReturn:
                _holdTimer = 0f;
                ForceStateVisuals(MoonbowState.Moonbow);
                break;

            case MoonbowState.FadingToMyst:
                _stateRoutine = StartCoroutine(FadeToMystRoutine());
                break;
        }
    }

    private IEnumerator FadeToMoonbowRoutine()
    {
        EnsureObjectsActiveForTransition();

        float mistTargetAlpha = neverFullyHideMist
            ? Mathf.Max(hiddenAlpha, mistMinimumAlpha)
            : hiddenAlpha;

        _transitionDelayTimer = 0f;
        SetMoonbowSolidImmediate(false);

        while (true)
        {
            bool mistDone = true;
            bool moonbowDone = true;

            if (_state != MoonbowState.FadingToMoonbow)
                yield break;

            if (mystRenderer != null)
            {
                float newMistAlpha = Mathf.MoveTowards(
                    mystRenderer.color.a,
                    mistTargetAlpha,
                    mistFadeOutSpeed * Time.deltaTime
                );

                SetAlpha(mystRenderer, newMistAlpha);
                mistDone = Mathf.Approximately(newMistAlpha, mistTargetAlpha);
            }

            _transitionDelayTimer += Time.deltaTime;

            if (_transitionDelayTimer >= moonbowFadeInLeadDelay)
            {
                if (moonbowRenderer != null)
                {
                    float newMoonbowAlpha = Mathf.MoveTowards(
                        moonbowRenderer.color.a,
                        visibleAlpha,
                        moonbowFadeInSpeed * Time.deltaTime
                    );

                    SetAlpha(moonbowRenderer, newMoonbowAlpha);
                    moonbowDone = Mathf.Approximately(newMoonbowAlpha, visibleAlpha);
                }
            }
            else
            {
                moonbowDone = false;
            }

            UpdateSparklesDuringUse();

            if (mistDone && moonbowDone)
                break;

            yield return null;
        }

        _stateRoutine = null;
        ChangeState(MoonbowState.Moonbow);
    }

    private IEnumerator FadeToMystRoutine()
    {
        EnsureObjectsActiveForTransition();

        _transitionDelayTimer = 0f;
        SetMoonbowSolidImmediate(false);

        while (true)
        {
            bool mistDone = true;
            bool moonbowDone = true;

            if (_state != MoonbowState.FadingToMyst)
                yield break;

            if (moonbowRenderer != null)
            {
                float newMoonbowAlpha = Mathf.MoveTowards(
                    moonbowRenderer.color.a,
                    hiddenAlpha,
                    moonbowFadeOutSpeed * Time.deltaTime
                );

                SetAlpha(moonbowRenderer, newMoonbowAlpha);
                moonbowDone = Mathf.Approximately(newMoonbowAlpha, hiddenAlpha);
            }

            _transitionDelayTimer += Time.deltaTime;

            if (_transitionDelayTimer >= mistFadeInLeadDelay)
            {
                if (mystRenderer != null)
                {
                    float newMistAlpha = Mathf.MoveTowards(
                        mystRenderer.color.a,
                        visibleAlpha,
                        mistFadeInSpeed * Time.deltaTime
                    );

                    SetAlpha(mystRenderer, newMistAlpha);
                    mistDone = Mathf.Approximately(newMistAlpha, visibleAlpha);
                }
            }
            else
            {
                mistDone = false;
            }

            UpdateSparklesDuringUse();

            if (moonbowDone && mistDone)
                break;

            yield return null;
        }

        _timeInRange = 0f;
        _holdTimer = 0f;
        _stateRoutine = null;
        ChangeState(MoonbowState.Myst);
    }

    private void EnsureObjectsActiveForTransition()
    {
        if (mystRenderer != null)
            mystRenderer.gameObject.SetActive(true);

        if (moonbowRenderer != null)
            moonbowRenderer.gameObject.SetActive(true);

        if (sparkles != null)
            sparkles.SetActive(true);
    }

    private void ForceStateVisuals(MoonbowState state)
    {
        switch (state)
        {
            case MoonbowState.Myst:
                if (mystRenderer != null)
                {
                    mystRenderer.gameObject.SetActive(true);
                    SetAlpha(mystRenderer, visibleAlpha);
                }

                if (moonbowRenderer != null)
                {
                    moonbowRenderer.gameObject.SetActive(true);
                    SetAlpha(moonbowRenderer, hiddenAlpha);

                    if (!neverDeactivateMoonbowObject)
                        moonbowRenderer.gameObject.SetActive(false);
                }

                if (sparkles != null)
                    sparkles.SetActive(true);

                if (moonbowSparkles != null)
                    moonbowSparkles.SetActive(false);

                SetMoonbowSolidImmediate(false);
                break;

            case MoonbowState.Moonbow:
            case MoonbowState.HoldingBeforeReturn:
                if (mystRenderer != null)
                {
                    mystRenderer.gameObject.SetActive(true);

                    float mistTargetAlpha = neverFullyHideMist
                        ? Mathf.Max(hiddenAlpha, mistMinimumAlpha)
                        : hiddenAlpha;

                    SetAlpha(mystRenderer, mistTargetAlpha);
                }

                if (moonbowRenderer != null)
                {
                    moonbowRenderer.gameObject.SetActive(true);
                    SetAlpha(moonbowRenderer, visibleAlpha);
                }

                if (sparkles != null)
                    sparkles.SetActive(keepSparklesVisibleAlways);

                if (moonbowSparkles != null)
                    moonbowSparkles.SetActive(keepMoonbowSparklesVisibleAlways);

                SetMoonbowSolidImmediate(true);
                break;
        }
    }

    private void UpdateSparklesDuringUse()
    {
        if (sparkles != null)
            sparkles.SetActive(keepSparklesVisibleAlways || _state == MoonbowState.FadingToMoonbow || _state == MoonbowState.FadingToMyst);

        if (moonbowSparkles != null)
        {
            bool moonbowVisible = moonbowRenderer != null && moonbowRenderer.color.a > 0.01f;
            moonbowSparkles.SetActive((keepMoonbowSparklesVisibleAlways || _state == MoonbowState.FadingToMoonbow) && moonbowVisible);
        }
    }

    private void ApplyColliderStateForCurrentState()
    {
        bool shouldBeSolid =
            _state == MoonbowState.Moonbow ||
            _state == MoonbowState.HoldingBeforeReturn;

        SetMoonbowSolidImmediate(shouldBeSolid);
    }

    private void SetMoonbowSolidImmediate(bool solid)
    {
        if (moonbowColliders == null)
            return;

        for (int i = 0; i < moonbowColliders.Length; i++)
        {
            if (moonbowColliders[i] == null)
                continue;

            if (moonbowColliders[i].enabled != solid)
            {
                moonbowColliders[i].enabled = solid;

                if (debugLogs)
                    Debug.Log($"{name}: Collider '{moonbowColliders[i].name}' -> {solid}");
            }
        }
    }

    private void EnforceCurrentState()
    {
        switch (_state)
        {
            case MoonbowState.Myst:
                ForceStateVisuals(MoonbowState.Myst);
                break;

            case MoonbowState.Moonbow:
            case MoonbowState.HoldingBeforeReturn:
                ForceStateVisuals(MoonbowState.Moonbow);
                break;

            case MoonbowState.FadingToMoonbow:
                if (mystRenderer != null && mystRenderer.color.a < hiddenAlpha && neverFullyHideMist)
                {
                    Color c = mystRenderer.color;
                    c.a = mistMinimumAlpha;
                    mystRenderer.color = c;
                }
                break;

            case MoonbowState.FadingToMyst:
                break;
        }

        ApplyColliderStateForCurrentState();
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
        if (_stateRoutine != null)
        {
            StopCoroutine(_stateRoutine);
            _stateRoutine = null;
        }

        _state = MoonbowState.Myst;
        _timeInRange = 0f;
        _holdTimer = 0f;
        _failsafeTimer = 0f;
        _transitionDelayTimer = 0f;

        SetMoonbowSolidImmediate(false);
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