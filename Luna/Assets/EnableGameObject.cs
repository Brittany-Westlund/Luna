using UnityEngine;
using PixelCrushers.DialogueSystem;

public class GameObjectConversationController : MonoBehaviour
{
    // Assign this in the Unity Inspector for the GameObject you want to control
    public GameObject targetGameObject;

    // Assign the specific conversation title in the Unity Inspector that will trigger enabling/disabling the GameObject
    public string conversationTitle;

    // This function will be called when a conversation starts
    public void OnConversationStart(Conversation conversation)
    {
        // Get the current conversation's title by accessing the DialogueEntry
        string currentConversationTitle = conversation.Title;

        // Check if the current conversation's title matches the desired conversation title
        if (conversation != null && currentConversationTitle == conversationTitle)
        {
            EnableGameObject();  // Enable the GameObject if the conversation title matches
        }
    }

    // Function to enable the GameObject
    public void EnableGameObject()
    {
        if (targetGameObject != null)
        {
            targetGameObject.SetActive(true);  // Set the GameObject as active
            Debug.Log($"GameObject '{targetGameObject.name}' has been enabled for conversation: {conversationTitle}");
        }
        else
        {
            Debug.LogError("Target GameObject is not assigned.");
        }
    }

    // Optionally, you can create a function to disable the GameObject if needed
    public void DisableGameObject()
    {
        if (targetGameObject != null)
        {
            targetGameObject.SetActive(false);  // Set the GameObject as inactive
            Debug.Log($"GameObject '{targetGameObject.name}' has been disabled.");
        }
        else
        {
            Debug.LogError("Target GameObject is not assigned.");
        }
    }

    // Optional: Automatically disable GameObject when conversation ends
    public void OnConversationEnd(Conversation conversation)
    {
        // Get the current conversation's title by accessing the DialogueEntry
        string currentConversationTitle = conversation.Title;

        // Disable the GameObject when the conversation ends if it matches the title
        if (conversation != null && currentConversationTitle == conversationTitle)
        {
            DisableGameObject();
        }
    }
}
