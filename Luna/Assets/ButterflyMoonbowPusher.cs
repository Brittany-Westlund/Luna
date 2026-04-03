using UnityEngine;

public class ButterflyMoonbowPusher : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Butterfly sprite renderer used to determine facing direction.")]
    public SpriteRenderer butterflyRenderer;

    [Tooltip("Butterfly animator.")]
    public Animator butterflyAnimator;

    [Tooltip("Assign the whole Silvermyst root here if you want the entire setup to shift.")]
    public Transform moonbowAnchor;

    [Header("Animator Speed Source")]
    [Tooltip("If true, use an Animator float parameter instead of animator.speed.")]
    public bool useAnimatorFloatParameter = false;

    [Tooltip("Animator float parameter name, if using one.")]
    public string speedParameterName = "Speed";

    [Header("Relative Speed Detection")]
    [Tooltip("If true, captures the butterfly's starting animator speed as its normal baseline.")]
    public bool captureBaselineOnStart = true;

    [Tooltip("If not capturing automatically, this value will be used as the baseline speed.")]
    public float manualBaselineSpeed = 1f;

    [Tooltip("How much faster than baseline the butterfly must get before the moonbow starts moving.")]
    public float speedIncreaseThreshold = 0.25f;

    [Tooltip("How much faster than baseline corresponds to full push distance.")]
    public float fullPushSpeedIncrease = 4f;

    [Header("Push Distance")]
    [Tooltip("Moonbow push distance when there is no qualifying speed increase.")]
    public float minPushDistance = 0f;

    [Tooltip("Moonbow push distance when the butterfly reaches full boosted speed relative to baseline.")]
    public float maxPushDistance = 2f;

    [Header("Movement")]
    [Tooltip("How quickly the moonbow anchor moves toward its target position.")]
    public float moveSpeed = 8f;

    [Header("One-Time Push (Puzzle Mode)")]
    [Tooltip("If true, the moonbow only drifts to its pushed position once, then stays there.")]
    public bool onlyAllowOnce = false;

    [Tooltip("How close the anchor must get to its target before it counts as completed.")]
    public float completionDistance = 0.05f;

    [Header("Persistence")]
    [Tooltip("If true, completed one-time pushes are saved with PlayerPrefs.")]
    public bool saveCompletionState = false;

    [Tooltip("Unique save key for this butterfly/moonbow pair.")]
    public string saveKey = "ButterflyMoonbowPush_01";

    [Header("Debug")]
    public bool debugLogs = false;
    public bool drawGizmos = true;

    private Vector3 moonbowAnchorStartPosition;
    private bool hasCompletedPush = false;
    private float baselineAnimatorSpeed = 1f;

    private string CompletionKey => "BUTTERFLY_MOONBOW_PUSH_" + saveKey;

    private void Awake()
    {
        if (moonbowAnchor != null)
            moonbowAnchorStartPosition = moonbowAnchor.position;

        if (saveCompletionState && onlyAllowOnce)
            LoadCompletionState();
    }

    private void Start()
    {
        if (butterflyRenderer == null)
            butterflyRenderer = GetComponentInChildren<SpriteRenderer>();

        if (butterflyAnimator == null)
            butterflyAnimator = GetComponentInChildren<Animator>();

        baselineAnimatorSpeed = captureBaselineOnStart
            ? GetCurrentAnimatorSpeedValue()
            : manualBaselineSpeed;

        if (debugLogs)
            Debug.Log($"[ButterflyMoonbowPusher] Baseline animator speed set to {baselineAnimatorSpeed}");

        if (moonbowAnchor != null && hasCompletedPush)
        {
            moonbowAnchor.position = GetTargetWorldPosition(maxPushDistance);
        }
    }

    private void Update()
    {
        if (moonbowAnchor == null)
            return;

        if (butterflyRenderer == null)
            return;

        if (butterflyAnimator == null)
            return;

        if (onlyAllowOnce && hasCompletedPush)
            return;

        float currentAnimatorSpeedValue = GetCurrentAnimatorSpeedValue();
        float speedIncrease = currentAnimatorSpeedValue - baselineAnimatorSpeed;

        float normalizedIncrease = NormalizeRelativeIncrease(speedIncrease);
        float currentPushDistance = Mathf.Lerp(minPushDistance, maxPushDistance, normalizedIncrease);

        Vector3 targetWorldPosition = GetTargetWorldPosition(currentPushDistance);

        moonbowAnchor.position = Vector3.Lerp(
            moonbowAnchor.position,
            targetWorldPosition,
            Time.deltaTime * moveSpeed
        );

        if (onlyAllowOnce && normalizedIncrease > 0f)
        {
            float dist = Vector3.Distance(moonbowAnchor.position, targetWorldPosition);

            if (dist <= completionDistance)
            {
                hasCompletedPush = true;
                moonbowAnchor.position = targetWorldPosition;

                if (saveCompletionState)
                    SaveCompletionState();

                if (debugLogs)
                    Debug.Log($"[ButterflyMoonbowPusher] Push completed and locked. SaveKey = {CompletionKey}");
            }
        }

        if (debugLogs)
        {
            Debug.Log(
                $"[ButterflyMoonbowPusher] baseline={baselineAnimatorSpeed}, current={currentAnimatorSpeedValue}, increase={speedIncrease}, normalized={normalizedIncrease}, pushDistance={currentPushDistance}, flipX={butterflyRenderer.flipX}, completed={hasCompletedPush}"
            );
        }
    }

    private float GetCurrentAnimatorSpeedValue()
    {
        if (butterflyAnimator == null)
            return baselineAnimatorSpeed;

        if (useAnimatorFloatParameter && !string.IsNullOrEmpty(speedParameterName))
            return butterflyAnimator.GetFloat(speedParameterName);

        return butterflyAnimator.speed;
    }

    private float NormalizeRelativeIncrease(float speedIncrease)
    {
        if (speedIncrease <= speedIncreaseThreshold)
            return 0f;

        float usableIncrease = speedIncrease - speedIncreaseThreshold;
        float usableRange = Mathf.Max(0.0001f, fullPushSpeedIncrease - speedIncreaseThreshold);

        return Mathf.Clamp01(usableIncrease / usableRange);
    }

    private Vector3 GetTargetWorldPosition(float pushDistance)
    {
        float direction = butterflyRenderer.flipX ? -1f : 1f;
        return moonbowAnchorStartPosition + new Vector3(direction * pushDistance, 0f, 0f);
    }

    private void SaveCompletionState()
    {
        PlayerPrefs.SetInt(CompletionKey, 1);
        PlayerPrefs.Save();
    }

    private void LoadCompletionState()
    {
        hasCompletedPush = PlayerPrefs.GetInt(CompletionKey, 0) == 1;
    }

    public void ResetPushState()
    {
        hasCompletedPush = false;

        if (moonbowAnchor != null)
            moonbowAnchor.position = moonbowAnchorStartPosition;

        if (saveCompletionState)
        {
            PlayerPrefs.DeleteKey(CompletionKey);
            PlayerPrefs.Save();
        }

        if (debugLogs)
            Debug.Log($"[ButterflyMoonbowPusher] Push state reset. SaveKey = {CompletionKey}");
    }

    public void RecalculateBaselineFromCurrentSpeed()
    {
        baselineAnimatorSpeed = GetCurrentAnimatorSpeedValue();

        if (debugLogs)
            Debug.Log($"[ButterflyMoonbowPusher] Baseline recalculated to {baselineAnimatorSpeed}");
    }

    [ContextMenu("DEBUG: Reset Push State")]
    private void DebugResetPushState()
    {
        ResetPushState();
    }

    [ContextMenu("DEBUG: Clear Saved Push State Only")]
    private void DebugClearSavedPushStateOnly()
    {
        if (saveCompletionState)
        {
            PlayerPrefs.DeleteKey(CompletionKey);
            PlayerPrefs.Save();
        }

        hasCompletedPush = false;

        if (debugLogs)
            Debug.Log($"[ButterflyMoonbowPusher] Saved push state cleared only. SaveKey = {CompletionKey}");
    }

    [ContextMenu("DEBUG: Recalculate Baseline From Current Speed")]
    private void DebugRecalculateBaselineFromCurrentSpeed()
    {
        RecalculateBaselineFromCurrentSpeed();
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos || moonbowAnchor == null)
            return;

        SpriteRenderer sr = butterflyRenderer != null ? butterflyRenderer : GetComponentInChildren<SpriteRenderer>();
        Animator anim = butterflyAnimator != null ? butterflyAnimator : GetComponentInChildren<Animator>();

        float baseline = Application.isPlaying
            ? baselineAnimatorSpeed
            : manualBaselineSpeed;

        float currentSpeed = baseline;

        if (anim != null)
        {
            if (useAnimatorFloatParameter && !string.IsNullOrEmpty(speedParameterName))
                currentSpeed = anim.GetFloat(speedParameterName);
            else
                currentSpeed = anim.speed;
        }

        float speedIncrease = currentSpeed - baseline;

        float normalizedIncrease = 0f;
        if (speedIncrease > speedIncreaseThreshold)
        {
            float usableIncrease = speedIncrease - speedIncreaseThreshold;
            float usableRange = Mathf.Max(0.0001f, fullPushSpeedIncrease - speedIncreaseThreshold);
            normalizedIncrease = Mathf.Clamp01(usableIncrease / usableRange);
        }

        float currentPushDistance = Mathf.Lerp(minPushDistance, maxPushDistance, normalizedIncrease);

        float direction = 1f;
        if (sr != null)
            direction = sr.flipX ? -1f : 1f;

        Vector3 basePos = Application.isPlaying && moonbowAnchor != null
            ? moonbowAnchorStartPosition
            : (moonbowAnchor != null ? moonbowAnchor.position : transform.position);

        Vector3 targetWorldPosition = basePos + new Vector3(direction * currentPushDistance, 0f, 0f);
        Vector3 maxTargetWorldPosition = basePos + new Vector3(direction * maxPushDistance, 0f, 0f);

        Gizmos.color = new Color(0.4f, 1f, 1f, 0.6f);
        Gizmos.DrawLine(transform.position, targetWorldPosition);
        Gizmos.DrawWireSphere(targetWorldPosition, 0.18f);

        Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.45f);
        Gizmos.DrawWireSphere(maxTargetWorldPosition, 0.24f);
    }
}