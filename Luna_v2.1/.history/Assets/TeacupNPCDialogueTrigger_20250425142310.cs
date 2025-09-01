using UnityEngine;

public class TeacupNPCDialogueTrigger : MonoBehaviour
{
    /// <summary>
    /// Call this when the NPC "drinks" the teacup. 
    /// It enables all Dialogue System Triggers on this teacup.
    /// </summary>
    public void TriggerNPCDrink()
    {
        // Find all Dialogue System Trigger components (including inactive ones)
        var triggers = GetComponentsInChildren<PixelCrushers.DialogueSystem.DialogueSystemTrigger>(true);

        if (triggers == null || triggers.Length == 0)
        {
            Debug.LogWarning($"No DialogueSystemTrigger found on {gameObject.name} or its children.");
            return;
        }

        foreach (var trigger in triggers)
        {
            trigger.enabled = true;
            Debug.Log($"Enabled DialogueSystemTrigger on {trigger.gameObject.name}");
        }
    }
}
