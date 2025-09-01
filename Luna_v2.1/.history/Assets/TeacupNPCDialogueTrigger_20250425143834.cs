using UnityEngine;

public class TeacupNPCDialogueTrigger : MonoBehaviour
{
    /// <summary>
    /// Call this when the NPC "drinks" the teacup. 
    /// It enables all Dialogue System Triggers on this teacup.
    /// </summary>
    public void TriggerNPCDrink()
{
    var triggers = GetComponentsInChildren<PixelCrushers.DialogueSystem.DialogueSystemTrigger>(true);

    if (triggers == null || triggers.Length == 0)
    {
        Debug.LogWarning($"No DialogueSystemTrigger found on {gameObject.name} or its children.");
        return;
    }

    foreach (var trigger in triggers)
    {
        // This ensures OnEnable fires (if needed)
        trigger.enabled = false;
        trigger.enabled = true;
        Debug.Log($"Toggled DialogueSystemTrigger on {trigger.gameObject.name}");

        // And this will always fire the action, regardless of inspector settings:
        trigger.OnUse();
        Debug.Log($"Fired OnUse() on DialogueSystemTrigger on {trigger.gameObject.name}");
    }
}

}
