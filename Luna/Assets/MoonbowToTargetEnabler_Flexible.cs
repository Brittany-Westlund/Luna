using UnityEngine;
using PixelCrushers.DialogueSystem;

public class MoonbowToTargetEnabler_Full : MonoBehaviour
{
    [Header("Moonbow To Watch")]
    public SpriteRenderer moonbowRenderer;

    [Header("Target To Enable (Assign Any or All)")]
    public GameObject targetGameObject;
    public SpriteRenderer targetSprite;
    public DialogueSystemTrigger targetDialogueTrigger;

    [Header("Optional Settings")]
    public bool onlyOnce = true;
    public bool alsoSetActiveDialogueObject = true;

    private bool hasFired = false;

    void Update()
    {
        if (moonbowRenderer == null)
            return;

        if (!moonbowRenderer.enabled)
            return;

        if (onlyOnce && hasFired)
            return;

        // ✅ Enable GameObject
        if (targetGameObject != null)
            targetGameObject.SetActive(true);

        // ✅ Enable SpriteRenderer
        if (targetSprite != null)
            targetSprite.enabled = true;

        // ✅ Enable Dialogue Trigger
        if (targetDialogueTrigger != null)
        {
            if (alsoSetActiveDialogueObject)
                targetDialogueTrigger.gameObject.SetActive(true);

            targetDialogueTrigger.enabled = false; // reset
            targetDialogueTrigger.enabled = true;  // fire OnEnable
        }

        hasFired = true;
    }
}
