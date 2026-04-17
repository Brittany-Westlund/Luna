using UnityEngine;
using System.Collections.Generic;

public class MoonbowMoveNearbyByTagOnButterflyMotion : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator sourceAnimator;
    [SerializeField] private Transform butterflyTransform;

    [Header("Tags To Search")]
    [SerializeField] private string moonbowTag = "Moonbow";
    [SerializeField] private string mistTag = "Mist";

    [Tooltip("If true, refresh tagged objects every frame. Use this if moonbows/mist are instantiated at runtime.")]
    [SerializeField] private bool refreshTaggedObjectsEveryFrame = true;

    [Header("Nearby Filter")]
    [SerializeField] private float nearbyRadius = 6f;

    [Header("Trigger Mode")]
    [Tooltip("If true, movement can trigger from Animator.speed. If false, it uses butterfly world movement only.")]
    [SerializeField] private bool allowAnimatorSpeedTrigger = true;

    [Tooltip("Animator.speed must be at or above this to trigger movement.")]
    [SerializeField] private float triggerAnimatorSpeed = 1.15f;

    [Tooltip("If true, movement can trigger from butterfly world-space movement.")]
    [SerializeField] private bool allowWorldMovementTrigger = true;

    [Tooltip("Butterfly world movement speed must be at or above this to trigger movement.")]
    [SerializeField] private float triggerWorldMoveSpeed = 0.05f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private bool onlyMoveOnce = true;
    [SerializeField] private float stopDistance = 0.02f;

    [Header("Startup")]
    [SerializeField] private float startupDelay = 0.1f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private readonly List<MoonbowMistMoveTarget> trackedTargets = new List<MoonbowMistMoveTarget>();

    private bool startupDelayComplete = false;
    private float startupTimer = 0f;
    private Vector3 lastButterflyPosition;
    private float currentWorldMoveSpeed = 0f;

    private void Reset()
    {
        if (butterflyTransform == null)
            butterflyTransform = transform;
    }

    private void Awake()
    {
        if (butterflyTransform == null)
            butterflyTransform = transform;

        lastButterflyPosition = butterflyTransform.position;
        RefreshTrackedTargets();
    }

    private void Update()
    {
        if (butterflyTransform == null)
            return;

        if (refreshTaggedObjectsEveryFrame)
            RefreshTrackedTargets();

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
                lastButterflyPosition = butterflyTransform.position;
                return;
            }
        }

        UpdateButterflyWorldMoveSpeed();

        bool animatorTriggered = false;
        bool movementTriggered = false;

        if (allowAnimatorSpeedTrigger && sourceAnimator != null)
        {
            animatorTriggered = sourceAnimator.speed >= triggerAnimatorSpeed;
        }

        if (allowWorldMovementTrigger)
        {
            movementTriggered = currentWorldMoveSpeed >= triggerWorldMoveSpeed;
        }

        if (debugLogs)
        {
            float animatorSpeedValue = sourceAnimator != null ? sourceAnimator.speed : -1f;
            Debug.Log($"{name}: Animator.speed={animatorSpeedValue}, WorldMoveSpeed={currentWorldMoveSpeed}, animatorTriggered={animatorTriggered}, movementTriggered={movementTriggered}");
        }

        if (!animatorTriggered && !movementTriggered)
            return;

        for (int i = 0; i < trackedTargets.Count; i++)
        {
            MoonbowMistMoveTarget entry = trackedTargets[i];

            if (entry == null || entry.targetPoint == null)
                continue;

            if (onlyMoveOnce && entry.hasFinishedMove)
                continue;

            if (!IsNearby(entry.transform.position))
                continue;

            MoveTowardTarget(entry);
        }
    }

    private void UpdateButterflyWorldMoveSpeed()
    {
        Vector3 currentPosition = butterflyTransform.position;
        float distance = Vector3.Distance(currentPosition, lastButterflyPosition);

        if (Time.deltaTime > 0f)
            currentWorldMoveSpeed = distance / Time.deltaTime;
        else
            currentWorldMoveSpeed = 0f;

        lastButterflyPosition = currentPosition;
    }

    private bool IsNearby(Vector3 objectPosition)
    {
        float distance = Vector3.Distance(butterflyTransform.position, objectPosition);

        if (debugLogs)
            Debug.Log($"{name}: Nearby check distance={distance}, radius={nearbyRadius}");

        return distance <= nearbyRadius;
    }

    private void MoveTowardTarget(MoonbowMistMoveTarget entry)
    {
        Vector3 currentPosition = entry.transform.position;
        Vector3 targetPosition = entry.targetPoint.position;

        Vector3 newPosition = Vector3.MoveTowards(
            currentPosition,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        entry.transform.position = newPosition;

        float distance = Vector3.Distance(entry.transform.position, targetPosition);

        if (distance <= stopDistance)
        {
            entry.transform.position = targetPosition;

            if (!entry.arrivalHandled)
                entry.HandleArrival(debugLogs);

            if (onlyMoveOnce)
            {
                entry.hasFinishedMove = true;

                if (debugLogs)
                    Debug.Log($"{name}: {entry.name} reached target and locked.");
            }
        }
    }

    public void RefreshTrackedTargets()
    {
        trackedTargets.Clear();

        AddTargetsWithTag(moonbowTag);
        AddTargetsWithTag(mistTag);

        if (debugLogs)
            Debug.Log($"{name}: RefreshTrackedTargets found {trackedTargets.Count} tagged move targets.");
    }

    private void AddTargetsWithTag(string tagToSearch)
    {
        if (string.IsNullOrWhiteSpace(tagToSearch))
            return;

        GameObject[] taggedObjects;

        try
        {
            taggedObjects = GameObject.FindGameObjectsWithTag(tagToSearch);
        }
        catch
        {
            if (debugLogs)
                Debug.LogWarning($"{name}: Tag '{tagToSearch}' does not exist in Tag Manager.");
            return;
        }

        for (int i = 0; i < taggedObjects.Length; i++)
        {
            GameObject obj = taggedObjects[i];
            if (obj == null)
                continue;

            MoonbowMistMoveTarget target = obj.GetComponent<MoonbowMistMoveTarget>();
            if (target == null)
                continue;

            if (!trackedTargets.Contains(target))
                trackedTargets.Add(target);
        }
    }

    public void ResetMoveState()
    {
        startupDelayComplete = false;
        startupTimer = 0f;
        currentWorldMoveSpeed = 0f;

        RefreshTrackedTargets();

        for (int i = 0; i < trackedTargets.Count; i++)
        {
            if (trackedTargets[i] != null)
                trackedTargets[i].ResetMoveState(debugLogs);
        }

        if (butterflyTransform != null)
            lastButterflyPosition = butterflyTransform.position;

        if (debugLogs)
            Debug.Log($"{name}: Move state reset.");
    }

    public float GetCurrentAnimatorSpeed()
    {
        if (sourceAnimator == null)
            return 0f;

        return sourceAnimator.speed;
    }

    public float GetCurrentWorldMoveSpeed()
    {
        return currentWorldMoveSpeed;
    }

    private void OnDrawGizmosSelected()
    {
        Transform gizmoCenter = butterflyTransform != null ? butterflyTransform : transform;

        if (gizmoCenter != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(gizmoCenter.position, nearbyRadius);
        }

        Gizmos.color = Color.cyan;

        for (int i = 0; i < trackedTargets.Count; i++)
        {
            MoonbowMistMoveTarget entry = trackedTargets[i];

            if (entry != null && entry.targetPoint != null)
            {
                Gizmos.DrawLine(entry.transform.position, entry.targetPoint.position);
                Gizmos.DrawSphere(entry.targetPoint.position, 0.08f);
            }
        }
    }
}