using UnityEngine;
using UnityEngine.Events;
using PixelCrushers.DialogueSystem;

public class MoonbowMoveOnAnimatorSpeed : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator sourceAnimator;
    [SerializeField] private Transform moonbowToMove;
    [SerializeField] private Transform targetPoint;

    [Header("Animator Speed Logic")]
    [Tooltip("Normal/default animator speed. Unity animators are usually at 1.")]
    [SerializeField] private float baselineAnimatorSpeed = 1f;

    [Tooltip("How close to baseline counts as 'normal' for arming the script.")]
    [SerializeField] private float baselineTolerance = 0.05f;

    [Tooltip("Moonbow movement starts only when animator speed goes above this value.")]
    [SerializeField] private float triggerAnimatorSpeed = 1.15f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;

    [Tooltip("If true, once the moonbow reaches the target, it stays there permanently.")]
    [SerializeField] private bool onlyMoveOnce = true;

    [SerializeField] private float stopDistance = 0.02f;

    [Header("Optional Startup Delay")]
    [Tooltip("Small delay before checking animator speed, just to avoid weird startup timing.")]
    [SerializeField] private float startupDelay = 0.1f;

    [Header("Target Point Visual Handling")]
    [Tooltip("If true, disables the entire target point GameObject when the moonbow arrives.")]
    [SerializeField] private bool disableTargetObjectOnArrival = false;

    [Tooltip("If true, disables only the target point SpriteRenderer when the moonbow arrives.")]
    [SerializeField] private bool disableTargetSpriteOnly = true;

    [Tooltip("Optional explicit SpriteRenderer on the target point. If left empty, the script will try to auto-find one.")]
    [SerializeField] private SpriteRenderer targetPointSpriteRenderer;

    [Header("Conversation / Dialogue")]
    [Tooltip("If true, sets a Dialogue System Lua bool when the moonbow reaches the target.")]
    [SerializeField] private bool setLuaBoolOnArrival = false;

    [Tooltip("Example: ButterflyConversationTerminated")]
    [SerializeField] private string luaBoolName = "";

    [Tooltip("Usually true if you want the convo marked terminated/completed.")]
    [SerializeField] private bool luaBoolValue = true;

    [Tooltip("Optional extra event(s) to fire when the moonbow reaches the target.")]
    [SerializeField] private UnityEvent onReachedTarget;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private bool hasFinishedMove = false;
    private bool isArmed = false;
    private bool startupDelayComplete = false;
    private bool arrivalHandled = false;
    private float startupTimer = 0f;

    private void Reset()
    {
        if (moonbowToMove == null)
            moonbowToMove = transform;
    }

    private void Awake()
    {
        if (moonbowToMove == null)
            moonbowToMove = transform;
    }

    private void Update()
    {
        if (sourceAnimator == null || moonbowToMove == null || targetPoint == null)
            return;

        if (!startupDelayComplete)
        {
            startupTimer += Time.deltaTime;

            if (startupTimer >= startupDelay)
            {
                startupDelayComplete = true;

                if (debugLogs)
                    Debug.Log($"{name}: Startup delay complete.");
            }
            else
            {
                return;
            }
        }

        if (hasFinishedMove && onlyMoveOnce)
            return;

        float currentSpeed = sourceAnimator.speed;

        // Step 1: Arm only after animator has settled at normal speed.
        // This prevents 0 -> 1 from counting as the trigger.
        if (!isArmed)
        {
            bool isAtBaseline =
                currentSpeed >= (baselineAnimatorSpeed - baselineTolerance) &&
                currentSpeed <= (baselineAnimatorSpeed + baselineTolerance);

            if (isAtBaseline)
            {
                isArmed = true;

                if (debugLogs)
                    Debug.Log($"{name}: Armed at normal animator speed {currentSpeed}.");
            }

            return;
        }

        // Step 2: Move only when animator speed is meaningfully above normal.
        if (currentSpeed >= triggerAnimatorSpeed)
        {
            MoveMoonbowTowardTarget();

            if (debugLogs)
                Debug.Log($"{name}: Animator speed {currentSpeed} >= trigger {triggerAnimatorSpeed}. Moving moonbow.");
        }
    }

    private void MoveMoonbowTowardTarget()
    {
        Vector3 currentPosition = moonbowToMove.position;
        Vector3 targetPosition = targetPoint.position;

        Vector3 newPosition = Vector3.MoveTowards(
            currentPosition,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        moonbowToMove.position = newPosition;

        float distance = Vector3.Distance(moonbowToMove.position, targetPosition);

        if (distance <= stopDistance)
        {
            moonbowToMove.position = targetPosition;

            if (!arrivalHandled)
            {
                HandleArrival();
            }

            if (onlyMoveOnce)
            {
                hasFinishedMove = true;

                if (debugLogs)
                    Debug.Log($"{name}: Moonbow reached target and locked.");
            }
        }
    }

    private void HandleArrival()
    {
        arrivalHandled = true;

        if (debugLogs)
            Debug.Log($"{name}: HandleArrival() called.");

        // 1. Terminate / mark conversation state FIRST
        HandleDialogueTermination();

        // 2. Fire any optional UnityEvent hooks
        if (onReachedTarget != null)
        {
            onReachedTarget.Invoke();

            if (debugLogs)
                Debug.Log($"{name}: onReachedTarget UnityEvent invoked.");
        }

        // 3. Then hide target visuals if desired
        HandleTargetPointArrivalVisuals();
    }

    private void HandleDialogueTermination()
    {
        if (!setLuaBoolOnArrival)
            return;

        if (string.IsNullOrEmpty(luaBoolName))
        {
            Debug.LogWarning($"{name}: setLuaBoolOnArrival is enabled, but luaBoolName is blank.");
            return;
        }

        DialogueLua.SetVariable(luaBoolName, luaBoolValue);

        if (debugLogs)
            Debug.Log($"{name}: DialogueLua bool set: {luaBoolName} = {luaBoolValue}");
    }

    private void HandleTargetPointArrivalVisuals()
    {
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

    public void ResetMoveState()
    {
        hasFinishedMove = false;
        isArmed = false;
        startupDelayComplete = false;
        arrivalHandled = false;
        startupTimer = 0f;

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

    public float GetCurrentAnimatorSpeed()
    {
        if (sourceAnimator == null)
            return 0f;

        return sourceAnimator.speed;
    }

    private void OnDrawGizmosSelected()
    {
        if (moonbowToMove != null && targetPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(moonbowToMove.position, targetPoint.position);
            Gizmos.DrawSphere(targetPoint.position, 0.08f);
        }
    }
}