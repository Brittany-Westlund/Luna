using UnityEngine;
using PixelCrushers.DialogueSystem;

public class FlowerToTargetEnabler_Full : MonoBehaviour
{
    [Header("Flower Sprite To Watch (LitFlowerB)")]
    public SpriteRenderer litFlowerRenderer;

    [Header("Targets To Enable (Assign Any or All)")]
    public GameObject targetGameObject;
    public SpriteRenderer targetSprite;
    public Behaviour targetComponent;
    public DialogueSystemTrigger targetDialogueTrigger;

    [Header("Optional Settings")]
    public bool onlyOnce = true;
    public bool alsoSetActiveDialogueObject = true;

    private bool hasFired = false;

    void Update()
    {
        if (litFlowerRenderer == null)
            return;

        if (!litFlowerRenderer.enabled)
            return;

        if (onlyOnce && hasFired)
            return;

        // ✅ Enable GameObject
        if (targetGameObject != null)
            targetGameObject.SetActive(true);

        // ✅ Enable SpriteRenderer
        if (targetSprite != null)
            targetSprite.enabled = true;

        // ✅ Enable Component
        if (targetComponent != null)
            targetComponent.enabled = true;

        // ✅ Fire Dialogue Trigger
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
