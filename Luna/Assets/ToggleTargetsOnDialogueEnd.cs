using UnityEngine;
using PixelCrushers.DialogueSystem;

public class ToggleTargetsOnDialogueEnd : MonoBehaviour
{
    [Header("Dialogue To Listen For (Exact Title)")]
    [Tooltip("Leave empty to react to ANY dialogue ending.")]
    public string requiredConversationTitle;

    [Header("Targets To Toggle")]
    public GameObject targetGameObject;
    public Behaviour targetComponent;       // Any component (Collider, Script, etc.)
    public SpriteRenderer targetSprite;

    [Header("Toggle Behavior")]
    [Tooltip("true = enable, false = disable")]
    public bool setActiveState = true;

    public bool onlyOnce = true;
    private bool hasFired = false;

    private void OnEnable()
    {
        DialogueManager.Instance.conversationEnded += OnConversationEnded;
    }

    private void OnDisable()
    {
        if (DialogueManager.HasInstance)
            DialogueManager.Instance.conversationEnded -= OnConversationEnded;
    }

    private void OnConversationEnded(Transform actor)
    {
        if (onlyOnce && hasFired)
            return;

        // ✅ If a specific conversation is required, check LAST conversation
        if (!string.IsNullOrEmpty(requiredConversationTitle))
        {
            string lastConversation = DialogueManager.LastConversationStarted;

            if (lastConversation != requiredConversationTitle)
                return;
        }

        // ✅ Toggle GameObject
        if (targetGameObject != null)
            targetGameObject.SetActive(setActiveState);

        // ✅ Toggle Component
        if (targetComponent != null)
            targetComponent.enabled = setActiveState;

        // ✅ Toggle SpriteRenderer
        if (targetSprite != null)
            targetSprite.enabled = setActiveState;

        hasFired = true;
    }
}
