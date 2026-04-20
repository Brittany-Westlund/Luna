using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class OpenBookTrigger : MonoBehaviour
{
    [Header("Book Location")]
    [SerializeField] private string locationId = "";

    [Header("Trigger Collider")]
    [SerializeField] private Collider2D triggerCollider;

    [Header("Availability")]
    [SerializeField] private bool startInteractionEnabled = false;
    [SerializeField] private bool interactionEnabled = false;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer bookVisualRenderer;
    [SerializeField] private bool keepVisualObjectActive = true;

    [Header("Player Detection")]
    [SerializeField] private string requiredTag = "PlayerFeet";

    [Header("Radius Detection")]
    [SerializeField] private float detectionRadius = 0.45f;
    [SerializeField] private float detectionInterval = 0.02f;

    [Header("Stability")]
    [SerializeField] private float exitBufferTime = 0.5f;

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 0.18f;
    [SerializeField] private float closedAlpha = 0f;
    [SerializeField] private float openAlpha = 1f;

    [Header("Startup")]
    [SerializeField] private bool startClosed = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private bool playerInside = false;
    private bool bookOpen = false;

    private Coroutine closeRoutine;
    private Coroutine visualFadeRoutine;
    private Coroutine detectionRoutine;

    private BookControllerSimple bookController;

    public bool IsOpen()
    {
        return bookOpen;
    }

    public bool IsInteractionEnabled()
    {
        return interactionEnabled;
    }

    private void Reset()
    {
        if (triggerCollider == null)
            triggerCollider = GetComponent<Collider2D>();

        if (bookVisualRenderer == null)
            bookVisualRenderer = GetComponentInChildren<SpriteRenderer>(true);

        if (triggerCollider != null)
            triggerCollider.isTrigger = true;
    }

    private void Awake()
    {
        if (triggerCollider == null)
            triggerCollider = GetComponent<Collider2D>();

        if (bookVisualRenderer == null)
            bookVisualRenderer = GetComponentInChildren<SpriteRenderer>(true);

        if (triggerCollider != null)
            triggerCollider.isTrigger = true;
        else
            Debug.LogWarning($"[OpenBookTrigger] No triggerCollider assigned/found on '{name}'.");

        interactionEnabled = startInteractionEnabled;

        ResolveBookController();
        PrepareVisualReference();

        if (!interactionEnabled)
        {
            ForceImmediateHiddenState();
        }
        else
        {
            ApplyBookState(!startClosed, true);
        }
    }

    private void Start()
    {
        ResolveBookController();
    }

    private void OnEnable()
    {
        ResolveBookController();

        if (detectionRoutine != null)
        {
            StopCoroutine(detectionRoutine);
            detectionRoutine = null;
        }

        detectionRoutine = StartCoroutine(RadiusDetectionRoutine());
    }

    private void OnDisable()
    {
        playerInside = false;

        if (closeRoutine != null)
        {
            StopCoroutine(closeRoutine);
            closeRoutine = null;
        }

        if (visualFadeRoutine != null)
        {
            StopCoroutine(visualFadeRoutine);
            visualFadeRoutine = null;
        }

        if (detectionRoutine != null)
        {
            StopCoroutine(detectionRoutine);
            detectionRoutine = null;
        }

        bookOpen = false;
        ApplyVisualImmediate(closedAlpha);
    }

    private IEnumerator RadiusDetectionRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(detectionInterval);

        while (true)
        {
            if (interactionEnabled)
            {
                bool detectedNow = IsPlayerFeetWithinRadius();

                if (detectedNow && !playerInside)
                {
                    playerInside = true;

                    CancelPendingClose();
                    PushCurrentLocationToBookController();
                    ApplyBookState(true);

                    if (debugLogs)
                        Debug.Log("[OpenBookTrigger] RADIUS ENTER");
                }
                else if (detectedNow && playerInside)
                {
                    CancelPendingClose();

                    if (!bookOpen)
                    {
                        PushCurrentLocationToBookController();
                        ApplyBookState(true);

                        if (debugLogs)
                            Debug.Log("[OpenBookTrigger] RADIUS STAY reopened");
                    }
                }
                else if (!detectedNow && playerInside)
                {
                    playerInside = false;

                    if (debugLogs)
                        Debug.Log("[OpenBookTrigger] RADIUS EXIT");

                    StartBufferedClose();
                }
            }

            yield return wait;
        }
    }

    private bool IsPlayerFeetWithinRadius()
    {
        if (triggerCollider == null)
            return false;

        Vector2 center = GetDetectionCenter();
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, detectionRadius);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit == null)
                continue;

            if (IsValidPlayerCollider(hit))
                return true;
        }

        return false;
    }

    private Vector2 GetDetectionCenter()
    {
        if (triggerCollider != null)
            return triggerCollider.bounds.center;

        return transform.position;
    }

    private bool IsValidPlayerCollider(Collider2D other)
    {
        if (other == null)
            return false;

        return other.CompareTag(requiredTag);
    }

    private void ResolveBookController()
    {
        if (bookController != null)
            return;

        bookController = FindObjectOfType<BookControllerSimple>();

        if (debugLogs)
        {
            if (bookController != null)
                Debug.Log($"[OpenBookTrigger] Found BookControllerSimple automatically: {bookController.name}");
            else
                Debug.LogWarning("[OpenBookTrigger] Could not find BookControllerSimple in scene.");
        }
    }

    private void PushCurrentLocationToBookController()
    {
        ResolveBookController();

        if (bookController != null)
            bookController.SetCurrentOpenLocation(locationId);
    }

    private void StartBufferedClose()
    {
        CancelPendingClose();

        if (!isActiveAndEnabled || !gameObject.activeInHierarchy)
        {
            ApplyBookState(false, true);
            return;
        }

        closeRoutine = StartCoroutine(CloseAfterBuffer());
    }

    private IEnumerator CloseAfterBuffer()
    {
        yield return new WaitForSeconds(exitBufferTime);

        if (!playerInside)
            ApplyBookState(false);

        closeRoutine = null;
    }

    private void CancelPendingClose()
    {
        if (closeRoutine != null)
        {
            StopCoroutine(closeRoutine);
            closeRoutine = null;
        }
    }

    private void ApplyBookState(bool shouldBeOpen, bool force = false)
    {
        if (!force && bookOpen == shouldBeOpen)
            return;

        if (!interactionEnabled)
        {
            ForceImmediateHiddenState();
            return;
        }

        bookOpen = shouldBeOpen;
        FadeBookVisualTo(shouldBeOpen ? openAlpha : closedAlpha);

        if (debugLogs)
            Debug.Log($"[OpenBookTrigger] ApplyBookState({shouldBeOpen})");
    }

    private void PrepareVisualReference()
    {
        if (bookVisualRenderer == null)
            return;

        if (keepVisualObjectActive && !bookVisualRenderer.gameObject.activeSelf)
            bookVisualRenderer.gameObject.SetActive(true);

        if (!bookVisualRenderer.enabled)
            bookVisualRenderer.enabled = true;
    }

    private void FadeBookVisualTo(float targetAlpha)
    {
        if (bookVisualRenderer == null)
            return;

        PrepareVisualReference();

        if (!gameObject.activeInHierarchy || !isActiveAndEnabled)
        {
            ApplyVisualImmediate(targetAlpha);
            return;
        }

        if (visualFadeRoutine != null)
        {
            StopCoroutine(visualFadeRoutine);
            visualFadeRoutine = null;
        }

        visualFadeRoutine = StartCoroutine(FadeRoutine(targetAlpha));
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        if (bookVisualRenderer == null)
            yield break;

        Color c = bookVisualRenderer.color;
        float startAlpha = c.a;
        float elapsed = 0f;

        if (fadeDuration <= 0f)
        {
            c.a = targetAlpha;
            bookVisualRenderer.color = c;
            visualFadeRoutine = null;
            yield break;
        }

        while (elapsed < fadeDuration)
        {
            if (bookVisualRenderer == null)
            {
                visualFadeRoutine = null;
                yield break;
            }

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            c = bookVisualRenderer.color;
            c.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            bookVisualRenderer.color = c;

            yield return null;
        }

        if (bookVisualRenderer != null)
        {
            c = bookVisualRenderer.color;
            c.a = targetAlpha;
            bookVisualRenderer.color = c;
        }

        visualFadeRoutine = null;
    }

    private void ApplyVisualImmediate(float alpha)
    {
        if (bookVisualRenderer == null)
            return;

        PrepareVisualReference();

        Color c = bookVisualRenderer.color;
        c.a = alpha;
        bookVisualRenderer.color = c;
    }

    private void ForceImmediateHiddenState()
    {
        bookOpen = false;
        playerInside = false;
        CancelPendingClose();

        if (visualFadeRoutine != null)
        {
            StopCoroutine(visualFadeRoutine);
            visualFadeRoutine = null;
        }

        ApplyVisualImmediate(closedAlpha);

        if (debugLogs)
            Debug.Log("[OpenBookTrigger] ForceImmediateHiddenState()");
    }

    public void EnableBookInteraction()
    {
        interactionEnabled = true;

        if (debugLogs)
            Debug.Log("[OpenBookTrigger] interaction enabled.");
    }

    public void DisableBookInteraction()
    {
        interactionEnabled = false;
        ForceImmediateHiddenState();

        if (debugLogs)
            Debug.Log("[OpenBookTrigger] interaction disabled.");
    }

    public void ForceOpen()
    {
        if (!interactionEnabled)
            return;

        CancelPendingClose();
        PushCurrentLocationToBookController();
        ApplyBookState(true, true);

        if (debugLogs)
            Debug.Log("[OpenBookTrigger] ForceOpen()");
    }

    public void ForceClose()
    {
        playerInside = false;
        CancelPendingClose();
        ApplyBookState(false, true);

        if (debugLogs)
            Debug.Log("[OpenBookTrigger] ForceClose()");
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 center;

        if (triggerCollider != null)
            center = triggerCollider.bounds.center;
        else
            center = transform.position;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, detectionRadius);
    }
}