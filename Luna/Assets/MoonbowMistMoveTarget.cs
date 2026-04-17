using UnityEngine;
using UnityEngine.Events;
using PixelCrushers.DialogueSystem;

public class MoonbowMistMoveTarget : MonoBehaviour
{
    [Header("References")]
    public Transform targetPoint;

    [Header("Target Point Visual Handling")]
    public bool disableTargetObjectOnArrival = false;
    public bool disableTargetSpriteOnly = true;
    public SpriteRenderer targetPointSpriteRenderer;

    [Header("Conversation / Dialogue")]
    public bool setLuaBoolOnArrival = false;
    public string luaBoolName = "";
    public bool luaBoolValue = true;

    public UnityEvent onReachedTarget;

    [HideInInspector] public bool hasFinishedMove = false;
    [HideInInspector] public bool arrivalHandled = false;

    public void HandleArrival(bool debugLogs = false)
    {
        arrivalHandled = true;

        if (setLuaBoolOnArrival)
        {
            if (string.IsNullOrEmpty(luaBoolName))
            {
                Debug.LogWarning($"{name}: setLuaBoolOnArrival is enabled, but luaBoolName is blank.");
            }
            else
            {
                DialogueLua.SetVariable(luaBoolName, luaBoolValue);

                if (debugLogs)
                    Debug.Log($"{name}: DialogueLua bool set: {luaBoolName} = {luaBoolValue}");
            }
        }

        if (onReachedTarget != null)
        {
            onReachedTarget.Invoke();

            if (debugLogs)
                Debug.Log($"{name}: onReachedTarget UnityEvent invoked.");
        }

        if (targetPoint == null)
            return;

        if (disableTargetSpriteOnly)
        {
            if (targetPointSpriteRenderer == null)
                targetPointSpriteRenderer = targetPoint.GetComponent<SpriteRenderer>();

            if (targetPointSpriteRenderer != null && targetPointSpriteRenderer.enabled)
            {
                targetPointSpriteRenderer.enabled = false;

                if (debugLogs)
                    Debug.Log($"{name}: Target sprite renderer disabled.");
            }
        }
        else if (disableTargetObjectOnArrival)
        {
            if (targetPoint.gameObject.activeSelf)
            {
                targetPoint.gameObject.SetActive(false);

                if (debugLogs)
                    Debug.Log($"{name}: Target GameObject disabled.");
            }
        }
    }

    public void ResetMoveState(bool debugLogs = false)
    {
        hasFinishedMove = false;
        arrivalHandled = false;

        if (targetPoint != null)
        {
            if (disableTargetObjectOnArrival && !targetPoint.gameObject.activeSelf)
                targetPoint.gameObject.SetActive(true);

            if (disableTargetSpriteOnly)
            {
                if (targetPointSpriteRenderer == null)
                    targetPointSpriteRenderer = targetPoint.GetComponent<SpriteRenderer>();

                if (targetPointSpriteRenderer != null)
                    targetPointSpriteRenderer.enabled = true;
            }
        }

        if (debugLogs)
            Debug.Log($"{name}: Move state reset.");
    }
}