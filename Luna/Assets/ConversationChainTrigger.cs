using UnityEngine;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;

[DisallowMultipleComponent]
public class ConversationChainTrigger_Safe : MonoBehaviour
{
    [Header("Tracked Conversations")]
    public List<string> prerequisiteConversations = new List<string>();

    [Header("Follow-Up Conversation")]
    public string followUpConversation;
    public string actorName = "ButterflyNPC";
    public string conversantName = "";

    [Header("Options")]
    public bool triggerOnce = true;
    public bool autoStartAsSoonAsReady = false;

    [Header("Debug")]
    public bool debugLogs = true;

    private HashSet<string> completedConversations = new HashSet<string>();
    private bool followUpTriggered = false;

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

    private void OnConversationEnded(Transform actorTransform)
    {
        string convo = DialogueManager.lastConversationStarted;

        if (!string.IsNullOrEmpty(convo))
            MarkConversationComplete(convo);
    }

    public void MarkConversationComplete(string conversationTitle)
    {
        if (string.IsNullOrEmpty(conversationTitle))
            return;

        if (prerequisiteConversations.Contains(conversationTitle))
        {
            completedConversations.Add(conversationTitle);

            if (debugLogs)
                Debug.Log("ConversationChainTrigger_Safe: Completed prerequisite -> " + conversationTitle);
        }

        if (autoStartAsSoonAsReady)
            TryStartFollowUp();
    }

    public void TryStartFollowUp()
    {
        if (triggerOnce && followUpTriggered)
            return;

        if (!AllPrerequisitesComplete())
        {
            if (debugLogs)
                Debug.Log("ConversationChainTrigger_Safe: Not ready yet.");
            return;
        }

        StartFollowUpNow();
    }

    public void StartFollowUpNow()
    {
        if (triggerOnce && followUpTriggered)
            return;

        if (string.IsNullOrEmpty(followUpConversation))
        {
            Debug.LogWarning("ConversationChainTrigger_Safe: followUpConversation is empty.");
            return;
        }

        if (DialogueManager.IsConversationActive)
        {
            if (debugLogs)
                Debug.Log("ConversationChainTrigger_Safe: A conversation is already active.");
            return;
        }

        Transform actor = FindTransformByName(actorName);
        Transform conversant = FindTransformByName(conversantName);

        followUpTriggered = true;

        if (actor != null || conversant != null)
            DialogueManager.StartConversation(followUpConversation, actor, conversant);
        else
            DialogueManager.StartConversation(followUpConversation);

        if (debugLogs)
            Debug.Log("ConversationChainTrigger_Safe: Started follow-up -> " + followUpConversation);
    }

    private Transform FindTransformByName(string objectName)
    {
        if (string.IsNullOrEmpty(objectName))
            return null;

        GameObject found = GameObject.Find(objectName);

        if (found == null)
        {
            if (debugLogs)
                Debug.LogWarning("ConversationChainTrigger_Safe: Could not find object named -> " + objectName);

            return null;
        }

        return found.transform;
    }

    private bool AllPrerequisitesComplete()
    {
        foreach (string convo in prerequisiteConversations)
        {
            if (!completedConversations.Contains(convo))
                return false;
        }

        return true;
    }
}