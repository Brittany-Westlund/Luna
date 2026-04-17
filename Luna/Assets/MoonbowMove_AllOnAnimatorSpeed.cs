using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;

public class MoonbowMoveNearbyOnAnimatorSpeed : MonoBehaviour
{
    [System.Serializable]
    public class MoonbowEntry
    {
        [Header("References")]
        public Transform moonbowToMove;
        public Transform targetPoint;

        [Header("Target Point Visual Handling")]
        [Tooltip("If true, disables the entire target point GameObject when the moonbow arrives.")]
        public bool disableTargetObjectOnArrival = false;

        [Tooltip("If true, disables only the target point SpriteRenderer when the moonbow arrives.")]
        public bool disableTargetSpriteOnly = true;

        [Tooltip("Optional explicit SpriteRenderer on the target point. If left empty, the script will try to auto-find one.")]
        public SpriteRenderer targetPointSpriteRenderer;

        [Header("Conversation / Dialogue")]
        [Tooltip("If true, sets a Dialogue System Lua bool when this moonbow reaches its target.")]
        public bool setLuaBoolOnArrival = false;

        [Tooltip("Example: ButterflyConversationTerminated")]
        public string luaBoolName = "";

        [Tooltip("Usually true if you want the convo marked terminated/completed.")]
        public bool luaBoolValue = true;

        [Tooltip("Optional extra event(s) to fire when this moonbow reaches the target.")]
        public UnityEvent onReachedTarget;

        [HideInInspector] public bool hasFinishedMove = false;
        [HideInInspector] public bool arrivalHandled = false;
    }

    [Header("References")]
    [SerializeField] private Animator sourceAnimator;

    [Tooltip("Usually the butterfly transform. If left empty, this GameObject's transform is used.")]
    [SerializeField] private Transform butterflyTransform;

    [SerializeField] private List<MoonbowEntry> moonbows = new List<MoonbowEntry>();

    [Header("Nearby Filter")]
    [Tooltip("Only moonbows within this distance of the butterfly can move.")]
    [SerializeField] private float nearbyRadius = 6f;

    [Tooltip("If true, distance is checked against the moonbow's current position. If false, distance is checked against the target point.")]
    [SerializeField] private bool useMoonbowPositionForNearbyCheck = true;

    [Header("Animator Speed Logic")]
    [Tooltip("Normal/default animator speed. Unity animators are usually at 1.")]
    [SerializeField] private float baselineAnimatorSpeed = 1f;

    [Tooltip("How close to baseline counts as 'normal' for arming the script.")]
    [SerializeField] private float baselineTolerance = 0.05f;

    [Tooltip("Moonbow movement starts only when animator speed goes above this value.")]
    [SerializeField] private float triggerAnimatorSpeed = 1.15f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;

    [Tooltip("If true, once a moonbow reaches its target, it stays there permanently.")]
    [SerializeField] private bool onlyMoveOnce = true;

    [SerializeField] private float stopDistance = 0.02f;

    [Header("Optional Startup Delay")]
    [Tooltip("Small delay before checking animator speed, just to avoid weird startup timing.")]
    [SerializeField] private float startupDelay = 0.1f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private bool isArmed = false;
    private bool startupDelayComplete = false;
    private float startupTimer = 0f;

    private void Reset()
    {
        if (butterflyTransform == null)
            butterflyTransform = transform;
    }

    private void Awake()
    {
        if (butterflyTransform == null)
            butterflyTransform = transform;
    }

