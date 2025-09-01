using UnityEngine;
using PixelCrushers.DialogueSystem;

public class InteractableObject : MonoBehaviour
{
    [Header("Interaction Settings")]
    public KeyCode interactionKey = KeyCode.T;
    public GameObject interactionPrompt;
    public string[] conversationTitles;
    public int requiredActivationCount = 10;
    private int currentDialogueIndex = 0;
    private bool actionTriggered = false;
    private bool isPlayerInRange = false;

    void Start()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    void Update()
    {
        // Check interaction within range
        if (isPlayerInRange && Input.GetKeyDown(interactionKey))
        {
            StartDialogue();
        }

        // Check if LanternLitCount has reached required count
        int currentCount = DialogueLua.GetVariable("LanternLitCount").asInt;
        if (!actionTriggered && currentCount >= requiredActivationCount)
        {
            actionTriggered = true;
            TriggerActions();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && interactionPrompt != null)
        {
            isPlayerInRange = true;
            interactionPrompt.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && interactionPrompt != null)
        {
            isPlayerInRange = false;
            interactionPrompt.SetActive(false);
            DialogueManager.StopConversation(); // Optional: stop if player leaves
        }
    }

    void StartDialogue()
    {
        if (currentDialogueIndex < conversationTitles.Length)
        {
            string conversationTitle = conversationTitles[currentDialogueIndex];
            if (!string.IsNullOrEmpty(conversationTitle))
            {
                DialogueManager.StartConversation(conversationTitle);
                Debug.Log($"Started dialogue: {conversationTitle}");
                currentDialogueIndex++;
            }
            else
            {
                Debug.LogWarning("Conversation title is not set.");
            }
        }
    }

    private void TriggerActions()
    {
        StartDialogue();
        // Any additional actions, e.g., enabling an object
    }
}
