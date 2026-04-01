using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using PixelCrushers.DialogueSystem;

public class FairyflyAssignedGardensLitWatcher : MonoBehaviour
{
    [Header("Assigned Gardens (match your fairyfly's list)")]
    [SerializeField] private List<Transform> assignedGardens = new List<Transform>();

    [Header("Check Timing")]
    [SerializeField] private bool checkOnStart = true;
    [SerializeField] private bool checkContinuously = true;
    [SerializeField] private float checkInterval = 0.2f;

    [Header("Completion Rules")]
    [SerializeField] private bool lockCompleteOnceTrue = true;

    [Header("Optional Lua Variable")]
    [SerializeField] private bool setLuaVariable = false;
    [SerializeField] private string luaVariableName = "";

    [Header("Optional Unity Events")]
    [SerializeField] private UnityEvent onAllAssignedGardensLit;
    [SerializeField] private UnityEvent onNoLongerAllAssignedGardensLit;

    [Header("Optional Object Toggles")]
    [SerializeField] private GameObject activateWhenComplete;
    [SerializeField] private GameObject deactivateWhenComplete;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private Coroutine monitorRoutine;
    private bool allAssignedGardensLit = false;
    private bool completionEventHasFired = false;

    public bool AreAllAssignedGardensLit => allAssignedGardensLit;
    public bool HasCompleted => completionEventHasFired;

    private void Start()
    {
        if (checkOnStart)
        {
            RefreshState();
        }

        if (checkContinuously)
        {
            monitorRoutine = StartCoroutine(MonitorRoutine());
        }
    }

    private void OnDisable()
    {
        if (monitorRoutine != null)
        {
            StopCoroutine(monitorRoutine);
            monitorRoutine = null;
        }
    }

    public void RefreshState()
    {
        bool evaluatedState = EvaluateAllAssignedGardensLit();
        ApplyEvaluatedState(evaluatedState);
    }

    public void ForceMarkComplete()
    {
        ApplyEvaluatedState(true);
    }

    public void ResetCompletionState()
    {
        completionEventHasFired = false;
        allAssignedGardensLit = false;

        if (setLuaVariable && !string.IsNullOrWhiteSpace(luaVariableName))
        {
            DialogueLua.SetVariable(luaVariableName, false);
        }

        ApplyOptionalTargets(false);

        if (debugLogs)
        {
            Debug.Log($"{name}: ResetCompletionState()");
        }
    }

    private IEnumerator MonitorRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(Mathf.Max(0.01f, checkInterval));

        while (true)
        {
            RefreshState();
            yield return wait;
        }
    }

    private bool EvaluateAllAssignedGardensLit()
    {
        if (lockCompleteOnceTrue && completionEventHasFired)
        {
            return true;
        }

        if (assignedGardens == null || assignedGardens.Count == 0)
        {
            if (debugLogs)
            {
                Debug.Log($"{name}: No assigned gardens.");
            }

            return false;
        }

        for (int i = 0; i < assignedGardens.Count; i++)
        {
            Transform garden = assignedGardens[i];

            if (garden == null)
            {
                if (debugLogs)
                {
                    Debug.Log($"{name}: Assigned garden at index {i} is null.");
                }

                return false;
            }

            if (!IsGardenComplete(garden))
            {
                return false;
            }
        }

        return true;
    }

    private bool IsGardenComplete(Transform garden)
    {
        SproutAndLightManager flower = garden.GetComponentInChildren<SproutAndLightManager>(true);

        if (flower == null)
        {
            if (debugLogs)
            {
                Debug.Log($"{name}: Garden '{garden.name}' has no flower yet.");
            }

            return false;
        }

        if (!flower.IsFullyGrown)
        {
            if (debugLogs)
            {
                Debug.Log($"{name}: Garden '{garden.name}' has a flower, but it is not fully grown yet.");
            }

            return false;
        }

        if (!IsFlowerAlreadyLit(flower))
        {
            if (debugLogs)
            {
                Debug.Log($"{name}: Garden '{garden.name}' has a fully grown flower, but it is not lit yet.");
            }

            return false;
        }

        return true;
    }

    private bool IsFlowerAlreadyLit(SproutAndLightManager flower)
    {
        if (flower == null)
            return false;

        if (flower.litFlowerRenderer != null && flower.litFlowerRenderer.enabled)
            return true;

        Transform litChild = flower.transform.Find("LitFlowerB");
        if (litChild != null && litChild.gameObject.activeSelf)
        {
            SpriteRenderer litSR = litChild.GetComponent<SpriteRenderer>();
            if (litSR == null || litSR.enabled)
                return true;
        }

        return false;
    }

    private void ApplyEvaluatedState(bool newState)
    {
        bool previousState = allAssignedGardensLit;

        if (lockCompleteOnceTrue && completionEventHasFired)
        {
            allAssignedGardensLit = true;
            SetLuaIfNeeded(true);
            ApplyOptionalTargets(true);
            return;
        }

        allAssignedGardensLit = newState;

        SetLuaIfNeeded(allAssignedGardensLit);
        ApplyOptionalTargets(allAssignedGardensLit);

        if (!previousState && allAssignedGardensLit)
        {
            completionEventHasFired = true;

            if (debugLogs)
            {
                Debug.Log($"{name}: All assigned gardens are complete.");
            }

            onAllAssignedGardensLit?.Invoke();
        }
        else if (previousState && !allAssignedGardensLit)
        {
            if (!lockCompleteOnceTrue)
            {
                if (debugLogs)
                {
                    Debug.Log($"{name}: Assigned gardens are no longer all complete.");
                }

                onNoLongerAllAssignedGardensLit?.Invoke();
            }
        }
    }

    private void SetLuaIfNeeded(bool value)
    {
        if (!setLuaVariable)
            return;

        if (string.IsNullOrWhiteSpace(luaVariableName))
            return;

        DialogueLua.SetVariable(luaVariableName, value);

        if (debugLogs)
        {
            Debug.Log($"{name}: Set Lua variable '{luaVariableName}' = {value}");
        }
    }

    private void ApplyOptionalTargets(bool completed)
    {
        if (activateWhenComplete != null)
        {
            activateWhenComplete.SetActive(completed);
        }

        if (deactivateWhenComplete != null)
        {
            deactivateWhenComplete.SetActive(!completed);
        }
    }
}