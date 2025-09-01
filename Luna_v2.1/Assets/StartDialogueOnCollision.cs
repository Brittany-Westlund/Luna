using UnityEngine;
using PixelCrushers.DialogueSystem;

public class StartDialogueOnCollision : MonoBehaviour
{
    public string conversationTitle; // The title of the conversation to start
    public GameObject object1; // Reference to the first GameObject
    public GameObject object2; // Reference to the second GameObject

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collided object matches either object1 or object2
        if (other.gameObject == object1 || other.gameObject == object2)
        {
            if (!string.IsNullOrEmpty(conversationTitle))
            {
                // Start the dialogue using the Dialogue Manager
                DialogueManager.StartConversation(conversationTitle, transform);
            }
        }
    }
}
