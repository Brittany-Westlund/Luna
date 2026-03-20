using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class PromptCycleFader : MonoBehaviour
{
    public enum PromptSourceMode
    {
        AutoUseDirectChildren,
        ManualList
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

    [Header("Prompt Source")]
    [Tooltip("AutoUseDirectChildren = cycle through this object's direct children. ManualList = use the list below.")]
    public PromptSourceMode promptSourceMode = PromptSourceMode.AutoUseDirectChildren;

    [Tooltip("Used only if Prompt Source Mode is ManualList.")]
    public List<GameObject> manualPromptObjects = new List<GameObject>();

    [Header("Behavior")]
    public bool autoPlayOnEnable = true;
    public bool cyclePrompts = true;
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

    [Tooltip("If true, the player must be inside this prompt's trigger to dismiss it.")]
    public bool mustPlayerBeInTrigger = true;

    public string playerTag = "Player";

    [Header("Before Turn Off Actions")]
    [Tooltip("If true, run the actions below before this prompt turns itself off.")]
    public bool triggerBeforeTurnOff = false;
    public List<TurnOffTriggerAction> beforeTurnOffActions = new List<TurnOffTriggerAction>();

    [Header("Unity Events")]
    [Tooltip("Called when this prompt object enables.")]
    public UnityEvent OnPromptActivated;

    [Tooltip("Called when the player enters this prompt trigger.")]
    public UnityEvent OnPlayerEntered;

    [Tooltip("Called when the player exits this prompt trigger.")]
    public UnityEvent OnPlayerExited;

    [Tooltip("Called right before the prompt object turns itself off.")]
    public UnityEvent OnPromptDismissed;

    [Header("Debug")]
    public bool debugLogging = false;

    private readonly List<GameObject> runtimePromptObjects = new List<GameObject>();

    private Coroutine cycleRoutine;
    private Coroutine singleRoutine;
    private int currentIndex = 0;
    private bool playerInRange = false;

    private void OnEnable()
    {
        RebuildPromptList();
        HideAllImmediately();

        if (debugLogging)
        {
            Debug.Log($"[PromptCycleFader] Enabled on '{name}'. Prompt count: {runtimePromptObjects.Count}");
        }

        OnPromptActivated?.Invoke();

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
            DismissPrompt();
        }
    }

    public void RefreshDisplay()
    {
        RebuildPromptList();
        StartCycling();
    }

    public void SetExternalAlphaMultiplier(float alpha)
    {
        externalAlphaMultiplier = Mathf.Clamp01(alpha);

        if (debugLogging)
        {
            Debug.Log($"[PromptCycleFader] External alpha multiplier set to {externalAlphaMultiplier} on '{name}'.");
        }
    }

    public void RebuildPromptList()
    {
        runtimePromptObjects.Clear();

        if (promptSourceMode == PromptSourceMode.AutoUseDirectChildren)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);

                if (child == null)
                    continue;

                GameObject childObject = child.gameObject;

                if (childObject == null)
                    continue;

                runtimePromptObjects.Add(childObject);
            }
        }
        else
        {
            foreach (GameObject obj in manualPromptObjects)
            {
                if (obj != null)
                {
                    runtimePromptObjects.Add(obj);
                }
            }
        }
    }

    public void StartCycling()
    {
        StopCycling();
        HideAllImmediately();
        currentIndex = 0;

        if (runtimePromptObjects.Count == 0)
        {
            if (debugLogging)
            {
                Debug.LogWarning($"[PromptCycleFader] No prompt objects available on '{name}'.");
            }
            return;
        }

        if (cyclePrompts && runtimePromptObjects.Count > 1)
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

    public void DismissPrompt()
    {
        if (debugLogging)
        {
            Debug.Log($"[PromptCycleFader] DismissPrompt called on '{name}'.");
        }

        if (triggerBeforeTurnOff)
        {
            ApplyBeforeTurnOffActions();
        }

        OnPromptDismissed?.Invoke();

        gameObject.SetActive(false);
    }

    public void TurnSelfOff()
    {
        if (debugLogging)
        {
            Debug.Log($"[PromptCycleFader] TurnSelfOff called on '{name}'.");
        }

        gameObject.SetActive(false);
    }

    public void TurnSelfOn()
    {
        if (debugLogging)
        {
            Debug.Log($"[PromptCycleFader] TurnSelfOn called on '{name}'.");
        }

        gameObject.SetActive(true);
    }

    public void RestartCycle()
    {
        if (debugLogging)
        {
            Debug.Log($"[PromptCycleFader] RestartCycle called on '{name}'.");
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
                            Debug.Log($"[PromptCycleFader] Before turn off: SetActive(true) on '{action.targetObject.name}'.");
                        }
                    }
                    break;

                case TriggerActionType.SetGameObjectActiveFalse:
                    if (action.targetObject != null)
                    {
                        action.targetObject.SetActive(false);

                        if (debugLogging)
                        {
                            Debug.Log($"[PromptCycleFader] Before turn off: SetActive(false) on '{action.targetObject.name}'.");
                        }
                    }
                    break;

                case TriggerActionType.SetSpriteRendererEnabledTrue:
                    if (action.targetSpriteRenderer != null)
                    {
                        action.targetSpriteRenderer.enabled = true;

                        if (debugLogging)
                        {
                            Debug.Log($"[PromptCycleFader] Before turn off: SpriteRenderer enabled on '{action.targetSpriteRenderer.name}'.");
                        }
                    }
                    break;

                case TriggerActionType.SetSpriteRendererEnabledFalse:
                    if (action.targetSpriteRenderer != null)
                    {
                        action.targetSpriteRenderer.enabled = false;

                        if (debugLogging)
                        {
                            Debug.Log($"[PromptCycleFader] Before turn off: SpriteRenderer disabled on '{action.targetSpriteRenderer.name}'.");
                        }
                    }
                    break;
            }
        }
    }

    private IEnumerator SingleRoutine()
    {
        if (runtimePromptObjects.Count == 0)
            yield break;

        GameObject target = runtimePromptObjects[0];
        if (target == null)
            yield break;

        SetPromptObjectActive(target, true);

        SpriteRenderer sr = GetPromptSpriteRenderer(target);
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
            if (runtimePromptObjects.Count == 0)
                yield break;

            GameObject current = runtimePromptObjects[currentIndex];

            if (current != null)
            {
                SetPromptObjectActive(current, true);

                SpriteRenderer sr = GetPromptSpriteRenderer(current);
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

                SetPromptObjectActive(current, false);
            }

            currentIndex++;
            if (currentIndex >= runtimePromptObjects.Count)
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
        foreach (GameObject obj in runtimePromptObjects)
        {
            if (obj == null)
                continue;

            SpriteRenderer sr = GetPromptSpriteRenderer(obj);
            if (sr != null)
            {
                SetAlpha(sr, 0f);
            }

            obj.SetActive(false);
        }
    }

    private void SetPromptObjectActive(GameObject obj, bool state)
    {
        if (obj != null)
        {
            obj.SetActive(state);
        }
    }

    private SpriteRenderer GetPromptSpriteRenderer(GameObject obj)
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!mustPlayerBeInTrigger)
            return;

        if (!other.CompareTag(playerTag))
            return;

        playerInRange = true;

        if (debugLogging)
        {
            Debug.Log($"[PromptCycleFader] Player entered trigger for '{name}'.");
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
            Debug.Log($"[PromptCycleFader] Player exited trigger for '{name}'.");
        }

        OnPlayerExited?.Invoke();
    }
}