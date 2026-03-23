using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class CustomInteractionFeedback : MonoBehaviour
{
    public enum FeedbackSourceMode
    {
        AutoUseDirectChildren,
        ManualList
    }

    public enum VisibilityControlMode
    {
        GameObjectActive,
        SpriteRendererAlpha
    }

    public enum TriggerActionType
    {
        None,
        SetGameObjectActiveTrue,
        SetGameObjectActiveFalse,
        SetSpriteRendererEnabledTrue,
        SetSpriteRendererEnabledFalse
    }

    [System.Serializable]
    public class TurnOffTriggerAction
    {
        public TriggerActionType actionType = TriggerActionType.None;
        public GameObject targetObject;
        public SpriteRenderer targetSpriteRenderer;
    }

    [System.Serializable]
    public class FeedbackSFXSettings
    {
        public bool playSFX = false;
        public AudioClip clip;

        [Range(0f, 1f)]
        public float volume = 1f;

        public bool spatialize = true;
    }

    [Header("Feedback Source")]
    [Tooltip("AutoUseDirectChildren = cycle through this object's direct children. ManualList = use the list below.")]
    public FeedbackSourceMode feedbackSourceMode = FeedbackSourceMode.AutoUseDirectChildren;

    [Tooltip("Used only if Feedback Source Mode is ManualList.")]
    public List<GameObject> manualFeedbackObjects = new List<GameObject>();

    [Header("Visibility")]
    public VisibilityControlMode visibilityControlMode = VisibilityControlMode.GameObjectActive;

    [Tooltip("If true, inserts a small delay between one feedback object turning off and the next turning on.")]
    public bool useGracePeriodBetweenObjects = false;

    public float gracePeriodDuration = 0.05f;

    [Header("Behavior")]
    public bool autoPlayOnEnable = true;
    public bool cycleFeedbackObjects = true;
    public bool fadeInAndOut = true;

    [Header("Timing")]
    public float fadeDuration = 0.5f;
    public float holdDuration = 0.1f;

    [Header("Alpha")]
    [Range(0f, 1f)]
    [SerializeField] private float externalAlphaMultiplier = 1f;

    [Header("Dismiss / Turn Off")]
    public bool allowPressEToTurnOff = false;
    public KeyCode dismissKey = KeyCode.E;

    [Tooltip("If true, the player must be inside this feedback trigger to dismiss it.")]
    public bool mustPlayerBeInTrigger = true;

    public string playerTag = "Player";

    [Header("Before Turn Off Actions")]
    [Tooltip("If true, run the actions below before this feedback object turns itself off.")]
    public bool triggerBeforeTurnOff = false;
    public List<TurnOffTriggerAction> beforeTurnOffActions = new List<TurnOffTriggerAction>();

    [Header("Optional SFX")]
    [Tooltip("Played when this feedback object enables.")]
    public FeedbackSFXSettings activateSFX = new FeedbackSFXSettings();

    [Tooltip("Played when this feedback object is dismissed.")]
    public FeedbackSFXSettings dismissSFX = new FeedbackSFXSettings();

    [Tooltip("Played when a feedback object becomes active in the cycle.")]
    public FeedbackSFXSettings cycleStepSFX = new FeedbackSFXSettings();

    [Header("Unity Events")]
    [Tooltip("Called when this feedback object enables.")]
    public UnityEvent OnFeedbackActivated;

    [Tooltip("Called when the player enters this feedback trigger.")]
    public UnityEvent OnPlayerEntered;

    [Tooltip("Called when the player exits this feedback trigger.")]
    public UnityEvent OnPlayerExited;

    [Tooltip("Called right before the feedback object turns itself off.")]
    public UnityEvent OnFeedbackDismissed;

    [Header("Debug")]
    public bool debugLogging = false;

    private readonly List<GameObject> runtimeFeedbackObjects = new List<GameObject>();

    private Coroutine cycleRoutine;
    private Coroutine singleRoutine;
    private int currentIndex = 0;
    private bool playerInRange = false;

    private void OnEnable()
    {
        RebuildFeedbackList();
        HideAllImmediately();

        if (debugLogging)
        {
            Debug.Log($"[CustomInteractionFeedback] Enabled on '{name}'. Feedback count: {runtimeFeedbackObjects.Count}");
        }

        PlayConfiguredSFX(activateSFX, transform.position);
        OnFeedbackActivated?.Invoke();

        if (autoPlayOnEnable)
        {
            StartCycling();
        }
    }

    private void OnDisable()
    {
        StopCycling();
        HideAllImmediately();
        playerInRange = false;
    }

    private void OnValidate()
    {
        fadeDuration = Mathf.Max(0f, fadeDuration);
        holdDuration = Mathf.Max(0f, holdDuration);
        gracePeriodDuration = Mathf.Max(0f, gracePeriodDuration);
        externalAlphaMultiplier = Mathf.Clamp01(externalAlphaMultiplier);
    }

    private void Update()
    {
        if (!allowPressEToTurnOff)
            return;

        if (mustPlayerBeInTrigger && !playerInRange)
            return;

        if (Input.GetKeyDown(dismissKey))
        {
            DismissFeedback();
        }
    }

    public void RefreshDisplay()
    {
        RebuildFeedbackList();
        StartCycling();
    }

    public void SetExternalAlphaMultiplier(float alpha)
    {
        externalAlphaMultiplier = Mathf.Clamp01(alpha);

        if (debugLogging)
        {
            Debug.Log($"[CustomInteractionFeedback] External alpha multiplier set to {externalAlphaMultiplier} on '{name}'.");
        }
    }

    public void RebuildFeedbackList()
    {
        runtimeFeedbackObjects.Clear();

        if (feedbackSourceMode == FeedbackSourceMode.AutoUseDirectChildren)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);

                if (child == null)
                    continue;

                GameObject childObject = child.gameObject;

                if (childObject == null)
                    continue;

                runtimeFeedbackObjects.Add(childObject);
            }
        }
        else
        {
            foreach (GameObject obj in manualFeedbackObjects)
            {
                if (obj != null)
                {
                    runtimeFeedbackObjects.Add(obj);
                }
            }
        }
    }

    public void StartCycling()
    {
        StopCycling();
        HideAllImmediately();
        currentIndex = 0;

        if (runtimeFeedbackObjects.Count == 0)
        {
            if (debugLogging)
            {
                Debug.LogWarning($"[CustomInteractionFeedback] No feedback objects available on '{name}'.");
            }
            return;
        }

        if (cycleFeedbackObjects && runtimeFeedbackObjects.Count > 1)
        {
            cycleRoutine = StartCoroutine(CycleRoutine());
        }
        else
        {
            singleRoutine = StartCoroutine(SingleRoutine());
        }
    }

    public void StopCycling()
    {
        if (cycleRoutine != null)
        {
            StopCoroutine(cycleRoutine);
            cycleRoutine = null;
        }

        if (singleRoutine != null)
        {
            StopCoroutine(singleRoutine);
            singleRoutine = null;
        }
    }

    public void DismissFeedback()
    {
        if (debugLogging)
        {
            Debug.Log($"[CustomInteractionFeedback] DismissFeedback called on '{name}'.");
        }

        if (triggerBeforeTurnOff)
        {
            ApplyBeforeTurnOffActions();
        }

        PlayConfiguredSFX(dismissSFX, transform.position);
        OnFeedbackDismissed?.Invoke();

        gameObject.SetActive(false);
    }

    public void TurnSelfOff()
    {
        if (debugLogging)
        {
            Debug.Log($"[CustomInteractionFeedback] TurnSelfOff called on '{name}'.");
        }

        gameObject.SetActive(false);
    }

    public void TurnSelfOn()
    {
        if (debugLogging)
        {
            Debug.Log($"[CustomInteractionFeedback] TurnSelfOn called on '{name}'.");
        }

        gameObject.SetActive(true);
    }

    public void RestartCycle()
    {
        if (debugLogging)
        {
            Debug.Log($"[CustomInteractionFeedback] RestartCycle called on '{name}'.");
        }

        RefreshDisplay();
    }

    private void ApplyBeforeTurnOffActions()
    {
        if (beforeTurnOffActions == null || beforeTurnOffActions.Count == 0)
            return;

        foreach (TurnOffTriggerAction action in beforeTurnOffActions)
        {
            if (action == null)
                continue;

            switch (action.actionType)
            {
                case TriggerActionType.None:
                    break;

                case TriggerActionType.SetGameObjectActiveTrue:
                    if (action.targetObject != null)
                    {
                        action.targetObject.SetActive(true);

                        if (debugLogging)
                        {
                            Debug.Log($"[CustomInteractionFeedback] Before turn off: SetActive(true) on '{action.targetObject.name}'.");
                        }
                    }
                    break;

                case TriggerActionType.SetGameObjectActiveFalse:
                    if (action.targetObject != null)
                    {
                        action.targetObject.SetActive(false);

                        if (debugLogging)
                        {
                            Debug.Log($"[CustomInteractionFeedback] Before turn off: SetActive(false) on '{action.targetObject.name}'.");
                        }
                    }
                    break;

                case TriggerActionType.SetSpriteRendererEnabledTrue:
                    if (action.targetSpriteRenderer != null)
                    {
                        action.targetSpriteRenderer.enabled = true;

                        if (debugLogging)
                        {
                            Debug.Log($"[CustomInteractionFeedback] Before turn off: SpriteRenderer enabled on '{action.targetSpriteRenderer.name}'.");
                        }
                    }
                    break;

                case TriggerActionType.SetSpriteRendererEnabledFalse:
                    if (action.targetSpriteRenderer != null)
                    {
                        action.targetSpriteRenderer.enabled = false;

                        if (debugLogging)
                        {
                            Debug.Log($"[CustomInteractionFeedback] Before turn off: SpriteRenderer disabled on '{action.targetSpriteRenderer.name}'.");
                        }
                    }
                    break;
            }
        }
    }

    private IEnumerator SingleRoutine()
    {
        if (runtimeFeedbackObjects.Count == 0)
            yield break;

        GameObject target = runtimeFeedbackObjects[0];
        if (target == null)
            yield break;

        ShowFeedbackObject(target);
        PlayConfiguredSFX(cycleStepSFX, target.transform.position);

        SpriteRenderer sr = GetFeedbackSpriteRenderer(target);
        if (sr == null)
            yield break;

        if (fadeInAndOut)
        {
            while (true)
            {
                yield return FadeAlpha(sr, 0f, externalAlphaMultiplier, fadeDuration);
                yield return new WaitForSeconds(holdDuration);
                yield return FadeAlpha(sr, externalAlphaMultiplier, 0f, fadeDuration);
                yield return new WaitForSeconds(holdDuration);
            }
        }
        else
        {
            SetAlpha(sr, externalAlphaMultiplier);
        }
    }

    private IEnumerator CycleRoutine()
    {
        while (true)
        {
            if (runtimeFeedbackObjects.Count == 0)
                yield break;

            GameObject current = runtimeFeedbackObjects[currentIndex];

            if (current != null)
            {
                ShowFeedbackObject(current);
                PlayConfiguredSFX(cycleStepSFX, current.transform.position);

                SpriteRenderer sr = GetFeedbackSpriteRenderer(current);
                if (sr != null)
                {
                    if (fadeInAndOut)
                    {
                        yield return FadeAlpha(sr, 0f, externalAlphaMultiplier, fadeDuration);
                        yield return new WaitForSeconds(holdDuration);
                        yield return FadeAlpha(sr, externalAlphaMultiplier, 0f, fadeDuration);
                    }
                    else
                    {
                        SetAlpha(sr, externalAlphaMultiplier);
                        yield return new WaitForSeconds(fadeDuration);
                        SetAlpha(sr, 0f);
                    }
                }

                HideFeedbackObject(current);

                if (useGracePeriodBetweenObjects)
                {
                    yield return new WaitForSeconds(gracePeriodDuration);
                }
            }

            currentIndex++;
            if (currentIndex >= runtimeFeedbackObjects.Count)
                currentIndex = 0;
        }
    }

    private IEnumerator FadeAlpha(SpriteRenderer sr, float startAlpha, float endAlpha, float duration)
    {
        if (sr == null)
            yield break;

        if (duration <= 0f)
        {
            SetAlpha(sr, endAlpha);
            yield break;
        }

        float time = 0f;
        SetAlpha(sr, startAlpha);

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            SetAlpha(sr, alpha);
            yield return null;
        }

        SetAlpha(sr, endAlpha);
    }

    private void HideAllImmediately()
    {
        foreach (GameObject obj in runtimeFeedbackObjects)
        {
            if (obj == null)
                continue;

            SpriteRenderer sr = GetFeedbackSpriteRenderer(obj);
            if (sr != null)
            {
                SetAlpha(sr, 0f);
            }

            if (visibilityControlMode == VisibilityControlMode.GameObjectActive)
            {
                obj.SetActive(false);
            }
            else
            {
                obj.SetActive(true);
            }
        }
    }

    private void ShowFeedbackObject(GameObject obj)
    {
        if (obj == null)
            return;

        if (visibilityControlMode == VisibilityControlMode.GameObjectActive)
        {
            obj.SetActive(true);
        }
        else
        {
            obj.SetActive(true);

            SpriteRenderer sr = GetFeedbackSpriteRenderer(obj);
            if (sr != null)
            {
                sr.enabled = true;
            }
        }
    }

    private void HideFeedbackObject(GameObject obj)
    {
        if (obj == null)
            return;

        if (visibilityControlMode == VisibilityControlMode.GameObjectActive)
        {
            obj.SetActive(false);
        }
        else
        {
            SpriteRenderer sr = GetFeedbackSpriteRenderer(obj);
            if (sr != null)
            {
                SetAlpha(sr, 0f);
                sr.enabled = true;
            }
        }
    }

    private SpriteRenderer GetFeedbackSpriteRenderer(GameObject obj)
    {
        if (obj == null)
            return null;

        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null)
            return sr;

        return obj.GetComponentInChildren<SpriteRenderer>(true);
    }

    private void SetAlpha(SpriteRenderer sr, float alpha)
    {
        if (sr == null)
            return;

        Color c = sr.color;
        c.a = Mathf.Clamp01(alpha);
        sr.color = c;
    }

    private void PlayConfiguredSFX(FeedbackSFXSettings settings, Vector3 defaultPosition)
    {
        if (settings == null || !settings.playSFX || settings.clip == null)
            return;

        Vector3 playPosition = settings.spatialize
            ? defaultPosition
            : Camera.main != null ? Camera.main.transform.position : defaultPosition;

        AudioSource.PlayClipAtPoint(settings.clip, playPosition, settings.volume);

        if (debugLogging)
        {
            Debug.Log($"[CustomInteractionFeedback] Played SFX '{settings.clip.name}' on '{name}'.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!mustPlayerBeInTrigger)
            return;

        if (!other.CompareTag(playerTag))
            return;

        playerInRange = true;

        if (debugLogging)
        {
            Debug.Log($"[CustomInteractionFeedback] Player entered trigger for '{name}'.");
        }

        OnPlayerEntered?.Invoke();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!mustPlayerBeInTrigger)
            return;

        if (!other.CompareTag(playerTag))
            return;

        playerInRange = false;

        if (debugLogging)
        {
            Debug.Log($"[CustomInteractionFeedback] Player exited trigger for '{name}'.");
        }

        OnPlayerExited?.Invoke();
    }
}