    private void Update()
    {
        if (sourceAnimator == null || butterflyTransform == null || moonbows == null || moonbows.Count == 0)
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
            for (int i = 0; i < moonbows.Count; i++)
            {
                MoonbowEntry entry = moonbows[i];

                if (entry == null || entry.moonbowToMove == null || entry.targetPoint == null)
                    continue;

                if (entry.hasFinishedMove && onlyMoveOnce)
                    continue;

                if (!IsEntryNearby(entry))
                    continue;

                MoveMoonbowTowardTarget(entry);

                if (debugLogs)
                    Debug.Log($"{name}: Animator speed {currentSpeed} >= trigger {triggerAnimatorSpeed}. Moving nearby moonbow: {entry.moonbowToMove.name}");
            }
        }
    }

    private bool IsEntryNearby(MoonbowEntry entry)
    {
        Vector3 referencePosition = useMoonbowPositionForNearbyCheck
            ? entry.moonbowToMove.position
            : entry.targetPoint.position;

        float distance = Vector3.Distance(butterflyTransform.position, referencePosition);

        if (debugLogs)
            Debug.Log($"{name}: Checking nearby for {entry.moonbowToMove.name}. Distance = {distance}, Radius = {nearbyRadius}");

        return distance <= nearbyRadius;
    }

    private void MoveMoonbowTowardTarget(MoonbowEntry entry)
    {
        Vector3 currentPosition = entry.moonbowToMove.position;
        Vector3 targetPosition = entry.targetPoint.position;

        Vector3 newPosition = Vector3.MoveTowards(
            currentPosition,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        entry.moonbowToMove.position = newPosition;

        float distance = Vector3.Distance(entry.moonbowToMove.position, targetPosition);

        if (distance <= stopDistance)
        {
            entry.moonbowToMove.position = targetPosition;

            if (!entry.arrivalHandled)
            {
                HandleArrival(entry);
            }

            if (onlyMoveOnce)
            {
                entry.hasFinishedMove = true;

                if (debugLogs)
                    Debug.Log($"{name}: Moonbow {entry.moonbowToMove.name} reached target and locked.");
            }
        }
    }

    private void HandleArrival(MoonbowEntry entry)
    {
        entry.arrivalHandled = true;

        if (debugLogs)
            Debug.Log($"{name}: HandleArrival() called for {entry.moonbowToMove.name}.");

        HandleDialogueTermination(entry);

        if (entry.onReachedTarget != null)
        {
            entry.onReachedTarget.Invoke();

            if (debugLogs)
                Debug.Log($"{name}: onReachedTarget UnityEvent invoked for {entry.moonbowToMove.name}.");
        }

        HandleTargetPointArrivalVisuals(entry);
    }

    private void HandleDialogueTermination(MoonbowEntry entry)
    {
        if (!entry.setLuaBoolOnArrival)
            return;

        if (string.IsNullOrEmpty(entry.luaBoolName))
        {
            Debug.LogWarning($"{name}: setLuaBoolOnArrival is enabled for {entry.moonbowToMove.name}, but luaBoolName is blank.");
            return;
        }

        DialogueLua.SetVariable(entry.luaBoolName, entry.luaBoolValue);

        if (debugLogs)
            Debug.Log($"{name}: DialogueLua bool set for {entry.moonbowToMove.name}: {entry.luaBoolName} = {entry.luaBoolValue}");
    }

    private void HandleTargetPointArrivalVisuals(MoonbowEntry entry)
    {
        if (entry.targetPoint == null)
            return;

        if (entry.disableTargetSpriteOnly)
        {
            if (entry.targetPointSpriteRenderer == null)
                entry.targetPointSpriteRenderer = entry.targetPoint.GetComponent<SpriteRenderer>();

            if (entry.targetPointSpriteRenderer != null && entry.targetPointSpriteRenderer.enabled)
            {
                entry.targetPointSpriteRenderer.enabled = false;

                if (debugLogs)
                    Debug.Log($"{name}: Target sprite renderer disabled for {entry.moonbowToMove.name}.");
            }
        }
        else if (entry.disableTargetObjectOnArrival)
        {
            if (entry.targetPoint.gameObject.activeSelf)
            {
                entry.targetPoint.gameObject.SetActive(false);

                if (debugLogs)
                    Debug.Log($"{name}: Target GameObject disabled for {entry.moonbowToMove.name}.");
            }
        }
    }

    public void ResetMoveState()
    {
        isArmed = false;
        startupDelayComplete = false;
        startupTimer = 0f;

        for (int i = 0; i < moonbows.Count; i++)
        {
            MoonbowEntry entry = moonbows[i];

            if (entry == null)
                continue;

            entry.hasFinishedMove = false;
            entry.arrivalHandled = false;

            if (entry.targetPoint != null)
            {
                if (entry.disableTargetObjectOnArrival && !entry.targetPoint.gameObject.activeSelf)
                    entry.targetPoint.gameObject.SetActive(true);

                if (entry.disableTargetSpriteOnly)
                {
                    if (entry.targetPointSpriteRenderer == null)
                        entry.targetPointSpriteRenderer = entry.targetPoint.GetComponent<SpriteRenderer>();

                    if (entry.targetPointSpriteRenderer != null)
                        entry.targetPointSpriteRenderer.enabled = true;
                }
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
        Transform gizmoCenter = butterflyTransform != null ? butterflyTransform : transform;

        if (gizmoCenter != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(gizmoCenter.position, nearbyRadius);
        }

        if (moonbows != null)
        {
            Gizmos.color = Color.cyan;

            for (int i = 0; i < moonbows.Count; i++)
            {
                MoonbowEntry entry = moonbows[i];

                if (entry != null && entry.moonbowToMove != null && entry.targetPoint != null)
                {
                    Gizmos.DrawLine(entry.moonbowToMove.position, entry.targetPoint.position);
                    Gizmos.DrawSphere(entry.targetPoint.position, 0.08f);
                }
            }
        }
    }
}