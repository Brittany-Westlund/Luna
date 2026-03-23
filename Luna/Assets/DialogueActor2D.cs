using UnityEngine;
using PixelCrushers.DialogueSystem;

[RequireComponent(typeof(Collider2D))]
public class DialogueActor2D : MonoBehaviour
{
    [Header("Identity")]
    public string actorID;

    [Header("Prompt")]
    [Tooltip("Assign the prompt parent object here. It can have no SpriteRenderer itself.")]
    public GameObject promptObject;

    [Header("Input")]
    public KeyCode interactKey = KeyCode.E;
    public string playerTag = "Player";

    [Header("Optional")]
    public Transform conversantOverride;

    [Header("Debug")]
    public bool debugLogging = false;

    private bool playerInRange;
    private Transform playerTransform;
    private LevelDialogueManager dialogueManager;
    private CustomInteractionFeedback customInteractionFeedback;

    private void Start()
    {
        dialogueManager = FindFirstObjectByType<LevelDialogueManager>();

        if (promptObject != null)
        {
            customInteractionFeedback = promptObject.GetComponent<CustomInteractionFeedback>();
        }

        SetPromptVisible(false);
    }

    private void Update()
    {
        if (dialogueManager == null)
        {
            SetPromptVisible(false);
            return;
        }

        if (DialogueManager.isConversationActive)
        {
            SetPromptVisible(false);
            return;
        }

        LevelDialogueManager.PromptState promptState = dialogueManager.GetPromptState(actorID);

        if (!promptState.showPrompt || promptState.entry == null)
        {
            SetPromptVisible(false);
            return;
        }

        // ✅ Apply alpha regardless of range
        if (customInteractionFeedback != null)
        {
            customInteractionFeedback.SetExternalAlphaMultiplier(promptState.alpha);
        }

        // 🔥 NEW LOGIC
        // Bright prompts (alpha == 1) show from anywhere
        // Dim prompts (fallback) require proximity
        bool isBrightPrompt = promptState.alpha >= 0.99f;

        bool shouldShow =
            isBrightPrompt || playerInRange;

        SetPromptVisible(shouldShow);

        // ❗ Interaction still requires proximity
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            Transform conversant = conversantOverride != null ? conversantOverride : transform;

            bool started = dialogueManager.TryStartConversation(actorID, playerTransform, conversant);

            if (debugLogging)
            {
                Debug.Log($"[DialogueActor2D] Interact on '{name}' actorID '{actorID}'. Started: {started}");
            }

            if (started)
            {
                SetPromptVisible(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInRange = true;
        playerTransform = other.transform;

        if (debugLogging)
        {
            Debug.Log($"[DialogueActor2D] Player entered range of '{name}' ({actorID}).");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInRange = false;
        playerTransform = null;

        // ⚠️ DO NOT force-hide here anymore
        // because bright prompts may still need to show

        if (debugLogging)
        {
            Debug.Log($"[DialogueActor2D] Player exited range of '{name}' ({actorID}).");
        }
    }

    private void SetPromptVisible(bool visible)
    {
        if (promptObject != null && promptObject.activeSelf != visible)
        {
            promptObject.SetActive(visible);
        }
    }
}