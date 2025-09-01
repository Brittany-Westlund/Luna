using UnityEngine;
using PixelCrushers.DialogueSystem;

public class GameObjectConversationController : MonoBehaviour
{
    // Assign this in the Unity Inspector for each GameObject you want to control
    public GameObject targetGameObject;
    public string conversationTitle;

    // This function will enable the GameObject only when the corresponding conversation title is triggered
    public void EnableGameObjectForConversation(string conversation)
    {
        if (conversation == conversationTitle)
        {
            if (targetGameObject != null)
            {
                targetGameObject.SetActive(true);
                Debug.Log($"GameObject '{targetGameObject.name}' has been enabled for conversation: {conversationTitle}");
            }
            else
            {
                Debug.LogError("Target GameObject is not assigned.");
            }
        }
    }

    // Optionally, you could create a function to disable the GameObject
    public void DisableGameObject()
    {
        if (targetGameObject != null)
        {
            targetGameObject.SetActive(false);
            Debug.Log($"GameObject '{targetGameObject.name}' has been disabled.");
        }
        else
        {
            Debug.LogError("Target GameObject is not assigned.");
        }
    }
}
