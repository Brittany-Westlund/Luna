using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CustomInteractionFeedback))]
public class ProximityPromptFade : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Optional: assign PlayerFeet manually. If empty, will auto-find by name.")]
    public Transform playerFeet;

    [Tooltip("Optional collider to measure distance against (ex: GardenSpot trigger).")]
    public Collider2D proximitySourceCollider;

    [Header("Auto Find")]
    public string playerFeetObjectName = "PlayerFeet";

    [Header("Distance Fade")]
    public float fullyVisibleDistance = 0.4f;
    public float fullyFadedDistance = 1.4f;

    [Range(0f, 1f)]
    public float minAlpha = 0f;

    [Range(0f, 1f)]
    public float maxAlpha = 1f;

    public float fadeLerpSpeed = 8f;

    [Header("Behavior")]
    public bool fadeOutIfMissing = true;

    [Tooltip("If true, this helper will also respect CustomInteractionFeedback's dismiss key behavior.")]
    public bool respectCustomInteractionDismiss = true;

    [Header("Debug")]
    public bool debugLogging = false;

    private CustomInteractionFeedback feedback;
    private Collider2D playerFeetCollider;
    private float currentAlpha = 1f;

    private void Awake()
    {
        feedback = GetComponent<CustomInteractionFeedback>();

        if (proximitySourceCollider == null)
            proximitySourceCollider = GetComponentInParent<Collider2D>();

        RebindPlayerFeet();

        currentAlpha = maxAlpha;
        feedback.SetExternalAlphaMultiplier(currentAlpha);
    }

    private void OnEnable()
    {
        RebindPlayerFeet();

        if (feedback == null)
            feedback = GetComponent<CustomInteractionFeedback>();
    }

    private void Update()
    {
        if (playerFeet == null || !playerFeet.gameObject.activeInHierarchy)
            RebindPlayerFeet();

        HandleDismissInput();

        float targetAlpha = GetTargetAlpha();
        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * fadeLerpSpeed);

        if (feedback != null)
            feedback.SetExternalAlphaMultiplier(currentAlpha);

        if (debugLogging)
            Debug.Log($"[ProximityPromptFade] {name} target:{targetAlpha:F2} current:{currentAlpha:F2}");
    }

    private void RebindPlayerFeet()
    {
        if (playerFeet == null)
        {
            GameObject found = GameObject.Find(playerFeetObjectName);
            if (found != null)
                playerFeet = found.transform;
        }

        if (playerFeet != null)
            playerFeetCollider = playerFeet.GetComponent<Collider2D>();
    }

    private void HandleDismissInput()
    {
        if (!respectCustomInteractionDismiss)
            return;

        if (feedback == null)
            return;

        if (!feedback.allowPressEToTurnOff)
            return;

        if (feedback.dismissKeys == null || feedback.dismissKeys.Count == 0)
            return;

        if (feedback.mustPlayerBeInTrigger && !IsPlayerInDismissRange())
            return;

        for (int i = 0; i < feedback.dismissKeys.Count; i++)
        {
            if (Input.GetKeyDown(feedback.dismissKeys[i]))
            {
                if (debugLogging)
                    Debug.Log($"[ProximityPromptFade] Dismiss key '{feedback.dismissKeys[i]}' pressed on {name}");

                feedback.DismissFeedback();
                return;
            }
        }
    }

    private bool IsPlayerInDismissRange()
    {
        if (playerFeetCollider != null && proximitySourceCollider != null)
            return proximitySourceCollider.IsTouching(playerFeetCollider);

        if (playerFeet != null)
            return GetDistance() <= fullyFadedDistance;

        return false;
    }

    private float GetTargetAlpha()
    {
        if (playerFeet == null)
            return fadeOutIfMissing ? minAlpha : maxAlpha;

        float distance = GetDistance();

        if (distance <= fullyVisibleDistance)
            return maxAlpha;

        if (distance >= fullyFadedDistance)
            return minAlpha;

        float t = Mathf.InverseLerp(fullyFadedDistance, fullyVisibleDistance, distance);
        return Mathf.Lerp(minAlpha, maxAlpha, t);
    }

    private float GetDistance()
    {
        if (proximitySourceCollider != null && playerFeetCollider != null)
        {
            Vector2 a = proximitySourceCollider.ClosestPoint(playerFeetCollider.bounds.center);
            Vector2 b = playerFeetCollider.ClosestPoint(a);
            return Vector2.Distance(a, b);
        }

        if (playerFeet != null)
            return Vector2.Distance(transform.position, playerFeet.position);

        return fullyFadedDistance;
    }
}