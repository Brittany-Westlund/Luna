using UnityEngine;
using PixelCrushers.DialogueSystem;
using System.Collections;

public class PersistentNPCExit : MonoBehaviour
{
    [Header("Save Data")]
    public CollectibleState worldState;        // Drag in your CollectibleState asset
    public string saveID = "";                 // Optional unique ID; defaults to GameObject name

    [Header("Conversation Trigger")]
    [Tooltip("Only trigger exit after this specific conversation. Leave blank to react to any.")]
    public string triggerConversation = "";

    [Header("Behavior")]
    public bool disableAfterConversation = true;
    public bool moveAwayAfterConversation = false;
    public bool fadeOutAfterConversation = false;
    public float disableDelay = 1f;

    [Header("Movement Settings")]
    public Vector3 moveDirection = new Vector3(2f, 0f, 0f);
    public float moveSpeed = 1.5f;

    [Header("Fade Settings")]
    public float fadeSpeed = 1f;

    private bool hasExited = false;
    private bool isMoving = false;
    private Vector3 targetPos;
    private SpriteRenderer sr;
    private string ID => string.IsNullOrEmpty(saveID) ? name : saveID;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        // Check saved state on load
        if (worldState != null && worldState.HasCollected(ID))
        {
            gameObject.SetActive(false);
            hasExited = true;
        }
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
        if (!isMoving) return;

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

    private void OnConversationEnd(Transform actor)
    {
        if (hasExited) return;

        // Only react if this NPC was the speaker
        var speaker = DialogueManager.currentConversationState?.subtitle?.speakerInfo?.transform;
        if (speaker == null || speaker != transform) return;

        // Optional filter by specific conversation name
        if (!string.IsNullOrEmpty(triggerConversation) &&
            DialogueManager.lastConversationStarted != triggerConversation)
            return;

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
        if (worldState != null && !worldState.HasCollected(ID))
        {
            worldState.MarkCollected(ID);
            Debug.Log($"[PersistentNPCExit] Saved '{ID}' as disabled (JSON).");
        }

        gameObject.SetActive(false);
    }
}
