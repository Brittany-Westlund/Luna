using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using PixelCrushers.DialogueSystem;

public class GroupedInteractionProgressHelper : MonoBehaviour
{
    [Header("Required Step IDs")]
    [Tooltip("Each required step must be reported complete exactly once.")]
    [SerializeField] private List<string> requiredStepIDs = new List<string>();

    [Header("Dialogue Unlock")]
    [Tooltip("Optional: enable this DialogueSystemTrigger when all required steps are complete.")]
    [SerializeField] private DialogueSystemTrigger dialogueTriggerOnComplete;

    [Header("Activation On Complete")]
    [Tooltip("Optional: activate this object when all required steps are complete.")]
    [SerializeField] private GameObject activateOnComplete;

    [Header("Optional Lua Bool")]
    [SerializeField] private bool setLuaBoolOnComplete = false;
    [SerializeField] private string luaBoolName = "";
    [SerializeField] private bool luaBoolValue = true;

    [Header("Unity Events")]
    [SerializeField] private UnityEvent onGroupComplete;

    [Header("Behavior")]
    [SerializeField] private bool disableDialogueTriggerAtStart = true;
    [SerializeField] private bool deactivateObjectAtStart = false;
    [SerializeField] private bool allowOnlyCompleteOnce = true;

    [Header("Debug")]
    [SerializeField] private bool logProgress = false;

    private readonly HashSet<string> completedStepIDs = new HashSet<string>();
    private bool groupComplete = false;

    public bool IsGroupComplete => groupComplete;

    private void Start()
    {
        if (dialogueTriggerOnComplete != null && disableDialogueTriggerAtStart)
        {
            dialogueTriggerOnComplete.enabled = false;
        }

        if (activateOnComplete != null && deactivateObjectAtStart)
        {
            activateOnComplete.SetActive(false);
        }

        if (logProgress)
        {
            Debug.Log($"[GroupedInteractionProgressHelper] Started on {name}. Required steps: {requiredStepIDs.Count}");
        }

        CheckForCompletion();
    }

    /// <summary>
    /// Marks a step as complete by ID.
    /// Safe to call multiple times; duplicates are ignored.
    /// </summary>
    public void MarkStepComplete(string stepID)
    {
        if (string.IsNullOrWhiteSpace(stepID))
        {
            if (logProgress)
                Debug.LogWarning($"[GroupedInteractionProgressHelper] Empty stepID passed to MarkStepComplete on {name}.");
            return;
        }

        if (groupComplete && allowOnlyCompleteOnce)
        {
            if (logProgress)
                Debug.Log($"[GroupedInteractionProgressHelper] Ignored step '{stepID}' because group is already complete.");
            return;
        }

        bool wasAdded = completedStepIDs.Add(stepID);

        if (logProgress)
        {
            if (wasAdded)
            {
                Debug.Log($"[GroupedInteractionProgressHelper] Step completed: '{stepID}' ({completedStepIDs.Count}/{requiredStepIDs.Count}) on {name}");
            }
            else
            {
                Debug.Log($"[GroupedInteractionProgressHelper] Step '{stepID}' was already completed on {name}.");
            }
        }

        CheckForCompletion();
    }

    /// <summary>
    /// Optional helper if you want to clear a step during debugging or reversible logic.
    /// </summary>
    public void UnmarkStepComplete(string stepID)
    {
        if (string.IsNullOrWhiteSpace(stepID))
            return;

        if (groupComplete && allowOnlyCompleteOnce)
            return;

        bool wasRemoved = completedStepIDs.Remove(stepID);

        if (logProgress && wasRemoved)
        {
            Debug.Log($"[GroupedInteractionProgressHelper] Step unmarked: '{stepID}' on {name}");
        }
    }

    /// <summary>
    /// Returns true if the specified step is already complete.
    /// </summary>
    public bool IsStepComplete(string stepID)
    {
        return completedStepIDs.Contains(stepID);
    }

    private void CheckForCompletion()
    {
        if (groupComplete && allowOnlyCompleteOnce)
            return;

        if (requiredStepIDs == null || requiredStepIDs.Count == 0)
        {
            if (logProgress)
                Debug.LogWarning($"[GroupedInteractionProgressHelper] No requiredStepIDs assigned on {name}.");
            return;
        }

        for (int i = 0; i < requiredStepIDs.Count; i++)
        {
            string requiredID = requiredStepIDs[i];

            if (string.IsNullOrWhiteSpace(requiredID))
            {
                if (logProgress)
                    Debug.LogWarning($"[GroupedInteractionProgressHelper] Blank required step ID at index {i} on {name}.");
                return;
            }

            if (!completedStepIDs.Contains(requiredID))
            {
                return;
            }
        }

        CompleteGroup();
    }

    private void CompleteGroup()
    {
        if (groupComplete && allowOnlyCompleteOnce)
            return;

        groupComplete = true;

        if (logProgress)
        {
            Debug.Log($"[GroupedInteractionProgressHelper] Group complete on {name}.");
        }

        if (dialogueTriggerOnComplete != null)
        {
            dialogueTriggerOnComplete.gameObject.SetActive(true);
            dialogueTriggerOnComplete.enabled = true;
        }

        if (activateOnComplete != null)
        {
            activateOnComplete.SetActive(true);
        }

        if (setLuaBoolOnComplete && !string.IsNullOrWhiteSpace(luaBoolName))
        {
            DialogueLua.SetVariable(luaBoolName, luaBoolValue);

            if (logProgress)
            {
                Debug.Log($"[GroupedInteractionProgressHelper] Set Lua bool '{luaBoolName}' = {luaBoolValue}");
            }
        }

        onGroupComplete?.Invoke();
    }
}