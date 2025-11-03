using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;

public class ConversationChainTrigger_LayerTagHybrid : MonoBehaviour
{
    [Header("Tracked Conversations")]
    public List<string> prerequisiteConversations = new List<string>();

    [Header("Follow-Up Conversation")]
    public string followUpConversation;
    public string actorName = "ButterflyNPC";

    [Header("Collision Settings")]
    [Tooltip("Layer of the Player character (e.g. PlayerLayer).")]
    public string playerLayerName = "Player";

    [Tooltip("Tag of the NPC that should trigger the follow-up when the player collides.")]
    public string npcTag = "Butterfly";

    [Header("Optional Activation")]
    [Tooltip("Objects to activate when the follow-up is triggered.")]
    public GameObject[] objectsToActivate;

    [Header("Settings")]
    public float delayBeforeFollowUp = 0.5f;
    public bool triggerOnce = true;

    private HashSet<string> completedConversations = new HashSet<string>();
    private bool followUpReady = false;
    private bool followUpTriggered = false;
    private int playerLayer;

    private void Awake()
    {
        playerLayer = LayerMask.NameToLayer(playerLayerName);
        if (playerLayer == -1)
        {
            Debug.LogWarning($"⚠️ Layer '{playerLayerName}' not found! Please check spelling in {name}.");
        }
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

    // --- Called automatically by Dialogue System ---
    private void OnConversationEnded(Transform actor)
    {
        string convo = DialogueManager.lastConversationStarted;
        if (string.IsNullOrEmpty(convo)) return;

        if (prerequisiteConversations.Contains(convo))
        {
            completedConversations.Add(convo);
            Debug.Log($"📘 Conversation '{convo}' marked complete for {name}.");
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
        Debug.Log($"🌟 All prerequisite conversations complete for {name}. Follow-up armed: waiting for PlayerLayer ↔ {npcTag} collision.");
    }

    // --- Listen for NPC collisions (this must be on a trigger collider) ---
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!followUpReady || followUpTriggered) return;

        // If this object is not the NPC, ignore
        if (!CompareTag(npcTag)) return;

        // Check if the other object is on the correct player layer
        if (other.gameObject.layer == playerLayer)
        {
            Debug.Log($"🦋 Player (layer: {playerLayerName}) collided with '{name}' (tag: {npcTag}).");
            StartCoroutine(TriggerFollowUpAfterDelay());
        }
    }

    private IEnumerator TriggerFollowUpAfterDelay()
    {
        followUpTriggered = true;
        yield return new WaitForSeconds(delayBeforeFollowUp);

        // Activate objects
        foreach (var obj in objectsToActivate)
        {
            if (obj != null)
            {
                obj.SetActive(true);
                Debug.Log($"✨ Activated object: {obj.name}");
            }
        }

        // Wait until any existing dialogue ends
        yield return new WaitUntil(() => !DialogueManager.IsConversationActive);

        // Start follow-up conversation
        if (!string.IsNullOrEmpty(followUpConversation))
        {
            GameObject actorObj = GameObject.Find(actorName);
            if (actorObj != null)
                DialogueManager.StartConversation(followUpConversation, actorObj.transform);
            else
                DialogueManager.StartConversation(followUpConversation);

            Debug.Log($"✅ Follow-up conversation '{followUpConversation}' started.");
        }
    }
}
