using UnityEngine;

public class InteractionItemReceiver : MonoBehaviour
{
    public enum RequiredItemType
    {
        Spore,
        Light,
        Tea
    }

    [Header("Requirement")]
    [SerializeField] private RequiredItemType requiredItemType = RequiredItemType.Spore;
    [SerializeField] private bool requirePlayerInsideTrigger = true;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool onlySucceedOnce = true;

    [Header("Direct Input")]
    [SerializeField] private bool allowDirectInteractKey = true;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Spore Settings")]
    [SerializeField] private bool consumeSporeOnSuccess = true;
    [SerializeField] private bool useGiveSoundForSpore = true;
    [SerializeField] private bool requireSporeAlreadyHeldOnTriggerEnter = false;

    [Header("External Fulfillment for Light / Tea")]
    [Tooltip("For Light or Tea, another script can call MarkExternalRequirementAvailable(true) before TryInteract().")]
    [SerializeField] private bool useExternalRequirementFlagForLightOrTea = true;

    [Header("Success Visuals - GameObjects")]
    [SerializeField] private GameObject objectToDisableOnSuccess;
    [SerializeField] private GameObject objectToEnableOnSuccess;

    [Header("Success Visuals - SpriteRenderers")]
    [SerializeField] private SpriteRenderer spriteRendererToDisableOnSuccess;
    [SerializeField] private SpriteRenderer spriteRendererToEnableOnSuccess;

    [Header("Optional Progress Reporting")]
    [SerializeField] private InteractionProgressStepReporter stepReporter;

    [Header("Optional Audio")]
    [SerializeField] private AudioSource successSFX;

    [Header("Debug")]
    [SerializeField] private bool logDebug = false;

    private bool playerInside = false;
    private bool hasSucceeded = false;

    private LunaSporeSystem lunaSporeSystem;
    private GameObject currentPlayerRootObject;

    private bool externalRequirementAvailable = false;

    // Spore-entry gating
    private bool hadRequiredSporeOnTriggerEnter = false;
    private bool canAcceptCurrentSporeVisit = false;

    public bool HasSucceeded => hasSucceeded;
    public bool PlayerInside => playerInside;

    private void Update()
    {
        if (!allowDirectInteractKey)
            return;

        if (Input.GetKeyDown(interactKey))
        {
            TryInteract();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInside = true;

        lunaSporeSystem = other.GetComponentInParent<LunaSporeSystem>();
        currentPlayerRootObject = lunaSporeSystem != null ? lunaSporeSystem.gameObject : other.gameObject;

        hadRequiredSporeOnTriggerEnter = false;
        canAcceptCurrentSporeVisit = false;

        if (requiredItemType == RequiredItemType.Spore && lunaSporeSystem != null)
        {
            hadRequiredSporeOnTriggerEnter = lunaSporeSystem.HasSporeAttached;

            if (requireSporeAlreadyHeldOnTriggerEnter)
            {
                canAcceptCurrentSporeVisit = hadRequiredSporeOnTriggerEnter;
            }
            else
            {
                canAcceptCurrentSporeVisit = true;
            }
        }

        if (logDebug)
        {
            Debug.Log(
                $"[{name}] Player entered trigger. " +
                $"other = {other.name}, " +
                $"lunaSporeSystem found = {(lunaSporeSystem != null)}, " +
                $"hadRequiredSporeOnTriggerEnter = {hadRequiredSporeOnTriggerEnter}, " +
                $"canAcceptCurrentSporeVisit = {canAcceptCurrentSporeVisit}"
            );
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;

        LunaSporeSystem exitingSporeSystem = other.GetComponentInParent<LunaSporeSystem>();
        GameObject exitingRootObject = exitingSporeSystem != null
            ? exitingSporeSystem.gameObject
            : other.gameObject;

        if (currentPlayerRootObject == exitingRootObject)
        {
            playerInside = false;
            currentPlayerRootObject = null;
            lunaSporeSystem = null;

            hadRequiredSporeOnTriggerEnter = false;
            canAcceptCurrentSporeVisit = false;
        }

        if (logDebug)
        {
            Debug.Log($"[{name}] Player exited trigger.");
        }
    }

    public void MarkExternalRequirementAvailable(bool isAvailable)
    {
        externalRequirementAvailable = isAvailable;

        if (logDebug)
        {
            Debug.Log($"[{name}] External requirement available set to {externalRequirementAvailable}.");
        }
    }

    public bool CanInteract()
    {
        if (onlySucceedOnce && hasSucceeded)
            return false;

        if (requirePlayerInsideTrigger && !playerInside)
            return false;

        switch (requiredItemType)
        {
            case RequiredItemType.Spore:
                return CheckSporeRequirement();

            case RequiredItemType.Light:
            case RequiredItemType.Tea:
                return CheckExternalRequirement();
        }

        return false;
    }

    public bool TryInteract()
    {
        if (!CanInteract())
        {
            if (logDebug)
            {
                Debug.Log($"[{name}] TryInteract failed. Requirement not met for {requiredItemType}.");
            }

            return false;
        }

        CompleteInteraction();
        return true;
    }

    private bool CheckSporeRequirement()
    {
        if (lunaSporeSystem == null)
            return false;

        if (!lunaSporeSystem.HasSporeAttached)
            return false;

        if (requireSporeAlreadyHeldOnTriggerEnter && !canAcceptCurrentSporeVisit)
            return false;

        return true;
    }

    private bool CheckExternalRequirement()
    {
        if (!useExternalRequirementFlagForLightOrTea)
            return false;

        return externalRequirementAvailable;
    }

    private void CompleteInteraction()
    {
        hasSucceeded = true;

        if (requiredItemType == RequiredItemType.Spore && consumeSporeOnSuccess && lunaSporeSystem != null)
        {
            bool playStoreSoundFallback = !useGiveSoundForSpore || successSFX == null;
            lunaSporeSystem.DestroyAttachedSpore(playStoreSoundFallback);

            if (logDebug)
            {
                Debug.Log($"[{name}] Spore consumed. playStoreSoundFallback = {playStoreSoundFallback}");
            }
        }

        if (requiredItemType == RequiredItemType.Light || requiredItemType == RequiredItemType.Tea)
        {
            externalRequirementAvailable = false;
        }

        if (objectToDisableOnSuccess != null)
        {
            objectToDisableOnSuccess.SetActive(false);
        }

        if (objectToEnableOnSuccess != null)
        {
            objectToEnableOnSuccess.SetActive(true);
        }

        if (spriteRendererToDisableOnSuccess != null)
        {
            spriteRendererToDisableOnSuccess.enabled = false;
        }

        if (spriteRendererToEnableOnSuccess != null)
        {
            spriteRendererToEnableOnSuccess.enabled = true;
        }

        if (stepReporter != null)
        {
            stepReporter.ReportStepComplete();

            if (logDebug)
            {
                Debug.Log($"[{name}] Reported step complete.");
            }
        }

        if (successSFX != null)
        {
            successSFX.Play();
        }

        if (logDebug)
        {
            Debug.Log($"[{name}] Interaction completed successfully for {requiredItemType}.");
        }
    }

    public void ResetSuccessState()
    {
        hasSucceeded = false;
        externalRequirementAvailable = false;
        hadRequiredSporeOnTriggerEnter = false;
        canAcceptCurrentSporeVisit = false;
    }
}