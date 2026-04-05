using UnityEngine;

public class InteractionProgressStepReporter : MonoBehaviour
{
    [Header("Progress Helper")]
    [SerializeField] private GroupedInteractionProgressHelper progressHelper;

    [Header("Step")]
    [SerializeField] private string stepID = "";

    [Header("Reporting")]
    [SerializeField] private bool onlyReportOnce = true;
    [SerializeField] private bool reportOnStart = false;

    [Header("Automatic Watch Conditions")]
    [SerializeField] private bool watchTargetGameObjectActiveState = false;
    [SerializeField] private GameObject targetGameObject;
    [SerializeField] private bool reportWhenTargetBecomesActive = false;
    [SerializeField] private bool reportWhenTargetBecomesInactive = false;

    [Header("SpriteRenderer Enabled Watch")]
    [SerializeField] private bool watchSpriteRendererEnabledState = false;
    [SerializeField] private SpriteRenderer targetSpriteRenderer;
    [SerializeField] private bool reportWhenSpriteRendererEnabled = false;
    [SerializeField] private bool reportWhenSpriteRendererDisabled = false;

    [Header("SpriteRenderer Alpha Watch")]
    [SerializeField] private bool watchSpriteRendererAlpha = false;
    [SerializeField] private bool reportWhenAlphaAtOrAboveThreshold = false;
    [SerializeField] private bool reportWhenAlphaAtOrBelowThreshold = false;
    [SerializeField] [Range(0f, 1f)] private float alphaThreshold = 1f;

    [Header("Debug")]
    [SerializeField] private bool logDebug = false;

    private bool hasReported = false;

    private void Start()
    {
        if (reportOnStart)
        {
            ReportStepComplete();
        }
    }

    private void Update()
    {
        if (onlyReportOnce && hasReported)
            return;

        if (watchTargetGameObjectActiveState && targetGameObject != null)
        {
            if (reportWhenTargetBecomesActive && targetGameObject.activeSelf)
            {
                ReportStepComplete();
                return;
            }

            if (reportWhenTargetBecomesInactive && !targetGameObject.activeSelf)
            {
                ReportStepComplete();
                return;
            }
        }

        if (watchSpriteRendererEnabledState && targetSpriteRenderer != null)
        {
            if (reportWhenSpriteRendererEnabled && targetSpriteRenderer.enabled)
            {
                ReportStepComplete();
                return;
            }

            if (reportWhenSpriteRendererDisabled && !targetSpriteRenderer.enabled)
            {
                ReportStepComplete();
                return;
            }
        }

        if (watchSpriteRendererAlpha && targetSpriteRenderer != null)
        {
            float currentAlpha = targetSpriteRenderer.color.a;

            if (reportWhenAlphaAtOrAboveThreshold && currentAlpha >= alphaThreshold)
            {
                ReportStepComplete();
                return;
            }

            if (reportWhenAlphaAtOrBelowThreshold && currentAlpha <= alphaThreshold)
            {
                ReportStepComplete();
                return;
            }
        }
    }

    public void ReportStepComplete()
    {
        if (onlyReportOnce && hasReported)
            return;

        if (progressHelper == null)
        {
            if (logDebug)
            {
                Debug.LogWarning($"[{name}] InteractionProgressStepReporter could not report because progressHelper is missing.");
            }
            return;
        }

        if (string.IsNullOrWhiteSpace(stepID))
        {
            if (logDebug)
            {
                Debug.LogWarning($"[{name}] InteractionProgressStepReporter could not report because stepID is blank.");
            }
            return;
        }

        progressHelper.MarkStepComplete(stepID);
        hasReported = true;

        if (logDebug)
        {
            Debug.Log($"[{name}] Reported step complete: {stepID}");
        }
    }

    public bool HasReported()
    {
        return hasReported;
    }

    public void ResetReportedState()
    {
        hasReported = false;
    }
}