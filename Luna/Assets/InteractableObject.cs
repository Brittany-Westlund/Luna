using UnityEngine;
using PixelCrushers.DialogueSystem;

public class InteractableObject : MonoBehaviour
{
    [Header("Interaction Settings")]
    public KeyCode interactionKey = KeyCode.E;
    public GameObject interactionPrompt;

    [System.Serializable]
    public class ConversationMapping
    {
        public string conversationTitle;
        public GameObject associatedObject;
        [TextArea]
        public string[] completionConditions;
    }

    public ConversationMapping[] conversationMappings;

    private bool isPlayerInRange = false;

    void Start()
    {
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(interactionKey))
        {
            Debug.Log("Interaction key pressed.");
            TryStartConversation();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            if (interactionPrompt != null) interactionPrompt.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (interactionPrompt != null) interactionPrompt.SetActive(false);
        }
    }

    private void TryStartConversation()
    {
        foreach (var mapping in conversationMappings)
        {
            Debug.Log($"Checking conversation: {mapping.conversationTitle}");

            if (CanStartConversation(mapping))
            {
                Debug.Log($"Starting conversation: {mapping.conversationTitle}");
                DialogueManager.StartConversation(mapping.conversationTitle);

                if (mapping.associatedObject != null)
                {
                    mapping.associatedObject.SetActive(true);
                    Debug.Log($"Enabled associated object: {mapping.associatedObject.name}");
                }

                return;
            }
        }

        Debug.LogWarning("No conversations available to start.");
    }

    private bool CanStartConversation(ConversationMapping mapping)
    {
        foreach (var condition in mapping.completionConditions)
        {
            if (!EvaluateLuaCondition(condition))
            {
                Debug.Log($"Condition failed: {condition}");
                return false;
            }
        }

        Debug.Log($"All conditions passed for: {mapping.conversationTitle}");
        return true;
    }

    private bool EvaluateLuaCondition(string condition)
    {
        try
        {
            var result = DialogueLua.GetVariable(condition).AsBool;
            Debug.Log($"Evaluating condition: {condition} -> {result}");
            return result;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error evaluating Lua condition: {condition}. Exception: {e.Message}");
            return false;
        }
    }
}
