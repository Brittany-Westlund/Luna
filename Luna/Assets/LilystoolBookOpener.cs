using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class OpenBookTrigger : MonoBehaviour
{
    [Header("Availability")]
    [SerializeField] private bool startInteractionEnabled = false;
    [SerializeField] private bool interactionEnabled = false;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer bookVisualRenderer;
    [SerializeField] private bool keepVisualObjectActive = true;

    [Header("Player Detection")]
    [SerializeField] private string requiredColliderName = "PlayerFeet";
    [SerializeField] private string playerTag = "Player";

    [Header("Stability")]
    [SerializeField] private float exitBufferTime = 3f;

    [Header("Dialogue Blocking")]
    [SerializeField] private bool blockWhileDialogueActive = true;
    [SerializeField] private bool forceCloseIfDialogueStarts = true;

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 0.18f;
    [SerializeField] private float closedAlpha = 0f;
    [SerializeField] private float openAlpha = 1f;

    [Header("Startup")]
    [SerializeField] private bool startClosed = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private readonly HashSet<int> insideColliderIds = new HashSet<int>();

    private Collider2D triggerCollider;
    private Coroutine closeRoutine;
    private Coroutine visualFadeRoutine;
    private int closeVersion = 0;
    private bool bookOpen = false;

    public bool IsOpen()
    {
        return bookOpen;
    }

    public bool IsInteractionEnabled()
    {
        return interactionEnabled;
    }

    private void Awake()
    {
        triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider != null)
            triggerCollider.isTrigger = true;

        if (bookVisualRenderer == null)
            bookVisualRenderer = GetComponentInChildren<SpriteRenderer>(true);

        interactionEnabled = startInteractionEnabled;

        EnsureVisualReady();

        if (!interactionEnabled)
        {
            ForceImmediateHiddenState();
        }
        else
        {
            if (startClosed)
                ApplyBookState(false, true);
            else
                ApplyBookState(true, true);
        }
    }

    private void Update()
    {
        if (!interactionEnabled)
        {
            ForceImmediateHiddenState();
            return;
        }

        EnsureVisualReady();

        if (blockWhileDialogueActive && DialogueManager.isConversationActive)
        {
            if (forceCloseIfDialogueStarts && bookOpen)
                ForceClose();
        }
    }

    private void OnDisable()
    {
        insideColliderIds.Clear();

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

        bookOpen = false;
        ApplyVisualImmediate(closedAlpha);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!interactionEnabled)
            return;

        if (!IsValidPlayerCollider(other))
            return;

        int id = other.GetInstanceID();
        insideColliderIds.Add(id);

        CancelPendingClose();
        closeVersion++;
        ApplyBookState(true);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!interactionEnabled)
            return;

        if (!IsValidPlayerCollider(other))
            return;

        int id = other.GetInstanceID();
        insideColliderIds.Add(id);

        CancelPendingClose();

        if (!bookOpen)
            ApplyBookState(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!interactionEnabled)
            return;

        if (!IsValidPlayerCollider(other))
            return;

        int id = other.GetInstanceID();
        insideColliderIds.Remove(id);

        if (insideColliderIds.Count > 0)
            return;

        StartBufferedClose();
    }

    private bool IsValidPlayerCollider(Collider2D other)
    {
        if (other == null)
            return false;

        return other.name == requiredColliderName;
    }

    private void StartBufferedClose()
    {
        CancelPendingClose();
        closeVersion++;

        if (!isActiveAndEnabled || !gameObject.activeInHierarchy)
        {
            ApplyBookState(false, true);
            return;
        }

        closeRoutine = StartCoroutine(CloseAfterBuffer(closeVersion));
    }

    private IEnumerator CloseAfterBuffer(int version)
    {
        yield return new WaitForSeconds(exitBufferTime);

        if (insideColliderIds.Count == 0 && version == closeVersion)
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
            Debug.Log($"OpenBookTrigger ApplyBookState({shouldBeOpen})");
    }

    private void EnsureVisualReady()
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

        EnsureVisualReady();

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

        visualFadeRoutine = StartCoroutine(FadeBookVisualRoutine(targetAlpha));
    }

    private IEnumerator FadeBookVisualRoutine(float targetAlpha)
    {
        if (bookVisualRenderer == null)
            yield break;

        EnsureVisualReady();

        Color c = bookVisualRenderer.color;
        float startAlpha = c.a;

        if (fadeDuration <= 0f)
        {
            c.a = targetAlpha;
            bookVisualRenderer.color = c;
            visualFadeRoutine = null;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            if (bookVisualRenderer == null)
            {
                visualFadeRoutine = null;
                yield break;
            }

            EnsureVisualReady();

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            c = bookVisualRenderer.color;
            c.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            bookVisualRenderer.color = c;

            yield return null;
        }

        if (bookVisualRenderer != null)
        {
            EnsureVisualReady();
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

        EnsureVisualReady();

        Color c = bookVisualRenderer.color;
        c.a = alpha;
        bookVisualRenderer.color = c;
    }

    private void ForceImmediateHiddenState()
    {
        bookOpen = false;
        insideColliderIds.Clear();
        CancelPendingClose();

        if (visualFadeRoutine != null)
        {
            StopCoroutine(visualFadeRoutine);
            visualFadeRoutine = null;
        }

        ApplyVisualImmediate(closedAlpha);
    }

    public void EnableBookInteraction()
    {
        interactionEnabled = true;

        if (debugLogs)
            Debug.Log("OpenBookTrigger: interaction enabled.");

        if (startClosed)
            ForceClose();
        else
            ForceOpen();
    }

    public void DisableBookInteraction()
    {
        interactionEnabled = false;

        if (debugLogs)
            Debug.Log("OpenBookTrigger: interaction disabled.");

        ForceImmediateHiddenState();
    }

    public void ForceOpen()
    {
        if (!interactionEnabled)
            return;

        CancelPendingClose();
        closeVersion++;
        ApplyBookState(true, true);
    }

    public void ForceClose()
    {
        insideColliderIds.Clear();
        CancelPendingClose();
        closeVersion++;
        ApplyBookState(false, true);
    }
}