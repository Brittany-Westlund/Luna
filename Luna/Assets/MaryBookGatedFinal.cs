using UnityEngine;
using System.Collections;

public class MaryBookGatedFinal : MonoBehaviour
{
    [Header("Book (best)")]
    public BookPageController bookPageController; // optional; auto-uses BookPageController.Instance

    [Header("Book (fallback)")]
    public SpriteRenderer bigBookPageRenderer; // optional fallback if you want

    [Header("Final Dialogue Trigger (starts on enable)")]
    public GameObject finalDialogueTriggerGO; // your FinalDialogue object

    [Header("Mary Root / Visuals")]
    public GameObject maryRoot;          // usually MaryNPC (can be same object)
    public GameObject maryVisualRoot;    // where SpriteRenderers live; leave null if on MaryNPC
    public Collider2D maryInteractionCollider; // optional: disable interaction when final starts

    [Header("Fade")]
    public float fadeSeconds = 1.0f;

    [Header("Debug")]
    public bool debugLogs = false;

    // internal state
    private bool lastBookOpen = false;
    private bool finalStarted = false;
    private bool convoFinished = false;
    private bool fadeStarted = false;

    private SpriteRenderer[] renderers;

    void Awake()
    {
        if (maryRoot == null) maryRoot = gameObject;

        if (maryVisualRoot == null) maryVisualRoot = maryRoot;

        renderers = maryVisualRoot.GetComponentsInChildren<SpriteRenderer>(true);

        if (finalDialogueTriggerGO != null)
            finalDialogueTriggerGO.SetActive(false);

        if (bookPageController == null && BookPageController.Instance != null)
            bookPageController = BookPageController.Instance;

        lastBookOpen = IsBookOpenNow();
    }

    void Update()
    {
        // BookPageController.Instance can come/go across scenes; reacquire safely.
        if (bookPageController == null && BookPageController.Instance != null)
            bookPageController = BookPageController.Instance;

        bool openNow = IsBookOpenNow();

        if (openNow != lastBookOpen)
        {
            if (openNow) OnBookOpened();
            else OnBookClosed();

            lastBookOpen = openNow;
        }
    }

    bool IsBookOpenNow()
    {
        // Best: authoritative flag
        if (bookPageController != null)
            return bookPageController.IsOpen;

        // Fallback: renderer enabled means "open"
        if (bigBookPageRenderer != null)
            return bigBookPageRenderer.enabled;

        // If neither is assigned, we can't detect it.
        return false;
    }

    void OnBookOpened()
    {
        if (debugLogs) Debug.Log("🌿 MaryBookGatedFinal: Book opened.");

        if (finalStarted) return;
        finalStarted = true;

        if (maryInteractionCollider != null)
            maryInteractionCollider.enabled = false;

        if (finalDialogueTriggerGO != null)
        {
            finalDialogueTriggerGO.SetActive(true);
            if (debugLogs) Debug.Log("🌿 MaryBookGatedFinal: Enabled FinalDialogue trigger.");
        }
        else
        {
            Debug.LogWarning("🌿 MaryBookGatedFinal: finalDialogueTriggerGO is null.");
        }
    }

    void OnBookClosed()
    {
        if (debugLogs)
            Debug.Log($"🌿 MaryBookGatedFinal: Book closed. finalStarted={finalStarted} convoFinished={convoFinished} fadeStarted={fadeStarted}");

        if (!finalStarted) return;
        if (!convoFinished) return;
        if (fadeStarted) return;

        StartCoroutine(FadeThenDisable());
    }

    // 🔥 CALL THIS FROM Dialogue System Trigger -> On Conversation End
    public void MarkConversationFinished()
    {
        convoFinished = true;
        if (debugLogs) Debug.Log("🌿 MaryBookGatedFinal: Conversation finished flag set.");
    }

    IEnumerator FadeThenDisable()
    {
        fadeStarted = true;

        if (renderers == null || renderers.Length == 0)
        {
            if (debugLogs) Debug.LogWarning("🌿 MaryBookGatedFinal: No SpriteRenderers found; disabling instantly.");
            maryRoot.SetActive(false);
            yield break;
        }

        // Cache original colors
        Color[] start = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            start[i] = renderers[i].color;

        float t = 0f;
        while (t < fadeSeconds)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / fadeSeconds);
            float a = Mathf.Lerp(1f, 0f, k);

            for (int i = 0; i < renderers.Length; i++)
            {
                Color c = start[i];
                c.a = a;
                renderers[i].color = c;
            }

            yield return null;
        }

        if (debugLogs) Debug.Log("🌿 MaryBookGatedFinal: Fade complete; disabling Mary.");

        maryRoot.SetActive(false);
    }
}