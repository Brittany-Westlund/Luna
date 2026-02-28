using UnityEngine;
using System.Collections;

public class MaryFinalReceiver : MonoBehaviour
{
    [Header("Book (New)")]
    public BookControllerSimple bookSimple; // drag BookControllerSimple here (preferred)

    [Header("Book (Old / Legacy)")]
    public BookPageController bookLegacy;   // drag BookPageController here if you're still using it anywhere

    [Header("Fade")]
    public float fadeSeconds = 1.0f;

    [Header("Optional")]
    public GameObject visualRoot; // leave empty if sprites are on Mary
    public Collider2D interactionCollider;

    [Header("Debug")]
    public bool debugLogs = false;

    private bool convoFinished = false;
    private bool fading = false;

    private SpriteRenderer[] renderers;

    void Awake()
    {
        ResolveBookRefs();

        if (visualRoot == null)
            visualRoot = gameObject;

        renderers = visualRoot.GetComponentsInChildren<SpriteRenderer>(true);
    }

    void Update()
    {
        ResolveBookRefs();

        // If convo has finished and the book is currently closed, fade.
        TryFadeIfReady();
    }

    void ResolveBookRefs()
    {
        // Prefer the new controller if present/assigned
        if (bookSimple == null)
            bookSimple = FindFirstObjectByTypeCompat<BookControllerSimple>();

        // Legacy fallback
        if (bookLegacy == null && BookPageController.Instance != null)
            bookLegacy = BookPageController.Instance;

        if (bookLegacy == null)
            bookLegacy = FindFirstObjectByTypeCompat<BookPageController>();
    }

    bool IsBookOpen()
    {
        if (bookSimple != null) return bookSimple.IsOpen;
        if (bookLegacy != null) return bookLegacy.IsOpen;
        return false; // if no book found, treat as closed so Mary can still fade
    }

    // 🔥 Dialogue System Trigger calls this at end of MaryGiveBook
    public void MarkConversationFinished()
    {
        convoFinished = true;

        if (interactionCollider != null)
            interactionCollider.enabled = false;

        if (debugLogs) Debug.Log("🌿 MaryFinalReceiver: convo finished.");

        // If the player already closed the book mid-convo, this will now fade immediately:
        TryFadeIfReady();
    }

    void TryFadeIfReady()
    {
        if (fading) return;
        if (!convoFinished) return;

        // Must be closed at fade time:
        if (IsBookOpen()) return;

        if (debugLogs) Debug.Log("🌿 MaryFinalReceiver: ready to fade (convoFinished && bookClosed).");

        StartCoroutine(FadeThenDisable());
    }

    IEnumerator FadeThenDisable()
    {
        fading = true;

        if (renderers == null || renderers.Length == 0)
        {
            if (debugLogs) Debug.LogWarning("🌿 MaryFinalReceiver: no SpriteRenderers; disabling instantly.");
            gameObject.SetActive(false);
            yield break;
        }

        // capture original colors
        Color[] start = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            start[i] = renderers[i].color;

        float t = 0f;
        float dur = Mathf.Max(0.01f, fadeSeconds);

        while (t < dur)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / dur);
            float a = Mathf.Lerp(1f, 0f, k);

            for (int i = 0; i < renderers.Length; i++)
            {
                Color c = start[i];
                c.a = a;
                renderers[i].color = c;
            }

            yield return null;
        }

        if (debugLogs) Debug.Log("🌿 MaryFinalReceiver: faded; disabling Mary.");

        gameObject.SetActive(false);
    }

    // -------------------------
    // Unity version compatibility helper
    // -------------------------
    static T FindFirstObjectByTypeCompat<T>() where T : Object
    {
#if UNITY_2023_1_OR_NEWER
        return Object.FindFirstObjectByType<T>();
#else
        return Object.FindObjectOfType<T>();
#endif
    }
}