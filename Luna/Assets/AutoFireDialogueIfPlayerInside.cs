using UnityEngine;
using PixelCrushers.DialogueSystem;

public class AutoFireDialogueIfPlayerInside : MonoBehaviour
{
    public DialogueSystemTrigger targetTrigger;

    private void OnEnable()
    {
        if (targetTrigger == null) return;

        // Check if player is already inside this trigger's collider
        Collider2D triggerCollider = targetTrigger.GetComponent<Collider2D>();
        if (triggerCollider == null) return;

        Collider2D playerCollider = FindPlayerCollider();
        if (playerCollider == null) return;

        if (triggerCollider.IsTouching(playerCollider))
        {
            targetTrigger.OnUse(); // ✅ Immediately fires the dialogue
        }
    }

    private Collider2D FindPlayerCollider()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return null;

        return player.GetComponent<Collider2D>();
    }
}
