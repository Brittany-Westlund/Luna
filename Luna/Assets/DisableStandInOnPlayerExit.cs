using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class StandInSwap_TargetScaledLuna : MonoBehaviour
{
    [Header("Stand-In")]
    [SerializeField] private GameObject standInObject;

    [Header("Runtime Object Names")]
    [SerializeField] private string scaledLunaObjectName = "ScaledLuna";
    [SerializeField] private string lunaChildName = "Luna";

    [Header("Timing")]
    [SerializeField] private float exitGraceTime = 0.1f;

    [Header("Cleanup")]
    [SerializeField] private bool destroyAfterSwap = true;
    [SerializeField] private float destroyDelay = 0f;

    private Collider2D triggerCollider;
    private Collider2D playerCollider;
    private SpriteRenderer lunaRenderer;

    private bool hasEverBeenInside = false;
    private bool hasSwapped = false;
    private float outsideTimer = 0f;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    private void Start()
    {
        GameObject scaledLuna = GameObject.Find(scaledLunaObjectName);

        if (scaledLuna == null)
        {
            Debug.LogWarning($"{name}: Could not find GameObject named '{scaledLunaObjectName}'.");
            return;
        }

        playerCollider = scaledLuna.GetComponent<Collider2D>();
        if (playerCollider == null)
        {
            playerCollider = scaledLuna.GetComponentInChildren<Collider2D>(true);
        }

        Transform lunaTransform = FindDeepChild(scaledLuna.transform, lunaChildName);
        if (lunaTransform == null)
        {
            Debug.LogWarning($"{name}: Could not find child '{lunaChildName}' under '{scaledLunaObjectName}'.");
            return;
        }

        lunaRenderer = lunaTransform.GetComponent<SpriteRenderer>();
        if (lunaRenderer == null)
        {
            Debug.LogWarning($"{name}: '{lunaChildName}' does not have a SpriteRenderer.");
            return;
        }

        if (playerCollider == null)
        {
            Debug.LogWarning($"{name}: No Collider2D found on '{scaledLunaObjectName}' or its children.");
            return;
        }

        if (IsPlayerInside())
        {
            hasEverBeenInside = true;
            lunaRenderer.enabled = false;
        }
    }

    private void Update()
    {
        if (hasSwapped) return;
        if (triggerCollider == null || playerCollider == null || lunaRenderer == null) return;

        bool isInside = IsPlayerInside();

        if (isInside)
        {
            hasEverBeenInside = true;
            outsideTimer = 0f;
            lunaRenderer.enabled = false;
            return;
        }

        if (!hasEverBeenInside) return;

        outsideTimer += Time.deltaTime;

        if (outsideTimer >= exitGraceTime)
        {
            PerformSwap();
        }
    }

    private bool IsPlayerInside()
    {
        return triggerCollider.bounds.Intersects(playerCollider.bounds);
    }

    private void PerformSwap()
    {
        hasSwapped = true;

        lunaRenderer.enabled = true;

        if (standInObject != null)
        {
            standInObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"{name}: Stand In Object is not assigned.");
        }

        if (destroyAfterSwap)
        {
            Destroy(gameObject, destroyDelay);
        }
    }

    private Transform FindDeepChild(Transform parent, string targetName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == targetName)
            {
                return child;
            }

            Transform result = FindDeepChild(child, targetName);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}