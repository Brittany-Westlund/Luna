using UnityEngine;
using PixelCrushers.DialogueSystem;
using System.Collections;

public class MaryFinalPixelCompanion : MonoBehaviour
{
    [Header("Book")]
    public SpriteRenderer bigBookPageRenderer;

    [Header("Final Dialogue Trigger")]
    public GameObject finalDialogueTriggerGO;
    public string finalConversationName = "MaryGiveBook";

    [Header("Mary")]
    public GameObject maryRoot;
    public GameObject maryVisualRoot;
    public Collider2D maryInteractionCollider;

    [Header("Fade")]
    public float fadeSeconds = 1f;

    [Header("Debug")]
    public bool debugLogs = false;

    private bool lastBookOpen;
    private bool finalStarted = false;
    private bool convoFinished = false;
    private bool fading = false;

    private SpriteRenderer[] renderers;

    void Awake()
    {
        if (maryVisualRoot == null && maryRoot != null)
            maryVisualRoot = maryRoot;

        if (maryVisualRoot != null)
            renderers = maryVisualRoot.GetComponentsInChildren<SpriteRenderer>(true);

        if (finalDialogueTriggerGO != null)
            finalDialogueTriggerGO.SetActive(false);

        lastBookOpen = IsBookOpen();
    }

    void OnEnable()
    {
        DialogueManager.instance.conversationEnded += OnConversationEnded;
    }

    void OnDisable()
    {
        if (DialogueManager.instance != null)
            DialogueManager.instance.conversationEnded -= OnConversationEnded;
    }

    void Update()
    {
        bool openNow = IsBookOpen();

        if (openNow != lastBookOpen)
        {
            if (openNow) OnBookOpened();
            else OnBookClosed();

            lastBookOpen = openNow;
        }
    }

    bool IsBookOpen()
    {
        if (bigBookPageRenderer == null) return false;
        return bigBookPageRenderer.enabled;
    }

    void OnBookOpened()
    {
        if (debugLogs) Debug.Log("📖 Book opened.");

        if (finalStarted) return;
        finalStarted = true;

        if (maryInteractionCollider != null)
            maryInteractionCollider.enabled = false;

        if (finalDialogueTriggerGO != null)
            finalDialogueTriggerGO.SetActive(true);
    }

    void OnBookClosed()
    {
        if (!finalStarted) return;
        if (!convoFinished) return;
        if (fading) return;

        if (debugLogs) Debug.Log("📖 Book closed after convo — fading Mary.");

        StartCoroutine(FadeThenDisable());
    }

    void OnConversationEnded(Transform actor)
    {
        var convo = DialogueManager.LastConversationStarted;
        if (convo == finalConversationName)
        {
            convoFinished = true;
            if (debugLogs) Debug.Log("🌿 Final Mary conversation finished.");
        }
    }

    IEnumerator FadeThenDisable()
    {
        fading = true;

        if (renderers == null || renderers.Length == 0)
        {
            if (maryRoot != null)
                maryRoot.SetActive(false);
            yield break;
        }

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

        if (maryRoot != null)
            maryRoot.SetActive(false);

        fading = false;
    }
}