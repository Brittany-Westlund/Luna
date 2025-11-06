using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;

/// <summary>
/// 🪄 A safe, queue-based chain trigger for Dialogue System conversations.
/// - Waits for prerequisite conversations to finish.
/// - Triggers follow-up only once, in order.
/// - Never disables or locks the player.
/// </summary>
[DisallowMultipleComponent]
public class ConversationChainTrigger_Safe : MonoBehaviour
{
    [Header("Tracked Conversations")]
    public List<string> prerequisiteConversations = new();

    [Header("Follow-Up Conversation")]
    public string followUpConversation;
    public string actorName = "ButterflyNPC";

    [Header("Collision Settings")]
    public string playerLayerName = "Player";
    public string npcTag = "Butterfly";

    [Header("Optional Activation")]
    public GameObject[] objectsToActivate;

    [Header("Settings")]
    public float delayBeforeFollowUp = 0.5f;
    public bool triggerOnce = true;

    // --- Private runtime data ---
    private HashSet<string> completedConversations = new();
    private bool followUpReady = false;
    private bool followUpTriggered = false;
    private int playerLayer;

    // --- Shared static queue for all chain triggers ---
    private static readonly Queue<IEnumerator> conversationQueue = new();
    private static bool queueRunning = false;

    // --- Cached player reference ---
    private GameObject player;

    // --------------------------------------------------------

    private void Awake()
    {
        playerLayer = LayerMask.NameToLayer(playerLayerName);
        if (playerLayer == -1)
            Debug.LogWarning($"⚠️ Layer '{playerLayerName}' not found on {name}");

        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void OnEnable()
    {
        if (DialogueManager.instance != null)
            DialogueManager.instance.conversationEnded += OnConversationEnded;
    }

    private void OnDisable()
    {
        if (DialogueManager.hasInstance)
            DialogueManager.instance.conversationEnded -= OnConversationEnded;
    }

    // --------------------------------------------------------

    private void OnConversationEnded(Transform actor)
    {
        string convo = DialogueManager.lastConversationStarted;
        if (string.IsNullOrEmpty(convo)) return;

        if (prerequisiteConversations.Contains(convo))
        {
            completedConversations.Add(convo);
            Debug.Log($"📘 Conversation '{convo}' marked complete for {name}");
            CheckCompletion();
        }
    }

    private void CheckCompletion()
    {
        if (followUpTriggered && triggerOnce) return;

        foreach (string convo in prerequisiteConversations)
        {
            if (!completedConversations.Contains(convo))
                return;
        }

        followUpReady = true;
        Debug.Log($"🌟 All prerequisite conversations complete for {name}. Ready for follow-up.");
    }

    // --------------------------------------------------------

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!followUpReady || followUpTriggered) return;
        if (!CompareTag(npcTag)) return;
        if (other.gameObject.layer != playerLayer) return;

        followUpTriggered = true;
        Debug.Log($"🦋 Player collided with '{name}'. Queuing follow-up conversation.");
        QueueFollowUp();
    }

    // --------------------------------------------------------
    // 🔁 Conversation Queue System
    // --------------------------------------------------------

    private void QueueFollowUp()
    {
        conversationQueue.Enqueue(FollowUpRoutine());
        if (!queueRunning)
            StartCoroutine(ProcessQueue());
    }

    private IEnumerator ProcessQueue()
    {
        queueRunning = true;
        while (conversationQueue.Count > 0)
        {
            yield return StartCoroutine(conversationQueue.Dequeue());
        }
        queueRunning = false;
    }

    // --------------------------------------------------------
    // 🌙 Safe Follow-Up Routine
    // --------------------------------------------------------

    private IEnumerator FollowUpRoutine()
    {
        yield return new WaitForSeconds(delayBeforeFollowUp);

        // 🌼 Activate linked objects
        foreach (var obj in objectsToActivate)
        {
            if (obj != null)
            {
                obj.SetActive(true);
                Debug.Log($"✨ Activated object: {obj.name}");
            }
        }

        // Wait for any existing conversation to finish fully
        yield return new WaitUntil(() => !DialogueManager.IsConversationActive);
        yield return new WaitForSeconds(0.25f); // allow cleanup

        // Ensure the player exists and is active
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        if (player != null && !player.activeSelf)
        {
            player.SetActive(true);
            Debug.Log("🧭 Player re-enabled before starting follow-up.");
        }

        // 🗣 Start follow-up conversation
        if (!string.IsNullOrEmpty(followUpConversation))
        {
            GameObject actorObj = GameObject.Find(actorName);

            if (actorObj != null)
                DialogueManager.StartConversation(followUpConversation, actorObj.transform);
            else
                DialogueManager.StartConversation(followUpConversation);

            Debug.Log($"✅ Follow-up conversation '{followUpConversation}' started.");

            // Force player reactivation again shortly after (DialogueSystem disables automatically)
            StartCoroutine(ReenablePlayerAfterDelay(0.4f));
        }

        // Wait for that conversation to end before processing the next in queue
        yield return new WaitUntil(() => !DialogueManager.IsConversationActive);
        yield return new WaitForSeconds(0.25f);
    }

    private IEnumerator ReenablePlayerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        if (player != null && !player.activeSelf)
        {
            player.SetActive(true);
            Debug.Log("🧭 Player re-enabled after Dialogue System auto-disable.");
        }

        // Some controllers disable components instead of the whole GameObject
        // Re-enable common ones if present:
        var behaviourScripts = player.GetComponents<Behaviour>();
        foreach (var b in behaviourScripts)
        {
            if (b != null && !b.enabled)
                b.enabled = true;
        }
    }
}
