using UnityEngine;
using PixelCrushers.DialogueSystem;
using System.Collections;

public class FairypetalConversationExit : MonoBehaviour
{
    [Header("Exit Behavior")]
    public bool disableAfterConversation = true;
    public bool moveAwayAfterConversation = false;
    public bool fadeOutAfterConversation = false;

    [Header("Movement Settings")]
    public Vector3 moveDirection = new Vector3(2f, 0f, 0f);
    public float moveSpeed = 1.5f;

    [Header("Fade Settings")]
    public float fadeSpeed = 1f;

    [Header("Delay")]
    public float disableDelay = 1f;

    private bool hasExited = false;
    private bool isMoving = false;
    private Vector3 targetPos;
    private SpriteRenderer sr;
    private string saveKey;

    private void Awake()
    {
        // Unique key so it remembers across sessions
        saveKey = $"{name}_Disabled";

        // Check saved state on load
        if (PlayerPrefs.GetInt(saveKey, 0) == 1)
        {
            gameObject.SetActive(false);
            hasExited = true;
        }
    }

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if (DialogueManager.instance != null)
            DialogueManager.instance.conversationEnded += OnConversationEnd;
    }

    private void OnDisable()
    {
        if (DialogueManager.hasInstance)
            DialogueManager.instance.conversationEnded -= OnConversationEnd;
    }

    private void Update()
    {
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPos) < 0.05f)
            {
                isMoving = false;

                if (fadeOutAfterConversation)
                    StartCoroutine(FadeOutAndDisable());
                else if (disableAfterConversation)
                    Invoke(nameof(DisableSelf), disableDelay);
            }
        }
    }

    private void OnConversationEnd(Transform actor)
    {
        if (hasExited) return;

        // Check that this NPC was the one speaking
        var lastSpeaker = DialogueManager.currentConversationState?.subtitle?.speakerInfo?.transform?.name;
        if (string.IsNullOrEmpty(lastSpeaker) || lastSpeaker != name) return;

        hasExited = true;

        if (moveAwayAfterConversation)
        {
            targetPos = transform.position + moveDirection;
            isMoving = true;
        }
        else if (fadeOutAfterConversation)
        {
            StartCoroutine(FadeOutAndDisable());
        }
        else if (disableAfterConversation)
        {
            Invoke(nameof(DisableSelf), disableDelay);
        }
    }

    private IEnumerator FadeOutAndDisable()
    {
        if (sr == null) yield break;
        Color c = sr.color;

        while (c.a > 0.05f)
        {
            c.a = Mathf.MoveTowards(c.a, 0f, Time.deltaTime * fadeSpeed);
            sr.color = c;
            yield return null;
        }

        DisableSelf();
    }

    private void DisableSelf()
    {
        PlayerPrefs.SetInt(saveKey, 1); // Remember disabled state
        PlayerPrefs.Save();
        gameObject.SetActive(false);
        Debug.Log($"{name} disabled and state saved.");
    }
}
