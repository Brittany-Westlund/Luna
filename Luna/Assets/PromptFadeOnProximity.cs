using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class PromptFadeOnProximity : MonoBehaviour
{
    [Header("Target Feedback")]
    [SerializeField] private CustomInteractionFeedback targetFeedback;

    [Header("Detection")]
    [SerializeField] private string playerTag = "";
    [SerializeField] private bool requireSpecificColliderName = true;
    [SerializeField] private string requiredColliderName = "PlayerFeet";

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.2f;
    [SerializeField] private float fadeOutDelay = 0.05f;
    [SerializeField] private float hiddenAlpha = 0f;
    [SerializeField] private float visibleAlpha = 1f;

    [Header("Feedback Control")]
    [SerializeField] private bool restartCycleOnEnter = true;
    [SerializeField] private bool stopCycleOnExit = false;

    [Header("Debug")]
    [SerializeField] private bool debugLogging = false;

    private Coroutine fadeRoutine;
    private bool playerInRange = false;
    private float currentAlpha = 0f;
    private Collider2D triggerCollider;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }

        if (targetFeedback == null)
        {
            targetFeedback = GetComponent<CustomInteractionFeedback>();
        }

        if (targetFeedback == null)
        {
            Debug.LogWarning($"[PromptFadeOnProximity] No CustomInteractionFeedback found on '{name}'.");
        }
    }

    private void Start()
    {
        currentAlpha = hiddenAlpha;
        ApplyAlphaImmediate(currentAlpha);

        if (debugLogging)
        {
            Debug.Log($"[PromptFadeOnProximity] Start on '{name}'. Hidden alpha applied: {hiddenAlpha}");
        }
    }

    private void OnEnable()
    {
        currentAlpha = hiddenAlpha;
        ApplyAlphaImmediate(currentAlpha);
    }

    private void OnDisable()
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsValidPlayer(other))
            return;

        playerInRange = true;

        if (debugLogging)
        {
            Debug.Log($"[PromptFadeOnProximity] Enter by '{other.name}' on '{name}'.");
        }

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        if (targetFeedback != null && restartCycleOnEnter)
        {
            targetFeedback.RestartCycle();
        }

        fadeRoutine = StartCoroutine(FadeTo(visibleAlpha, fadeInDuration));
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsValidPlayer(other))
            return;

        playerInRange = false;

        if (debugLogging)
        {
            Debug.Log($"[PromptFadeOnProximity] Exit by '{other.name}' on '{name}'.");
        }

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        fadeRoutine = StartCoroutine(FadeOutAfterDelay());
    }

    private bool IsValidPlayer(Collider2D other)
    {
        if (other == null)
            return false;

        if (!string.IsNullOrEmpty(playerTag))
        {
            if (!other.CompareTag(playerTag))
                return false;
        }

        if (requireSpecificColliderName && !string.IsNullOrEmpty(requiredColliderName))
        {
            if (other.name != requiredColliderName)
                return false;
        }

        return true;
    }

    private IEnumerator FadeOutAfterDelay()
    {
        if (fadeOutDelay > 0f)
        {
            yield return new WaitForSeconds(fadeOutDelay);
        }

        if (playerInRange)
            yield break;

        fadeRoutine = StartCoroutine(FadeTo(hiddenAlpha, fadeOutDuration));

        if (targetFeedback != null && stopCycleOnExit)
        {
            targetFeedback.StopCycling();
        }
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        float startAlpha = currentAlpha;

        if (duration <= 0f)
        {
            currentAlpha = targetAlpha;
            ApplyAlphaImmediate(currentAlpha);
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            ApplyAlphaImmediate(currentAlpha);
            yield return null;
        }

        currentAlpha = targetAlpha;
        ApplyAlphaImmediate(currentAlpha);
    }

    private void ApplyAlphaImmediate(float alpha)
    {
        currentAlpha = Mathf.Clamp01(alpha);

        if (targetFeedback != null)
        {
            targetFeedback.SetExternalAlphaMultiplier(currentAlpha);

            if (debugLogging)
            {
                Debug.Log($"[PromptFadeOnProximity] Applied external alpha {currentAlpha} on '{name}'.");
            }
        }
    }
}