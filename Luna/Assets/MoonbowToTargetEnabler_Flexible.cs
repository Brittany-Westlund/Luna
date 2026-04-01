using UnityEngine;
using PixelCrushers.DialogueSystem;

public class MoonbowToTargetEnabler_Full : MonoBehaviour
{
    [Header("Moonbow To Watch")]
    public SpriteRenderer moonbowRenderer;

    [Header("Visibility Threshold")]
    [Range(0f, 1f)]
    public float requiredAlpha = 0.95f;

    [Header("Target To Enable (Assign Any or All)")]
    public GameObject targetGameObject;
    public SpriteRenderer targetSprite;
    public DialogueSystemTrigger targetDialogueTrigger;

    [Header("Optional Settings")]
    public bool onlyOnce = true;
    public bool alsoSetActiveDialogueObject = true;
    public bool requireMoonbowGameObjectActive = true;

    private bool hasFired = false;

    void Update()
    {
        if (moonbowRenderer == null)
            return;

        if (onlyOnce && hasFired)
            return;

        if (requireMoonbowGameObjectActive && !moonbowRenderer.gameObject.activeInHierarchy)
            return;

        if (!moonbowRenderer.enabled)
            return;

        if (moonbowRenderer.color.a < requiredAlpha)
            return;

        if (targetGameObject != null)
            targetGameObject.SetActive(true);

        if (targetSprite != null)
            targetSprite.enabled = true;

        if (targetDialogueTrigger != null)
        {
            if (alsoSetActiveDialogueObject)
                targetDialogueTrigger.gameObject.SetActive(true);

            targetDialogueTrigger.enabled = false;
            targetDialogueTrigger.enabled = true;
        }

        hasFired = true;
    }
}