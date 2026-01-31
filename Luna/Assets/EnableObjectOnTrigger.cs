using UnityEngine;
using PixelCrushers.DialogueSystem;

public class EnableTargetsOnTrigger_Full : MonoBehaviour
{
    [Header("What Can Be Enabled (Any or All)")]
    public GameObject targetGameObject;
    public SpriteRenderer targetSprite;
    public DialogueSystemTrigger targetDialogueTrigger;

    [Header("Trigger Settings")]
    public string requiredTag = "Player";
    public bool onlyOnce = true;

    private bool hasFired = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (onlyOnce && hasFired)
            return;

        if (!other.CompareTag(requiredTag))
            return;

        // ✅ Enable GameObject
        if (targetGameObject != null)
            targetGameObject.SetActive(true);

        // ✅ Enable SpriteRenderer
        if (targetSprite != null)
            targetSprite.enabled = true;

        // ✅ Fire Dialogue Trigger
        if (targetDialogueTrigger != null)
        {
            targetDialogueTrigger.gameObject.SetActive(true);
            targetDialogueTrigger.enabled = false; // reset
            targetDialogueTrigger.enabled = true;  // fire OnEnable
        }

        hasFired = true;
    }
}
