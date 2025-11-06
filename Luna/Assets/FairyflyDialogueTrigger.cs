using UnityEngine;
using System.Collections;
using PixelCrushers.DialogueSystem;

[RequireComponent(typeof(Collider2D))]
public class FairyflyDialogueTrigger : MonoBehaviour
{
    [Header("Conversation Settings")]
    public string conversationName;
    public string actorName = "Fairyfly";
    public float delayBeforeStart = 0.2f;
    public bool triggerOnce = true;

    private bool hasTriggered = false;

    void Awake()
    {
        // Just making sure the collider is set up properly
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void Start()
    {
        // For debugging clarity — should only log this once on startup.
        Debug.Log($"🌙 FairyflyDialogueTrigger initialized on {gameObject.name}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ✅ Make sure this only runs when Fairyfly overlaps
        if (hasTriggered && triggerOnce) return;

        // Make sure it’s the Fairyfly (by tag or name match)
        if (!other.CompareTag("Fairyfly") && !other.name.Contains("Fairyfly")) return;

        Debug.Log($"🧚 Fairyfly overlapped trigger: {gameObject.name}");
        hasTriggered = true;
        StartCoroutine(StartConversationAfterDelay());
    }

    private IEnumerator StartConversationAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeStart);

        // Wait until no conversation is active
        yield return new WaitUntil(() => !DialogueManager.IsConversationActive);

        if (!string.IsNullOrEmpty(conversationName))
        {
            GameObject actorObj = GameObject.Find(actorName);
            if (actorObj != null)
                DialogueManager.StartConversation(conversationName, actorObj.transform);
            else
                DialogueManager.StartConversation(conversationName);

            Debug.Log($"🎬 Fairyfly triggered conversation: {conversationName}");
        }
    }
}
