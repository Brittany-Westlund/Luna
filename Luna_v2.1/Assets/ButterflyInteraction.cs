using UnityEngine;
using UnityEngine.UI;
using PixelCrushers.DialogueSystem; // Add this namespace to access Dialogue System classes

public class ButterflyInteraction : MonoBehaviour
{
    public GameObject interactionPrompt;
    public string conversationTitle; // Name of the conversation in the dialogue database

    void Start()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false); // Ensure the prompt is hidden initially
        }
    }

    void Update()
    {
        if (interactionPrompt != null && interactionPrompt.activeInHierarchy && Input.GetKeyDown(KeyCode.E)) // Assuming 'E' is the interaction key
        {
            Debug.Log("E key pressed, attempting to start dialogue: " + conversationTitle);
            StartDialogue();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered butterfly interaction zone.");
            interactionPrompt.SetActive(true); // Show interaction prompt
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player exited butterfly interaction zone.");
            interactionPrompt.SetActive(false); // Hide interaction prompt
            DialogueManager.StopConversation(); // Optional: Stop the conversation if the player walks away
        }
    }

    void StartDialogue()
    {
        if (!string.IsNullOrEmpty(conversationTitle))
        {
            DialogueManager.StartConversation(conversationTitle); // Start the conversation
            Debug.Log("Started conversation: " + conversationTitle);
        }
        else
        {
            Debug.LogWarning("Conversation title is not set for this butterfly interaction.");
        }
    }
}
