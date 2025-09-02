using UnityEngine;
using PixelCrushers.DialogueSystem;

public class SetConversationVariable : MonoBehaviour
{
    // Method to set the variable
    public void SetHasConversationOccurredTrue()
    {
        // This line sets the Dialogue System variable "HasConversationOccurred" to true
        DialogueLua.SetVariable("HasConversationOccurred", true);
    }
}
