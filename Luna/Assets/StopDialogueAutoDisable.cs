using UnityEngine;
using PixelCrushers.DialogueSystem;

public class StopDialogueAutoDisable : MonoBehaviour
{
    void OnEnable()
    {
        DialogueManager.Instance.conversationEnded += OnConversationEnded;
    }

    void OnDisable()
    {
        if (DialogueManager.HasInstance)
            DialogueManager.Instance.conversationEnded -= OnConversationEnded;
    }

    void OnConversationEnded(Transform actor)
    {
        // Re-enable Luna if DialogueSystem tried to disable her.
        var luna = GameObject.Find("Luna");
        if (luna != null && !luna.activeSelf)
        {
            Debug.Log("DialogueSystem tried to disable Luna—re-enabling.");
            luna.SetActive(true);
        }
    }
}
