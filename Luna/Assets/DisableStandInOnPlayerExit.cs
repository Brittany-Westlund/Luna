using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class StandInSwap_TargetScaledLuna : MonoBehaviour
{
    [Header("Stand-In")]
    [SerializeField] private GameObject standInObject;

    [Header("Runtime Object Names")]
    [SerializeField] private string scaledLunaObjectName = "ScaledLuna";
    [SerializeField] private string lunaChildName = "Luna";
    [SerializeField] private string requiredColliderName = "PlayerFeet";

    [Header("Startup")]
    [SerializeField] private bool forceStartInside = true;

    [Header("Timing")]
    [SerializeField] private float exitGraceTime = 0.1f;

    [Header("Visual Control")]
    [SerializeField] private bool hideLunaWhileInside = true;
    [SerializeField] private bool disableLunaAnimatorWhileInside = true;
    [SerializeField] private bool restoreLunaRendererOnSwap = true;
    [SerializeField] private bool restoreLunaAnimatorOnSwap = true;

    [Header("Cleanup")]
    [SerializeField] private bool destroyAfterSwap = true;
    [SerializeField] private float destroyDelay = 0f;

    private Collider2D triggerCollider;
    private Collider2D playerFeetCollider;

    private SpriteRenderer lunaRenderer;
    private Animator lunaAnimator;
    private SpriteRenderer[] standInRenderers;

    private bool hasEverBeenInside = false;
    private bool hasSwapped = false;
    private bool playerInside = false;
    private float outsideTimer = 0f;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider != null)
            triggerCollider.isTrigger = true;
    }

    private void Start()
    {
        CacheReferences();

        if (standInObject != null)
        {
            standInRenderers = standInObject.GetComponentsInChildren<SpriteRenderer>(true);
        }

        if (forceStartInside)
        {
            hasEverBeenInside = true;
            playerInside = true;
            outsideTimer = 0f;
            ApplyInsideVisualState();
            return;
        }

        if (IsPlayerInside())
        {
            hasEverBeenInside = true;
            playerInside = true;
            outsideTimer = 0f;
            ApplyInsideVisualState();
        }
        else
        {
            playerInside = false;
            ApplyOutsideVisualState();
        }
    }

    private void Update()
    {
        if (hasSwapped)
            return;

        RefreshInsideState();

        if (playerInside)
        {
            outsideTimer = 0f;
            return;
        }

        if (!hasEverBeenInside)
            return;

        outsideTimer += Time.deltaTime;

        if (outsideTimer >= exitGraceTime)
        {
            PerformSwap();
        }
    }

    private void LateUpdate()
    {
        if (hasSwapped)
            return;

        if (!playerInside)
            return;

        ApplyInsideVisualState();
    }

    private void RefreshInsideState()
    {
        if (hasSwapped)
            return;

        if (playerFeetCollider == null)
            CacheReferences();

        if (triggerCollider == null || playerFeetCollider == null)
            return;

        bool insideNow = triggerCollider.bounds.Intersects(playerFeetCollider.bounds);

        if (insideNow)
        {
            playerInside = true;
            hasEverBeenInside = true;
            outsideTimer = 0f;
        }
        else
        {
            playerInside = false;
        }
    }

    private void ApplyInsideVisualState()
    {
        SetStandInVisible(true);

        if (hideLunaWhileInside && lunaRenderer != null)
            lunaRenderer.enabled = false;

        if (disableLunaAnimatorWhileInside && lunaAnimator != null)
            lunaAnimator.enabled = false;
    }

    private void ApplyOutsideVisualState()
    {
        SetStandInVisible(false);

        if (restoreLunaRendererOnSwap && lunaRenderer != null)
            lunaRenderer.enabled = true;

        if (restoreLunaAnimatorOnSwap && lunaAnimator != null)
            lunaAnimator.enabled = true;
    }

    private void SetStandInVisible(bool visible)
    {
        if (standInObject == null)
            return;

        if (standInObject.activeSelf != visible)
            standInObject.SetActive(visible);

        if (standInRenderers == null)
            standInRenderers = standInObject.GetComponentsInChildren<SpriteRenderer>(true);

        if (standInRenderers != null)
        {
            for (int i = 0; i < standInRenderers.Length; i++)
            {
                if (standInRenderers[i] != null)
                    standInRenderers[i].enabled = visible;
            }
        }
    }

    private void PerformSwap()
    {
        hasSwapped = true;
        playerInside = false;
        outsideTimer = 0f;

        ApplyOutsideVisualState();

        if (destroyAfterSwap)
            Destroy(gameObject, destroyDelay);
    }

    private void CacheReferences()
    {
        GameObject scaledLuna = GameObject.Find(scaledLunaObjectName);
        if (scaledLuna == null)
        {
            Debug.LogWarning($"{name}: Could not find '{scaledLunaObjectName}'.");
            return;
        }

        Transform playerFeetTransform = FindDeepChild(scaledLuna.transform, requiredColliderName);
        if (playerFeetTransform == null)
        {
            Debug.LogWarning($"{name}: Could not find '{requiredColliderName}'.");
            return;
        }

        playerFeetCollider = playerFeetTransform.GetComponent<Collider2D>();
        if (playerFeetCollider == null)
        {
            Debug.LogWarning($"{name}: '{requiredColliderName}' has no Collider2D.");
            return;
        }

        Transform lunaTransform = FindDeepChild(scaledLuna.transform, lunaChildName);
        if (lunaTransform == null)
        {
            Debug.LogWarning($"{name}: Could not find '{lunaChildName}'.");
            return;
        }

        lunaRenderer = lunaTransform.GetComponent<SpriteRenderer>();
        lunaAnimator = lunaTransform.GetComponent<Animator>();

        if (lunaRenderer == null)
            Debug.LogWarning($"{name}: '{lunaChildName}' has no SpriteRenderer.");

        if (lunaAnimator == null)
            Debug.LogWarning($"{name}: '{lunaChildName}' has no Animator.");
    }

    private Transform FindDeepChild(Transform parent, string targetName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == targetName)
                return child;

            Transform result = FindDeepChild(child, targetName);
            if (result != null)
                return result;
        }

        return null;
    }

    private bool IsPlayerInside()
    {
        if (triggerCollider == null || playerFeetCollider == null)
            return false;

        return triggerCollider.bounds.Intersects(playerFeetCollider.bounds);
    }
}