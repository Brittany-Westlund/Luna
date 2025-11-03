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
    public float checkInterval = 0.5f; // how often to check teacup states

    private bool dialogueTriggered = false;

    void Start()
    {
        InvokeRepeating(nameof(CheckTeacups), 0f, checkInterval);
    }

    void CheckTeacups()
    {
        if (dialogueTriggered || teacups == null || teacups.Length == 0)
            return;

        bool allOff = true;

        foreach (var teacup in teacups)
        {
            if (teacup != null)
            {
                var sr = teacup.GetComponent<SpriteRenderer>();
                if (sr != null && sr.color.a > 0.05f) // still visible
                {
                    allOff = false;
                    break;
                }
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

        if (!string.IsNullOrEmpty(conversationToStart))
        {
            if (!string.IsNullOrEmpty(actorName))
            {
                GameObject actorObj = GameObject.Find(actorName);
                if (actorObj != null)
                {
                    DialogueManager.StartConversation(conversationToStart, actorObj.transform);
                }
                else
                {
                    DialogueManager.StartConversation(conversationToStart);
                }
            }
            else
            {
                DialogueManager.StartConversation(conversationToStart);
            }
        }
    }
}
