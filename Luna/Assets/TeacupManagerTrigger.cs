using UnityEngine;
using PixelCrushers.DialogueSystem;

public class TeacupManagerTrigger : MonoBehaviour
{
    [Header("Teacup Settings")]
    [Tooltip("All teacups to monitor for fade-off state.")]
    public TeacupToggle[] teacups;

    [Header("Dialogue Settings")]
    [Tooltip("Name of the conversation to start when all teacups are off.")]
    public string conversationToStart = "TeacupsAllOff";
    [Tooltip("Optional actor name for who starts the conversation.")]
    public string actorName = "Luna";

    [Header("Check Settings")]
    [Tooltip("How often to check teacup states (in seconds).")]
    public float checkInterval = 0.5f;

    private bool dialogueTriggered = false;

    void Start()
    {
        if (teacups == null || teacups.Length == 0)
        {
            Debug.LogWarning($"{name}: No teacups assigned to TeacupManagerTrigger.");
            return;
        }

        InvokeRepeating(nameof(CheckTeacups), 0f, checkInterval);
    }

    void CheckTeacups()
    {
        if (dialogueTriggered) return;

        bool allOff = true;

        foreach (var teacup in teacups)
        {
            if (teacup == null) continue;

            var sr = teacup.GetComponent<SpriteRenderer>();
            if (sr != null && sr.color.a > 0.05f) // still visible
            {
                allOff = false;
                break;
            }
        }

        if (allOff)
        {
            TriggerDialogue();
        }
    }

    void TriggerDialogue()
    {
        dialogueTriggered = true;
        Debug.Log($"☕ All teacups toggled off! Starting conversation '{conversationToStart}'");

        if (string.IsNullOrEmpty(conversationToStart))
        {
            Debug.LogWarning($"{name}: No conversation name assigned.");
            return;
        }

        // Try to start the conversation with the given actor name if it exists.
        if (!string.IsNullOrEmpty(actorName))
        {
            GameObject actorObj = GameObject.Find(actorName);
            if (actorObj != null)
            {
                // Use actor's name string instead of its Transform to prevent Dialogue System from deactivating it.
                DialogueManager.StartConversation(conversationToStart);

                return;
            }
        }

        // Fallback: start conversation without specifying an actor
        DialogueManager.StartConversation(conversationToStart);
    }
}
